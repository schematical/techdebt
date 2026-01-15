using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Tilemaps;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.UI; // Added for RawImage

[System.Serializable]
public class TechTreeNode
{
    public string id;
    [System.NonSerialized] public Vector2Int position; // This will now be calculated procedurally
    public List<string> dependencies;
    public bool unlocked;
}

namespace UI
{
    public class MetaUnlockPanel : MonoBehaviour
    {
        public Tilemap connectorTilemap;
        public Tilemap nodeTilemap;
        public GameObject nameLabelPrefab;
        public Tile unlockedTile;
        public Tile lockedTile;
        public TileBase connectorTile;
        public UnityAction<string> onNodeClicked;
                public float panSpeed = 1f;
        
                private List<GameObject> _nameLabels = new List<GameObject>();
        
                // Configuration for procedural layout
                private int columnSpacing = 3; // Horizontal spacing between dependency levels
                private int rowSpacing = 2;    // Vertical spacing between nodes in the same column        

        [System.Serializable]
        public class TechTreeData
        {
            public List<TechTreeNode> nodes;
        }

        private List<TechTreeNode> _techTree;

        private void Awake()
        {
            if (connectorTilemap == null) Debug.LogError("Connector Tilemap reference is NOT set in the Inspector!");
            if (nodeTilemap == null) Debug.LogError("Node Tilemap reference is NOT set in the Inspector!");

            var path = Path.Combine(Application.streamingAssetsPath, "TechTree.json");
            if (File.Exists(path))
            {
                var json = File.ReadAllText(path);
                var data = JsonUtility.FromJson<TechTreeData>(json);
                _techTree = data.nodes;
                CalculateNodePositions();
            }
            else
            {
                Debug.LogError($"TechTree.json not found at path: {path}");
                _techTree = new List<TechTreeNode>();
            }
        }

        private void Start()
        {
            DrawTechTree();
            CenterTilemapOnCamera();
            onNodeClicked += UnlockNode;
        }

        private void CenterTilemapOnCamera()
        {
            if (_techTree == null || _techTree.Count == 0 || Camera.main == null) return;

            // Find the bounds of the entire tech tree in world space
            var minX = _techTree.Min(n => n.position.x);
            var maxX = _techTree.Max(n => n.position.x);
            var minY = _techTree.Min(n => n.position.y);
            var maxY = _techTree.Max(n => n.position.y);

            // The +1 on max is to account for the size of the tile itself
            Vector3 worldCenter = new Vector3(
                (minX + maxX + 1) / 2.0f,
                (minY + maxY + 1) / 2.0f,
                0
            );

            // The Grid's position needs to be adjusted.
            // We want the center of the tree to be at the camera's center (0,0 for a basic setup).
            // The tilemap positions are local to the grid, so moving the grid moves the whole tree.
            var gridTransform = connectorTilemap.transform.parent; // This gets the Grid's transform

            // Assuming the camera is at (0, 0, -10), we want the tree center to be at (0, 0, 0).
            // So we move the grid by the inverse of the tree's calculated center.
            gridTransform.position = -worldCenter;
        }

        #region Procedural Layout Logic

