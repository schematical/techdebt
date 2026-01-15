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
    public Vector2Int position; // This will now be calculated procedurally
    public List<string> dependencies;
    public bool unlocked;
}

namespace UI
{
    public class MetaUnlockPanel : MonoBehaviour, IDragHandler, IScrollHandler
    {
        public Tilemap tilemap;
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
        public int columnSpacing = 5; // Horizontal spacing between dependency levels
        public int rowSpacing = 2;    // Vertical spacing between nodes in the same column

        [System.Serializable]
        public class TechTreeData
        {
            public List<TechTreeNode> nodes;
        }

        private List<TechTreeNode> _techTree;

        private void Awake()
        {
            Debug.Log("MetaUnlockPanel.Awake() called.");

            if (tilemap == null) Debug.LogError("Tilemap reference is NOT set in the Inspector!");
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
            DrawTechTree();
            onNodeClicked += UnlockNode;
        }
        
        #region Procedural Layout Logic

        private void CalculateNodePositions()
        {
            if (_techTree == null || _techTree.Count == 0) return;

            var nodeDepths = new Dictionary<string, int>();
            foreach (var node in _techTree)
            {
                CalculateDepth(node, nodeDepths);
            }

            var nodesByDepth = nodeDepths
                .GroupBy(kvp => kvp.Value) // Group nodes by their depth
                .OrderBy(g => g.Key)       // Order groups by depth (0, 1, 2...)
                .ToList();

            foreach (var group in nodesByDepth)
            {
                var depth = group.Key;
                var nodesInGroup = group.ToList();
                for (var i = 0; i < nodesInGroup.Count; i++)
                {
                    var nodeKvp = nodesInGroup[i];
                    var node = _techTree.Find(n => n.id == nodeKvp.Key);
                    if (node != null)
                    {
                        node.position = new Vector2Int(depth * columnSpacing, i * rowSpacing);
                    }
                }
            }
             Debug.Log("Procedural node positions calculated.");
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
                // This now needs to use the TechTreeCamera to convert screen point to world point
                if (techTreeCamera == null) return;
                
                Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
                
                // We need to check if the mouse is over the RawImage viewport
                RectTransform rawImageRect = GetComponentInChildren<RawImage>().rectTransform;
                if (!RectTransformUtility.RectangleContainsScreenPoint(rawImageRect, mouseScreenPos, Camera.main))
                {
                    return; // Mouse is not over the tech tree UI, do nothing
                }

                Vector2 localPoint;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(rawImageRect, mouseScreenPos, Camera.main, out localPoint);
                
                // Normalize the local point to be a viewport coordinate (0,0 to 1,1)
                Vector2 viewportPoint = new Vector2((localPoint.x / rawImageRect.rect.width) + rawImageRect.pivot.x, 
                                                    (localPoint.y / rawImageRect.rect.height) + rawImageRect.pivot.y);

                var mouseWorldPosition = techTreeCamera.ViewportToWorldPoint(viewportPoint);
                var cellPosition = tilemap.WorldToCell(mouseWorldPosition);
                var clickedNode = _techTree.Find(n => n.position == (Vector2Int)cellPosition);

                if (clickedNode != null)
                {
                    onNodeClicked?.Invoke(clickedNode.id);
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
            if (_techTree == null || _techTree.Count == 0 || tilemap == null) return;

            tilemap.ClearAllTiles();
            Debug.Log("Tilemap cleared for drawing.");

            // Since positions are procedural, we must calculate the bounds now
            var minX = _techTree.Min(n => n.position.x);
            var minY = _techTree.Min(n => n.position.y);
            var maxX = _techTree.Max(n => n.position.x);
            var maxY = _techTree.Max(n => n.position.y);

            var walkableNodes = new HashSet<Vector2Int>();
            for (var x = minX - columnSpacing; x <= maxX + columnSpacing; x++)
            {
                for (var y = minY - rowSpacing; y <= maxY + rowSpacing; y++)
                {
                    walkableNodes.Add(new Vector2Int(x, y));
                }
            }

            // 1. Draw paths first using the connector tile
            Debug.Log("Drawing connector paths...");
            foreach (var node in _techTree)
            {
                foreach (var dependencyId in node.dependencies)
                {
                    var dependencyNode = _techTree.Find(n => n.id == dependencyId);
                    if (dependencyNode != null)
                    {
                        var path = AStar.FindPath(node.position, dependencyNode.position, walkableNodes);
                        if (path != null)
                        {
                            foreach (var position in path)
                            {
                                tilemap.SetTile((Vector3Int)position, connectorTile);
                            }
                        }
                    }
                }
            }

            // 2. Draw main technology nodes on top of the paths
            Debug.Log($"Drawing {_techTree.Count} main nodes...");
            foreach (var node in _techTree)
            {
                var tile = node.unlocked ? unlockedTile : lockedTile;
                tilemap.SetTile((Vector3Int)node.position, tile);
            }

            // Force the tilemap to re-evaluate all tiles and apply the rules for the connector tiles.
            tilemap.RefreshAllTiles();
            
            Debug.Log("Drawing complete.");
        }
    }
}