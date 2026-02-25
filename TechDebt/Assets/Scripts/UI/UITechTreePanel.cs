using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using TMPro;
using UnityEngine.Serialization;

namespace UI
{
    public class UITechTreePanel : UIPanel
    {
        // Public fields for Unity Inspector assignments
        public Tilemap connectorTilemap;
        public Tilemap nodeTilemap;
        public Tilemap backgroundTilemap;
        public GameObject nameLabelPrefab;
        public Tile unlockedTile;
        public Tile lockedTile;
        [FormerlySerializedAs("researchTitle")] public Tile researchedTitle;
        public TileBase connectorTile;
        public Tile backgroundTile;
        public float panSpeed = 1f;
        public float zoomSpeed = 1f;
        public float minZoom = 5f;
        public float maxZoom = 20f;
        public Grid grid;
        public UITextArea metaUnlockTextArea;

        // Internal state
        private class TechNodeView
        {
            public Technology Technology;
            public Vector2Int Position;
            public string Id => Technology.TechnologyID;
            public string DisplayName => Technology.DisplayName;
            public List<string> Dependencies => Technology.RequiredTechnologies;
      
        }

        private List<TechNodeView> _techTreeNodes = new List<TechNodeView>();
        private List<GameObject> _nameLabels = new List<GameObject>();
        private TechNodeView _hoveredNode = null;
        private bool _initialSetupComplete = false;

        // Configuration for procedural layout
        private int columnSpacing = 4;
        private int rowSpacing = 2;

        public UnityAction<string> onNodeClicked;

        protected override void Update()
        {   
            base.Update();
            
            HandleInput();
            UpdateLineageView();
        }