        private void CalculateNodePositions()
        {
            if (_techTree == null || _techTree.Count == 0) return;
            // 1. Calculate depth for each node
            var nodeDepths = new Dictionary<string, int>();
            foreach (var node in _techTree)
            {
                CalculateDepth(node, nodeDepths);
            }

            // 2. Build a map of parent IDs to their children nodes
            var childrenMap = new Dictionary<string, List<TechTreeNode>>();
            foreach (var potentialChild in _techTree)
            {
                if (potentialChild.dependencies == null) continue;
                foreach (var parentId in potentialChild.dependencies)
                {
                    if (!childrenMap.ContainsKey(parentId))
                    {
                        childrenMap[parentId] = new List<TechTreeNode>();
                    }

                    childrenMap[parentId].Add(potentialChild);
                }
            }

            // 3. Group nodes by depth and order from deepest to shallowest for the first pass
            var nodesByDepth = nodeDepths
                .GroupBy(kvp => kvp.Value)
                .OrderByDescending(g => g.Key)
                .ToList();

            var yTrack = new Dictionary<int, int>(); // Tracks next available Y for childless nodes

            // 4. First pass (bottom-up): Position parents based on their children's average Y
            foreach (var group in nodesByDepth)
            {
                var depth = group.Key;
                if (!yTrack.ContainsKey(depth))
                {
                    yTrack[depth] = 0;
                }

                // Process nodes in the current depth group
                foreach (var nodeKvp in group)
                {
                    var node = _techTree.Find(n => n.id == nodeKvp.Key);
                    if (node == null) continue;

                    node.position.x = depth * columnSpacing;

                    if (childrenMap.TryGetValue(node.id, out var children) && children.Count > 0)
                    {
                        // Position this parent in the vertical middle of its children
                        var minY = children.Min(c => c.position.y);
                        var maxY = children.Max(c => c.position.y);
                        double midY = (minY + maxY) / 2.0;
                        node.position.y = Mathf.RoundToInt((float)midY);
                    }
                    else
                    {
                        // Leaf node (no children), stack it vertically
                        node.position.y = yTrack[depth];
                        yTrack[depth] += rowSpacing;
                    }
                }
            }

            // 5. Second pass (top-down): Resolve any collisions
            foreach (var group in nodesByDepth.AsEnumerable().Reverse()) // Iterate ascending depth
            {
                var nodesInGroup = group
                    .Select(kvp => _techTree.Find(n => n.id == kvp.Key))
                    .OrderBy(n => n.position.y)
                    .ToList();

                for (int i = 1; i < nodesInGroup.Count; i++)
                {
                    var prevNode = nodesInGroup[i - 1];
                    var currNode = nodesInGroup[i];

                    if (currNode.position.y < prevNode.position.y + rowSpacing)
                    {
                        int shift = (prevNode.position.y + rowSpacing) - currNode.position.y;

                        // Shift the current node and its entire subtree down to resolve the overlap
                        var queue = new Queue<TechTreeNode>();
                        queue.Enqueue(currNode);
                        var visited = new HashSet<string> { currNode.id };

                        while (queue.Count > 0)
                        {
                            var nodeToShift = queue.Dequeue();
                            nodeToShift.position.y += shift;

                            if (childrenMap.TryGetValue(nodeToShift.id, out var children))
                            {
                                foreach (var child in children)
                                {
                                    if (!visited.Contains(child.id))
                                    {
                                        queue.Enqueue(child);
                                        visited.Add(child.id);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // 6. Final pass: Center the entire tree vertically around Y=0
            if (_techTree.Count > 0)
            {
                var minY = _techTree.Min(n => n.position.y);
                var maxY = _techTree.Max(n => n.position.y);
                var midY = (minY + maxY) / 2;
                foreach (var node in _techTree)
                {
                    node.position.y -= midY;
                }
            }
        }

        private int CalculateDepth(TechTreeNode node, Dictionary<string, int> memo)
        {
            if (memo.ContainsKey(node.id))
            {
                return memo[node.id];
            }

            if (node.dependencies == null || node.dependencies.Count == 0)
            {
                memo[node.id] = 0;
                return 0;
            }

            int maxDependencyDepth = 0;
            foreach (var dependencyId in node.dependencies)
            {
                var dependencyNode = _techTree.Find(n => n.id == dependencyId);
                if (dependencyNode != null)
                {
                    int dependencyDepth = CalculateDepth(dependencyNode, memo);
                    if (dependencyDepth > maxDependencyDepth)
                    {
                        maxDependencyDepth = dependencyDepth;
                    }
                }
            }

            int currentDepth = maxDependencyDepth + 1;
            memo[node.id] = currentDepth;
            return currentDepth;
        }

        #endregion

        public void UnlockNode(string nodeId)
        {
            var node = _techTree.Find(n => n.id == nodeId);
            if (node != null && !node.unlocked)
            {
                if (node.dependencies.All(depId => _techTree.Find(n => n.id == depId)?.unlocked ?? true))
                {
                    node.unlocked = true;
                    DrawTechTree();
                }
            }
        }

        private void Update()
        {
            // Left-click detection for unlocking nodes
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                Vector2 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
                var cellPosition = nodeTilemap.WorldToCell(mouseWorldPosition);
                var clickedNode = _techTree.Find(n => n.position == (Vector2Int)cellPosition);

                if (clickedNode != null)
                {
                    Debug.Log($"Clicked on technology node: {clickedNode.id}");
                    onNodeClicked?.Invoke(clickedNode.id);
                }
            }

            // Right-click and drag for horizontal panning
            if (Mouse.current.rightButton.isPressed)
            {
                Vector2 mouseDelta = Mouse.current.delta.ReadValue();
                if (Mathf.Abs(mouseDelta.x) > 0.01f) // Add a small deadzone
                {
                    var gridTransform = connectorTilemap.transform.parent;

                    // Convert mouse delta from screen space to world space
                    float scaleFactor = (Camera.main.orthographicSize * 2) / Screen.height;

                    // Apply the movement (adding to make the drag feel natural)
                    gridTransform.position += new Vector3(mouseDelta.x * scaleFactor * panSpeed, 0, 0);
                }
            }
        }

        private void DrawTechTree()
        {
            if (_techTree == null || _techTree.Count == 0 || connectorTilemap == null || nodeTilemap == null) return;

            connectorTilemap.ClearAllTiles();
            nodeTilemap.ClearAllTiles();

            // 1. Draw paths from dependencies TO nodes on the connector tilemap
            foreach (var node in _techTree)
            {
                if (node.dependencies == null || node.dependencies.Count == 0) continue;

                var dependencyNodes = node.dependencies
                    .Select(depId => _techTree.Find(n => n.id == depId))
                    .Where(n => n != null)
                    .ToList();

                if (dependencyNodes.Count == 0) continue;

                int maxDepX = dependencyNodes.Max(d => d.position.x);
                int busX = maxDepX + 1;

                for (int x = busX; x <= node.position.x; x++)
                {
                    connectorTilemap.SetTile(new Vector3Int(x, node.position.y, 0), connectorTile);
                }

                var minDepY = dependencyNodes.Min(d => d.position.y);
                var maxDepY = dependencyNodes.Max(d => d.position.y);
                var busMinY = Mathf.Min(minDepY, node.position.y);
                var busMaxY = Mathf.Max(maxDepY, node.position.y);

                for (int y = busMinY; y <= busMaxY; y++)
                {
                    connectorTilemap.SetTile(new Vector3Int(busX, y, 0), connectorTile);
                }

                foreach (var dep in dependencyNodes)
                {
                    for (int x = dep.position.x; x <= busX; x++)
                    {
                        connectorTilemap.SetTile(new Vector3Int(x, dep.position.y, 0), connectorTile);
                    }
                }
            }

            // 2. Draw main technology nodes on top of the paths on the node tilemap
            foreach (var node in _techTree)
            {
                var tile = node.unlocked ? unlockedTile : lockedTile;
                nodeTilemap.SetTile((Vector3Int)node.position, tile);
            }
            
            // 3. Instantiate name labels for each node
            if (nameLabelPrefab != null)
            {
                // Clear any existing labels first
                foreach (var label in _nameLabels)
                {
                    Destroy(label);
                }
                _nameLabels.Clear();

                foreach (var node in _techTree)
                {
                    // Get the world position of the center of the tile
                    Vector3 worldPos = nodeTilemap.GetCellCenterWorld((Vector3Int)node.position);
                    
                    // Instantiate the prefab and position it above the node, parenting it to the Grid
                    GameObject labelInstance = Instantiate(nameLabelPrefab, worldPos + new Vector3(0, -1.5f, 0), Quaternion.identity, nodeTilemap.transform.parent);
                    
                    // Set the text (assuming a TextMeshPro or UI Text component is on the prefab)
                    var textComponent = labelInstance.GetComponentInChildren<TMPro.TextMeshPro>();
                    if (textComponent != null)
                    {
                        textComponent.text = node.id;
                    }
                    else
                    {
                        var uiTextComponent = labelInstance.GetComponentInChildren<UnityEngine.UI.Text>();
                        if(uiTextComponent != null)
                            uiTextComponent.text = node.id;
                    }
                    
                    _nameLabels.Add(labelInstance);
                }
            }

            // Force both tilemaps to re-evaluate tiles and apply rules.
            connectorTilemap.RefreshAllTiles();
            nodeTilemap.RefreshAllTiles();
        }
    }
}