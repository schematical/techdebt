using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

namespace UI
{
    public class TechTreeController : MonoBehaviour
    {
        // Public fields for Unity Inspector assignments
        public Tilemap connectorTilemap;
        public Tilemap nodeTilemap;
        public GameObject nameLabelPrefab;
        public Tile unlockedTile;
        public Tile lockedTile;
        public TileBase connectorTile;
        public float panSpeed = 1f;
        public float zoomSpeed = 1f;
        public float minZoom = 5f;
        public float maxZoom = 20f;

        // Event for when a node is clicked
        public UnityAction<string> onNodeClicked;

        // Internal state
        private List<TechTreeNode> _techTree;
        private List<GameObject> _nameLabels = new List<GameObject>();
        private TechTreeNode _hoveredNode = null;
        private bool _initialSetupComplete = false;

        // Configuration for procedural layout
        private int columnSpacing = 4;
        private int rowSpacing = 2;

        public void Initialize(List<TechTreeNode> techTreeNodes)
        {
            if (techTreeNodes == null)
            {
                Debug.LogError("TechTree initialization failed: provided list of nodes is null.");
                return;
            }

            _techTree = techTreeNodes;
            
            CalculateNodePositions();
            DrawNodesAndLabels();
            
            if (!_initialSetupComplete)
            {
                CenterTilemapOnCamera();
                _initialSetupComplete = true;
            }
        }

        private void Update()
        {
            // Left-click detection
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                Vector2 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
                var cellPosition = nodeTilemap.WorldToCell(mouseWorldPosition);
                var clickedNode = _techTree.Find(n => n.position == (Vector2Int)cellPosition);

                if (clickedNode != null)
                {
                    onNodeClicked?.Invoke(clickedNode.id);
                }
            }
            
            // Right-click and drag for panning (horizontal and vertical)
            if (Mouse.current.rightButton.isPressed)
            {
                Vector2 mouseDelta = Mouse.current.delta.ReadValue();
                if (mouseDelta.magnitude > 0.01f)
                {
                    var gridTransform = connectorTilemap.transform.parent;
                    float scaleFactor = (Camera.main.orthographicSize * 2) / Screen.height;
                    gridTransform.position += new Vector3(mouseDelta.x * scaleFactor * panSpeed, mouseDelta.y * scaleFactor * panSpeed, 0);
                }
            }
            
            // Zoom with mouse scroll wheel
            float scroll = Mouse.current.scroll.ReadValue().y;
            if (Mathf.Abs(scroll) > 0.1f)
            {}
            float newSize = Camera.main.orthographicSize - scroll * zoomSpeed;
            Camera.main.orthographicSize = Mathf.Clamp(newSize, minZoom, maxZoom);
            
            UpdateLineageView();
        }

        private void UpdateLineageView()
        {
            Vector2 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            var cellPosition = nodeTilemap.WorldToCell(mouseWorldPosition);
            var currentNode = _techTree.Find(n => n.position == (Vector2Int)cellPosition);

            if (currentNode != _hoveredNode)
            {
                connectorTilemap.ClearAllTiles();
                
                if (currentNode != null)
                {
                    // Draw paths to dependencies (parents)
                    DrawPaths(new List<TechTreeNode> { currentNode });

                    // Find and draw paths to dependents (children)
                    var children = _techTree.Where(n => n.dependencies != null && n.dependencies.Contains(currentNode.id)).ToList();
                    DrawPaths(children);
                }
                
                _hoveredNode = currentNode;
            }
        }
        
        private void DrawPaths(List<TechTreeNode> nodesToDraw)
        {
            foreach (var node in nodesToDraw)
            {
                if (node.dependencies == null || node.dependencies.Count == 0) continue;

                var dependencyNodes = node.dependencies
                    .Select(depId => _techTree.Find(n => n.id == depId))
                    .Where(n => n != null).ToList();

                if (dependencyNodes.Count == 0) continue;

                int maxDepX = dependencyNodes.Max(d => d.position.x);
                int busX = maxDepX + 2; // Create a 1-tile horizontal space before the vertical bus

                // Path from child node to bus
                for (int x = busX; x <= node.position.x; x++)
                {
                    connectorTilemap.SetTile(new Vector3Int(x, node.position.y, 0), connectorTile);
                }

                // Vertical bus itself
                var minDepY = dependencyNodes.Min(d => d.position.y);
                var maxDepY = dependencyNodes.Max(d => d.position.y);
                var busMinY = Mathf.Min(minDepY, node.position.y);
                var busMaxY = Mathf.Max(maxDepY, node.position.y);
                for (int y = busMinY; y <= busMaxY; y++)
                {
                    connectorTilemap.SetTile(new Vector3Int(busX, y, 0), connectorTile);
                }

                // Paths from each dependency to the bus
                foreach (var dep in dependencyNodes)
                {
                    for (int x = dep.position.x; x <= busX; x++)
                    {
                        connectorTilemap.SetTile(new Vector3Int(x, dep.position.y, 0), connectorTile);
                    }
                }
            }
        }

