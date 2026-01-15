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
    [System.NonSerialized]
    public Vector2Int position; // This will now be calculated procedurally
    public List<string> dependencies;
    public bool unlocked;
}

namespace UI
{
    public class MetaUnlockPanel : MonoBehaviour, IDragHandler, IScrollHandler
    {
        public Tilemap connectorTilemap;
        public Tilemap nodeTilemap;
        public Tile unlockedTile;
        public Tile lockedTile;
        public TileBase connectorTile;
        public UnityAction<string> onNodeClicked;

        public Camera techTreeCamera;
        public float panSpeed = 0.01f;
        public float zoomSpeed = 0.5f;
        public float minZoom = 1f;
        public float maxZoom = 10f;

        // Configuration for procedural layout
        private int columnSpacing = 2; // Horizontal spacing between dependency levels
        private int rowSpacing = 2;    // Vertical spacing between nodes in the same column

        [System.Serializable]
        public class TechTreeData
        {
            public List<TechTreeNode> nodes;
        }

        private List<TechTreeNode> _techTree;
        private Canvas _parentCanvas;
        private Camera _uiCamera;

        private void Awake()
        {
            Debug.Log("MetaUnlockPanel.Awake() called.");

            if (connectorTilemap == null) Debug.LogError("Connector Tilemap reference is NOT set in the Inspector!");
            if (nodeTilemap == null) Debug.LogError("Node Tilemap reference is NOT set in the Inspector!");
            if (techTreeCamera == null) Debug.LogError("TechTreeCamera reference is NOT set in the Inspector!");

            var path = Path.Combine(Application.streamingAssetsPath, "TechTree.json");
            if (File.Exists(path))
            {
                var json = File.ReadAllText(path);
                var data = JsonUtility.FromJson<TechTreeData>(json);
                _techTree = data.nodes;
                Debug.Log($"Successfully loaded {_techTree.Count} nodes from TechTree.json.");
                
                // NEW: Procedurally calculate all node positions
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
            Debug.Log("MetaUnlockPanel.Start() called.");

            // Find the parent canvas and its camera to correctly handle UI clicks
            _parentCanvas = GetComponentInParent<Canvas>();
            if (_parentCanvas != null && _parentCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                _uiCamera = _parentCanvas.worldCamera;
            }
            else
            {
                _uiCamera = null; // This is the correct value for ScreenSpaceOverlay canvases
            }
            
            FitCameraToRawImage();
            DrawTechTree();
            onNodeClicked += UnlockNode;
        }

        private void FitCameraToRawImage()
        {
            if (techTreeCamera == null) return;
            var rawImage = GetComponentInChildren<RawImage>();
            if (rawImage == null) return;

            var rawImageRect = rawImage.rectTransform.rect;
            
            // For an orthographic camera, the size is half the vertical height of the area it's viewing.
            // We want the camera's view to match the RawImage's height.
            // The grid/tilemap cell size must also be considered. Let's assume default pixels per unit of 100.
            float pixelsPerUnit = 100f; // This should match your project's tile settings
            techTreeCamera.orthographicSize = rawImageRect.height / (2f * pixelsPerUnit);
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
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                if (techTreeCamera == null) return;
                
                Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
                Debug.Log($"[Click Debug] Mouse Screen Position: {mouseScreenPos}");

                RectTransform rawImageRect = GetComponentInChildren<RawImage>().rectTransform;
                if (!RectTransformUtility.RectangleContainsScreenPoint(rawImageRect, mouseScreenPos, _uiCamera))
                {
                    return; 
                }

                Vector2 localPoint;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(rawImageRect, mouseScreenPos, _uiCamera, out localPoint);
                Debug.Log($"[Click Debug] Converted Local Point in Rect: {localPoint}");
                
                Vector2 viewportPoint = new Vector2(
                    (localPoint.x / rawImageRect.rect.width) + rawImageRect.pivot.x, 
                    (localPoint.y / rawImageRect.rect.height) + rawImageRect.pivot.y);
                Debug.Log($"[Click Debug] Calculated Viewport Point: {viewportPoint}");

                var mouseWorldPosition = techTreeCamera.ViewportToWorldPoint(viewportPoint);
                Debug.Log($"[Click Debug] Calculated World Position: {mouseWorldPosition}");

                var cellPosition = nodeTilemap.WorldToCell(mouseWorldPosition);
                Debug.Log($"[Click Debug] Calculated Tilemap Cell Position: {cellPosition}");

                var clickedNode = _techTree.Find(n => n.position == (Vector2Int)cellPosition);

                if (clickedNode != null)
                {
                    Debug.Log($"[Click Debug] SUCCESS: Found node '{clickedNode.id}' at cell {cellPosition}");
                    onNodeClicked?.Invoke(clickedNode.id);
                }
                else
                {
                    Debug.Log($"[Click Debug] FAILED: No node found at cell {cellPosition}");
                }
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (techTreeCamera == null) return;
            float scaleFactor = techTreeCamera.orthographicSize * 2 / Screen.height;
            techTreeCamera.transform.position -= new Vector3(eventData.delta.x * scaleFactor, eventData.delta.y * scaleFactor, 0);
        }

        public void OnScroll(PointerEventData eventData)
        {
            if (techTreeCamera == null) return;
            var scroll = eventData.scrollDelta.y;
            float newSize = techTreeCamera.orthographicSize - scroll * zoomSpeed;
            techTreeCamera.orthographicSize = Mathf.Clamp(newSize, minZoom, maxZoom);
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

            // Force both tilemaps to re-evaluate tiles and apply rules.
            connectorTilemap.RefreshAllTiles();
            nodeTilemap.RefreshAllTiles();
        }
    }
}