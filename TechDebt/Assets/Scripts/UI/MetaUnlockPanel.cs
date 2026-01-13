using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Tilemaps;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems; // Added for UI event handling

[System.Serializable]
public class TechTreeNode
{
    public string id;
    public Vector2Int position;
    public List<string> dependencies;
    public bool unlocked;
}

namespace UI
{
    public class MetaUnlockPanel : MonoBehaviour, IDragHandler, IScrollHandler // Implemented interfaces
    {
        public Tilemap tilemap;
        public Tile unlockedTile;
        public Tile lockedTile;
        public TileBase connectorTile; // New property for RuleTile
        public UnityAction<string> onNodeClicked;

        // Camera control fields
        public Camera techTreeCamera;
        public float panSpeed = 0.01f; // Adjusted for better feel
        public float zoomSpeed = 0.5f;
        public float minZoom = 1f;
        public float maxZoom = 10f;

        [System.Serializable]
        public class TechTreeData
        {
            public List<TechTreeNode> nodes;
        }

        private List<TechTreeNode> _techTree;

        private void Awake()
        {
            Debug.Log("MetaUnlockPanel.Awake() called.");

            // Check for missing references
            if (tilemap == null) Debug.LogError("Tilemap reference is NOT set in the Inspector!");
            if (unlockedTile == null) Debug.LogError("UnlockedTile reference is NOT set in the Inspector!");
            if (lockedTile == null) Debug.LogError("LockedTile reference is NOT set in the Inspector!");
            if (techTreeCamera == null) Debug.LogError("TechTreeCamera reference is NOT set in the Inspector!");

            var path = Path.Combine(Application.streamingAssetsPath, "TechTree.json");
            if (File.Exists(path))
            {
                var json = File.ReadAllText(path);
                var data = JsonUtility.FromJson<TechTreeData>(json);
                _techTree = data.nodes;
                Debug.Log($"Successfully loaded {_techTree.Count} nodes from TechTree.json.");
            }
            else
            {
                Debug.LogError($"TechTree.json not found at path: {path}");
                _techTree = new List<TechTreeNode>(); // Prevent further errors
            }
        }

        private void Start()
        {
            Debug.Log("MetaUnlockPanel.Start() called.");
            DrawTechTree();
            onNodeClicked += UnlockNode;
        }

        public void UnlockNode(string nodeId)
        {
            var node = _techTree.Find(n => n.id == nodeId);
            if (node != null && !node.unlocked)
            {
                var dependenciesMet = true;
                foreach (var dependencyId in node.dependencies)
                {
                    var dependencyNode = _techTree.Find(n => n.id == dependencyId);
                    if (dependencyNode != null && !dependencyNode.unlocked)
                    {
                        dependenciesMet = false;
                        break;
                    }
                }

                if (dependenciesMet)
                {
                    node.unlocked = true;
                    DrawTechTree();
                }
            }
        }

        private void Update()
        {
            // Existing click logic remains, but OnDrag/OnScroll handle camera movement
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                var mousePosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
                var cellPosition = tilemap.WorldToCell(mousePosition);
                var clickedNode = _techTree.Find(n => n.position == (Vector2Int)cellPosition);

                if (clickedNode != null)
                {
                    onNodeClicked?.Invoke(clickedNode.id);
                }
            }
        }

        // IDragHandler implementation for panning
        public void OnDrag(PointerEventData eventData)
        {
            if (techTreeCamera == null) return;

            // Scale movement by camera orthographic size to make panning consistent at different zoom levels
            float scaleFactor = techTreeCamera.orthographicSize * 2 / Screen.height; 
            techTreeCamera.transform.position -= new Vector3(eventData.delta.x * scaleFactor * panSpeed, eventData.delta.y * scaleFactor * panSpeed, 0);
        }

        // IScrollHandler implementation for zooming
        public void OnScroll(PointerEventData eventData)
        {
            if (techTreeCamera == null) return;

            var scroll = eventData.scrollDelta.y;
            float newSize = techTreeCamera.orthographicSize - scroll * zoomSpeed;
            techTreeCamera.orthographicSize = Mathf.Clamp(newSize, minZoom, maxZoom);
        }

        private void DrawTechTree()
        {
            Debug.Log("DrawTechTree() called.");
            if (_techTree == null || _techTree.Count == 0)
            {
                Debug.LogWarning("Tech Tree has no nodes to draw.");
                return;
            }
            
            if (tilemap == null)
            {
                Debug.LogError("Tilemap reference is null in DrawTechTree! Cannot draw.");
                return;
            }

            tilemap.ClearAllTiles();
            Debug.Log("Tilemap cleared.");

            var minX = int.MaxValue;
            var minY = int.MaxValue;
            var maxX = int.MinValue;
            var maxY = int.MinValue;

            foreach (var node in _techTree)
            {
                if (node.position.x < minX) minX = node.position.x;
                if (node.position.y < minY) minY = node.position.y;
                if (node.position.x > maxX) maxX = node.position.x;
                if (node.position.y > maxY) maxY = node.position.y;
            }

            var walkableNodes = new HashSet<Vector2Int>();
            for (var x = minX; x <= maxX; x++)
            {
                for (var y = minY; y <= maxY; y++)
                {
                    walkableNodes.Add(new Vector2Int(x, y));
                }
            }
            
            Debug.Log($"Drawing {_techTree.Count} nodes...");
            foreach (var node in _techTree)
            {
                var tile = node.unlocked ? unlockedTile : lockedTile;
                tilemap.SetTile((Vector3Int)node.position, tile);

                foreach (var dependencyId in node.dependencies)
                {
                    var dependencyNode = _techTree.Find(n => n.id == dependencyId);
                    if (dependencyNode != null)
                    {
                        var path = AStar.FindPath(node.position, dependencyNode.position, walkableNodes);
                        if (path != null)
                        {
                            var pathTile = dependencyNode.unlocked ? unlockedTile : lockedTile;
                            foreach (var position in path)
                            {
                                tilemap.SetTile((Vector3Int)position, pathTile);
                            }
                        }
                    }
                }
            }
            Debug.Log("Drawing complete.");
        }
    }
}