        private void HandleInput()
        {
            if (Mouse.current == null) return;

            // Left-click detection
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                Vector2 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
                var cellPosition = nodeTilemap.WorldToCell(mouseWorldPosition);
                var clickedNode = _techTreeNodes.Find(n => n.Position == (Vector2Int)cellPosition);

                if (clickedNode != null)
                {
                    onNodeClicked?.Invoke(clickedNode.Id);
                    Debug.Log($"Clicked on {clickedNode.DisplayName}");
                    // Logic to start research or show details could go here
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
            {
                float newSize = Camera.main.orthographicSize - scroll * zoomSpeed;
                Camera.main.orthographicSize = Mathf.Clamp(newSize, minZoom, maxZoom);
            }
        }

        public void Refresh(Technology technology = null)
        {
            Debug.Log("UITechTreePanel.Refresh called");
            
            if (nodeTilemap == null) Debug.LogError("UITechTreePanel: nodeTilemap is null!");
            if (connectorTilemap == null) Debug.LogError("UITechTreePanel: connectorTilemap is null!");
            if (nameLabelPrefab == null) Debug.LogError("UITechTreePanel: nameLabelPrefab is null!");
            if (unlockedTile == null) Debug.LogError("UITechTreePanel: unlockedTile is null!");
            if (lockedTile == null) Debug.LogError("UITechTreePanel: lockedTile is null!");

            // Clear existing
            if (nodeTilemap != null) nodeTilemap.ClearAllTiles();
            if (connectorTilemap != null) connectorTilemap.ClearAllTiles();
      

            var allTech = GameManager.Instance.AllTechnologies;
            Debug.Log($"UITechTreePanel: GameManager.AllTechnologies count: {allTech.Count}");

            // Rebuild list
            foreach (var tech in allTech)
            {
                /*if (tech.CurrentState == Technology.State.MetaLocked)
                {
                    continue;
                }*/

                _techTreeNodes.Add(new TechNodeView { Technology = tech });
            }
            
            Debug.Log($"UITechTreePanel: _techTreeNodes count after filtering: {_techTreeNodes.Count}");

            if (_techTreeNodes.Count == 0) return;

            DrawBackground();
            CalculateNodePositions();
            DrawNodesAndLabels();

            if (metaUnlockTextArea != null)
            {
                 metaUnlockTextArea.transform.SetAsLastSibling();
            }

           
        }

        private void DrawBackground()
        {
            if (backgroundTilemap == null)
            {
                Debug.LogError("UITechTreePanel: backgroundTilemap is null!");
                return;
            }

            if (backgroundTile == null)
            {
                Debug.LogError("UITechTreePanel: backgroundTile is null!");
                return;
            }

            backgroundTilemap.ClearAllTiles();

            for (int x = -64; x < 64; x++)
            {
                for (int y = -64; y < 64; y++)
                {
                    backgroundTilemap.SetTile(new Vector3Int(x, y, 0), backgroundTile);
                }
            }
        }

        public override void Show()
        {
            base.Show();
         
            GameManager.OnTechnologyUnlocked += Refresh;
            grid.gameObject.SetActive(true);
            Refresh();
            CenterTilemapOnCamera();
        }

        public override void Close(bool forceClose = false)
        {
            foreach (var label in _nameLabels)
            {
                label.gameObject.SetActive(false);
            }
            _nameLabels.Clear();
            _techTreeNodes.Clear();
            if (nodeTilemap != null) nodeTilemap.ClearAllTiles();
            if (connectorTilemap != null) connectorTilemap.ClearAllTiles();
            if (backgroundTilemap != null) backgroundTilemap.ClearAllTiles();
            grid.gameObject.SetActive(false);
            

            GameManager.OnTechnologyUnlocked -= Refresh;
            GameManager.OnTechnologyResearchStarted -= Refresh;
            base.Close(forceClose);
        }

        private void UpdateLineageView()
        {
            if (Camera.main == null || Mouse.current == null) return;

            Vector2 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            var cellPosition = nodeTilemap.WorldToCell(mouseWorldPosition);
            var currentNode = _techTreeNodes.Find(n => n.Position == (Vector2Int)cellPosition);

            if (currentNode != _hoveredNode)
            {
                connectorTilemap.ClearAllTiles();
                
                if (currentNode != null)
                {
                    // Draw paths to dependencies (parents)
                    DrawPaths(new List<TechNodeView> { currentNode });

                    // Find and draw paths to dependents (children)
                    var children = _techTreeNodes.Where(n => n.Dependencies != null && n.Dependencies.Contains(currentNode.Id)).ToList();
                    DrawPaths(children);
                }
                
                _hoveredNode = currentNode;
            }
        }

        private void DrawPaths(List<TechNodeView> nodesToDraw)
        {
            foreach (var node in nodesToDraw)
            {
                if (node.Dependencies == null || node.Dependencies.Count == 0) continue;

                var dependencyNodes = node.Dependencies
                    .Select(depId => _techTreeNodes.Find(n => n.Id == depId))
                    .Where(n => n != null).ToList();

                if (dependencyNodes.Count == 0) continue;

                int maxDepX = dependencyNodes.Max(d => d.Position.x);
                int busX = maxDepX + 2; // Create a 1-tile horizontal space before the vertical bus

                // Path from child node to bus
                for (int x = busX; x <= node.Position.x; x++)
                {
                    connectorTilemap.SetTile(new Vector3Int(x, node.Position.y, 0), connectorTile);
                }

                // Vertical bus itself
                var minDepY = dependencyNodes.Min(d => d.Position.y);
                var maxDepY = dependencyNodes.Max(d => d.Position.y);
                var busMinY = Mathf.Min(minDepY, node.Position.y);
                var busMaxY = Mathf.Max(maxDepY, node.Position.y);
                for (int y = busMinY; y <= busMaxY; y++)
                {
                    connectorTilemap.SetTile(new Vector3Int(busX, y, 0), connectorTile);
                }

                // Paths from each dependency to the bus
                foreach (var dep in dependencyNodes)
                {
                    for (int x = dep.Position.x; x <= busX; x++)
                    {
                        connectorTilemap.SetTile(new Vector3Int(x, dep.Position.y, 0), connectorTile);
                    }
                }
            }
        }

        private void DrawNodesAndLabels()
        {
            Debug.Log("DrawNodesAndLabels called");
            if (_techTreeNodes == null || nodeTilemap == null)
            {
                Debug.LogError($"UITechTreePanel: Cannot draw. nodes: {_techTreeNodes?.Count}, nodeTilemap: {nodeTilemap}");
                return;
            }

            foreach (var node in _techTreeNodes)
            {
                Tile tile;
                switch (node.Technology.CurrentState)
                {
                    case(Technology.State.MetaLocked):
                        tile = lockedTile;
                        break;
                    case(Technology.State.Locked):
                        tile = unlockedTile;
                        break;
                    case(Technology.State.Unlocked):
                        tile = researchedTitle;
                        break;
                    default:
                        throw new System.NotImplementedException();
                        
                }
         
                nodeTilemap.SetTile((Vector3Int)node.Position, tile);

              
                Vector3 worldPos = nodeTilemap.GetCellCenterWorld((Vector3Int)node.Position);
                GameObject labelInstance = GameManager.Instance.prefabManager.Create("UITechTreeLabel",
                    worldPos + new Vector3(0, -1.5f, 0), nodeTilemap.transform.parent);
                var textComponent = labelInstance.GetComponentInChildren<TextMeshPro>();
                if (textComponent != null)
                {
                    textComponent.text = node.DisplayName;
                }
                _nameLabels.Add(labelInstance);
              
            }
            nodeTilemap.RefreshAllTiles();
        }

        private void CenterTilemapOnCamera()
        {
            if (_techTreeNodes == null || _techTreeNodes.Count == 0 || Camera.main == null) return;

            var minX = _techTreeNodes.Min(n => n.Position.x);
            var maxX = _techTreeNodes.Max(n => n.Position.x);
            var minY = _techTreeNodes.Min(n => n.Position.y);
            var maxY = _techTreeNodes.Max(n => n.Position.y);

            Vector3 worldCenter = new Vector3((minX + maxX + 1) / 2.0f, (minY + maxY + 1) / 2.0f, 0);
            var gridTransform = connectorTilemap.transform.parent;
            
            // Move the grid to the camera's current position, then offset it so the tree's center is at the camera's center
            Vector3 cameraPos = Camera.main.transform.position;
            gridTransform.position = new Vector3(cameraPos.x - worldCenter.x, cameraPos.y - worldCenter.y, gridTransform.position.z);
            
            Debug.Log($"UITechTreePanel: Centering tree. Camera at {cameraPos}, Tree center at {worldCenter}. New Grid Position: {gridTransform.position}");
        }

        private void CalculateNodePositions()
        {
            Debug.Log($"CalculateNodePositions called with {_techTreeNodes?.Count} nodes.");
            if (_techTreeNodes == null || _techTreeNodes.Count == 0) return;

            // Pass 1: Determine X position (column) for each node.
            foreach (var node in _techTreeNodes)
            {
                CalculateNodeXPosition(node);
            }

            // Group nodes by column for Y positioning
            var nodesByColumn = _techTreeNodes.GroupBy(n => n.Position.x)
                                         .OrderBy(g => g.Key)
                                         .ToDictionary(g => g.Key, g => g.ToList());
            
            // Pass 2: Position Y values, starting with leaf nodes.
            var allDependencyIds = new HashSet<string>(_techTreeNodes.SelectMany(n => n.Dependencies ?? new List<string>()));
            var leafNodes = _techTreeNodes.Where(n => !allDependencyIds.Contains(n.Id)).OrderBy(n => n.Position.x).ToList();

            int currentY = 0;
            foreach (var node in leafNodes)
            {
                node.Position.y = currentY;
                currentY += rowSpacing * 2;
            }

            // Position Y values from right to left, centering parents over children
            for (int i = nodesByColumn.Keys.Max(); i >= 0; i--)
            {
                if (!nodesByColumn.ContainsKey(i)) continue;
                
                foreach (var node in nodesByColumn[i])
                {
                    var children = _techTreeNodes.Where(n => n.Dependencies != null && n.Dependencies.Contains(node.Id)).ToList();
                    if (children.Count > 0)
                    {
                        float avgChildY = (float)children.Average(c => c.Position.y);
                        node.Position.y = Mathf.RoundToInt(avgChildY);
                    }
                }
            }

            // Pass 3: Overlap Resolution
            foreach (var col in nodesByColumn.Keys)
            {
                var columnNodes = nodesByColumn[col].OrderBy(n => n.Position.y).ToList();
                for (int j = 1; j < columnNodes.Count; j++)
                {
                    var prevNode = columnNodes[j - 1];
                    var currNode = columnNodes[j];
                    
                    if (currNode.Position.y < prevNode.Position.y + rowSpacing)
                    {
                        currNode.Position.y = prevNode.Position.y + rowSpacing;
                    }
                }
            }

            foreach (var node in _techTreeNodes)
            {
                Debug.Log($"Node {node.DisplayName} calculated position: {node.Position}");
            }
        }

        private int CalculateNodeXPosition(TechNodeView node)
        {
            // If position is already calculated (x > 0 or it's a root), return it.
            // Note: Assuming 0 is a valid start, checking if dependencies are processed might be safer, 
            // but recursive logic usually handles it. 
            // Better check: if we initialized positions to -1 or similar. 
            // But here we rely on the fact that we calculate from dependencies up.
            // If dependencies are empty, it's 0.
            
            if (node.Dependencies == null || node.Dependencies.Count == 0)
            {
                return node.Position.x = 0;
            }

            // If we already visited this node in a previous recursive call (not strictly cycle detection but memoization)
            // Since we iterate all nodes in the outer loop, we might re-calculate. 
            // For now, let's just re-calculate to be safe or add a visited check if performance matters.
            // The original code: if (node.position.x > 0 ... return it).
            if (node.Position.x > 0) return node.Position.x;

            int maxDepX = 0;
            if (node.Dependencies != null && node.Dependencies.Count > 0)
            {
                maxDepX = node.Dependencies
                    .Select(depId => _techTreeNodes.Find(n => n.Id == depId))
                    .Where(n => n != null)
                    .Max(n => CalculateNodeXPosition(n)); 
            }

            node.Position.x = maxDepX + columnSpacing;
            return node.Position.x;
        }
    }
}