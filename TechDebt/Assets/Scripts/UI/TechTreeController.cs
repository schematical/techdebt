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

        // Event for when a node is clicked
        public UnityAction<string> onNodeClicked;

        // Internal state
        private List<TechTreeNode> _techTree;
        private List<GameObject> _nameLabels = new List<GameObject>();

        // Configuration for procedural layout
        private int columnSpacing = 3;
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
            DrawTechTree();
            CenterTilemapOnCamera();
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
            
            // Right-click and drag for horizontal panning
            if (Mouse.current.rightButton.isPressed)
            {
                Vector2 mouseDelta = Mouse.current.delta.ReadValue();
                if (Mathf.Abs(mouseDelta.x) > 0.01f)
                {
                    var gridTransform = connectorTilemap.transform.parent;
                    float scaleFactor = (Camera.main.orthographicSize * 2) / Screen.height;
                    gridTransform.position += new Vector3(mouseDelta.x * scaleFactor * panSpeed, 0, 0);
                }
            }
        }

        private void DrawTechTree()
        {
            if (_techTree == null || connectorTilemap == null || nodeTilemap == null) return;

            connectorTilemap.ClearAllTiles();
            nodeTilemap.ClearAllTiles();
            
            foreach (var label in _nameLabels)
            {
                Destroy(label);
            }
            _nameLabels.Clear();

            // 1. Draw paths
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

            // 2. Draw nodes and labels
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
                        textComponent.text = node.id;
                    }
                    _nameLabels.Add(labelInstance);
                }
            }

            connectorTilemap.RefreshAllTiles();
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

            // Pass 1: Initial Positioning (Child-centric)
            // Determine column for each node based on dependencies. Roots are column 0.
            foreach (var node in _techTree)
            {
                if (node.dependencies == null || node.dependencies.Count == 0)
                {
                    node.position.x = 0; // Root node
                }
                else
                {
                    int maxDepX = node.dependencies
                        .Select(depId => _techTree.Find(n => n.id == depId))
                        .Where(n => n != null)
                        .Max(n => n.position.x);
                    node.position.x = maxDepX + columnSpacing;
                }
            }

            // Group nodes by column for Y positioning
            var nodesByColumn = _techTree.GroupBy(n => n.position.x)
                                         .OrderBy(g => g.Key)
                                         .ToDictionary(g => g.Key, g => g.ToList());

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
                    // Else, if it has no children, its Y position will be determined by overlap resolution
                }
            }

            // Pass 2: Overlap Resolution
            // Iterate through columns and resolve Y-position overlaps.
            foreach (var col in nodesByColumn.Keys)
            {
                var columnNodes = nodesByColumn[col].OrderBy(n => n.position.y).ToList();
                for (int j = 1; j < columnNodes.Count; j++)
                {
                    var prevNode = columnNodes[j - 1];
                    var currNode = columnNodes[j];

                    // If nodes overlap or are too close, shift the current node down
                    if (currNode.position.y < prevNode.position.y + rowSpacing)
                    {
                        currNode.position.y = prevNode.position.y + rowSpacing;
                    }
                }
            }
        }
    }
}