        private void DrawNodesAndLabels()
        {
            if (_techTree == null || nodeTilemap == null) return;

            nodeTilemap.ClearAllTiles();
            foreach (var label in _nameLabels)
            {
                Destroy(label);
            }
            _nameLabels.Clear();

            // Draw nodes and labels
            foreach (var node in _techTree)
            {
                var tile = node.unlocked ? unlockedTile : lockedTile;
                nodeTilemap.SetTile((Vector3Int)node.position, tile);

                if (nameLabelPrefab != null)
                {
                    Vector3 worldPos = nodeTilemap.GetCellCenterWorld((Vector3Int)node.position);
                    GameObject labelInstance = Instantiate(nameLabelPrefab, worldPos + new Vector3(0, -1.5f, 0), Quaternion.identity, nodeTilemap.transform.parent);
                    var textComponent = labelInstance.GetComponentInChildren<TMPro.TextMeshPro>();
                    if (textComponent != null)
                    {
                        textComponent.text = node.DisplayName; // Use DisplayName now
                    }
                    _nameLabels.Add(labelInstance);
                }
            }
            nodeTilemap.RefreshAllTiles();
        }

        private void CenterTilemapOnCamera()
        {
            if (_techTree == null || _techTree.Count == 0 || Camera.main == null) return;

            var minX = _techTree.Min(n => n.position.x);
            var maxX = _techTree.Max(n => n.position.x);
            var minY = _techTree.Min(n => n.position.y);
            var maxY = _techTree.Max(n => n.position.y);

            Vector3 worldCenter = new Vector3((minX + maxX + 1) / 2.0f, (minY + maxY + 1) / 2.0f, 0);
            var gridTransform = connectorTilemap.transform.parent;
            gridTransform.position = -worldCenter;
        }
        
        private void CalculateNodePositions()
        {
            if (_techTree == null || _techTree.Count == 0) return;

            // Pass 1: Determine X position (column) for each node.
            // This is a recursive calculation to ensure nodes are always to the right of their dependencies.
            foreach (var node in _techTree)
            {
                // This will recursively calculate and cache positions
                CalculateNodeXPosition(node);
            }

            // Group nodes by column for Y positioning
            var nodesByColumn = _techTree.GroupBy(n => n.position.x)
                                         .OrderBy(g => g.Key)
                                         .ToDictionary(g => g.Key, g => g.ToList());
            
            // Pass 2: Position Y values, starting with leaf nodes.
            // A "leaf" node is one that is not a dependency for any other node.
            var allDependencyIds = new HashSet<string>(_techTree.SelectMany(n => n.dependencies ?? new List<string>()));
            var leafNodes = _techTree.Where(n => !allDependencyIds.Contains(n.id)).OrderBy(n => n.position.x).ToList();

            int currentY = 0;
            foreach (var node in leafNodes)
            {
                node.position.y = currentY;
                currentY += rowSpacing * 2; // Increase spacing for leaf nodes to spread out branches
            }

            // Position Y values from right to left, centering parents over children
            for (int i = nodesByColumn.Keys.Max(); i >= 0; i--)
            {
                if (!nodesByColumn.ContainsKey(i)) continue;
                
                foreach (var node in nodesByColumn[i])
                {
                    var children = _techTree.Where(n => n.dependencies != null && n.dependencies.Contains(node.id)).ToList();
                    if (children.Count > 0)
                    {
                        float avgChildY = (float)children.Average(c => c.position.y);
                        node.position.y = Mathf.RoundToInt(avgChildY);
                    }
                }
            }

            // Pass 3: Overlap Resolution
            // Iterate through columns and resolve Y-position overlaps.
            foreach (var col in nodesByColumn.Keys)
            {
                var columnNodes = nodesByColumn[col].OrderBy(n => n.position.y).ToList();
                for (int j = 1; j < columnNodes.Count; j++)
                {
                    var prevNode = columnNodes[j - 1];
                    var currNode = columnNodes[j];
                    
                    if (currNode.position.y < prevNode.position.y + rowSpacing)
                    {
                        currNode.position.y = prevNode.position.y + rowSpacing;
                    }
                }
            }
        }
        
        private int CalculateNodeXPosition(TechTreeNode node)
        {
            // If position is already calculated (x > 0 or it's a root), return it.
            if (node.position.x > 0 || (node.dependencies == null || node.dependencies.Count == 0))
            {
                return node.position.x;
            }

            // Find the maximum X position among all dependencies.
            int maxDepX = 0;
            if (node.dependencies != null && node.dependencies.Count > 0)
            {
                maxDepX = node.dependencies
                    .Select(depId => _techTree.Find(n => n.id == depId))
                    .Where(n => n != null)
                    .Max(CalculateNodeXPosition); // Recursive call
            }

            node.position.x = maxDepX + columnSpacing;
            return node.position.x;
        }
    }
}
