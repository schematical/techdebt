using System;
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
      
        public Tile unlockedTile;
        public Tile lockedTile;
        [FormerlySerializedAs("researchTitle")] public Tile researchedTitle;
        public Tile researchingTile;
        public TileBase connectorTile;
        public Tile backgroundTile;
        public float panSpeed = 1f;
        public float zoomSpeed = 1f;
        public float minZoom = 5f;
        public float maxZoom = 20f;
        public Grid grid;
        public TextMeshProUGUI metaUnlockText;

        // Internal state
        private class TechNodeView
        {
            public Technology Technology;
            public Vector2Int Position = new Vector2Int(-1, -1);
            public string Id => Technology.TechnologyID;
            public string DisplayName => Technology.DisplayName;
            public List<string> Dependencies => Technology.RequiredTechnologies;
            public TextMeshPro LabelInstance;
        }

        private List<TechNodeView> _techTreeNodes = new List<TechNodeView>();
        private TechNodeView _hoveredNode = null;
        private TechNodeView _selectedNode = null;
        private bool _initialSetupComplete = false;
        private float _lastProgressUpdateTime = 0f;

        // Configuration for procedural layout
        private int columnSpacing = 4;
        private int rowSpacing = 2;

        public UnityAction<string> onNodeClicked;

        protected override void Update()
        {   
            base.Update();
            
            HandleInput();
            UpdateLineageView();
            UpdateResearchProgress();
        }

        private void UpdateResearchProgress()
        {
            if (Time.time - _lastProgressUpdateTime < 0.5f) return;
            _lastProgressUpdateTime = Time.time;

            foreach (TechNodeView node in _techTreeNodes)
            {
                if (node.Technology.CurrentState == Technology.State.Researching)
                {
                    TextMeshPro textComponent = node.LabelInstance.GetComponentInChildren<TextMeshPro>();
                   
                    float percentage = (node.Technology.CurrentResearchProgress / node.Technology.ResearchPointCost) * 100f;
                    textComponent.text = $"{node.DisplayName}({Mathf.FloorToInt(percentage)}%)";
                    
                }
            }
        }

        private void HandleInput()
        {
            if (Mouse.current == null) return;

            // Left-click detection
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                Vector2 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
                Vector3Int cellPosition = nodeTilemap.WorldToCell(mouseWorldPosition);
                TechNodeView clickedNode = _techTreeNodes.Find(n => n.Position == (Vector2Int)cellPosition);

                if (clickedNode != null)
                {
                    SelectNode(clickedNode);
                    onNodeClicked?.Invoke(clickedNode.Id);
                }
            }
            
            // Right-click and drag for panning (horizontal and vertical)
            if (Mouse.current.rightButton.isPressed)
            {
                Vector2 mouseDelta = Mouse.current.delta.ReadValue();
                if (mouseDelta.magnitude > 0.01f)
                {
                    Transform gridTransform = connectorTilemap.transform.parent;
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

        private void SelectNode(TechNodeView node)
        {
            if (_selectedNode == node)
            {
                // Double click or click while selected - try to research
                if (node.Technology.CurrentState == Technology.State.Locked)
                {
                    GameManager.Instance.SelectTechnologyForResearch(node.Technology);
                    Refresh();
                }
            }

            _selectedNode = node;
            UpdateDetailsArea();
        }

        private void UpdateDetailsArea()
        {


            if (_selectedNode == null)
            {
                metaUnlockText.text = "Select a technology to see details.";
                return;
            }

            Technology tech = _selectedNode.Technology;
            string details = $"<b>{tech.DisplayName}</b>\n\n";
            details += $"{tech.Description}\n\n";
            details += $"Cost: {tech.ResearchPointCost} RP\n";

            string reqs = "Requires: None";
            if (tech.RequiredTechnologies != null && tech.RequiredTechnologies.Count == 0)
            {
                reqs =
                    $"Requires: {string.Join(", ", tech.RequiredTechnologies.Select(reqId => GameManager.Instance.GetTechnologyByID(reqId)?.DisplayName ?? "Unknown"))}";
            }
                
            details += reqs + "\n\n";

            if (tech.CurrentState == Technology.State.Locked)
            {
                bool prerequisitesMet = true;
                if (tech.RequiredTechnologies != null)
                {
                    prerequisitesMet = tech.RequiredTechnologies.All(reqId =>
                        GameManager.Instance.GetTechnologyByID(reqId)?.CurrentState == Technology.State.Unlocked);
                }

                if (!prerequisitesMet)
                    details += "Prerequisites not met.";
                else if (GameManager.Instance.CurrentlyResearchingTechnology != null)
                    details += "Already researching another tech.";
                else
                    details += "Click again to start research.";
            }
            else if (tech.CurrentState == Technology.State.Researching)
            {
                float percentage = (tech.CurrentResearchProgress / tech.ResearchPointCost) * 100f;
                details += $"Researching: {Mathf.FloorToInt(percentage)}%";
            }
            else if (tech.CurrentState == Technology.State.Unlocked)
            {
                details += "Fully Researched";
            }

            metaUnlockText.text = details;
        }

        public void Refresh(Technology technology = null)
        {
            
            if (nodeTilemap == null) Debug.LogError("UITechTreePanel: nodeTilemap is null!");
            if (connectorTilemap == null) Debug.LogError("UITechTreePanel: connectorTilemap is null!");
            if (unlockedTile == null) Debug.LogError("UITechTreePanel: unlockedTile is null!");
            if (lockedTile == null) Debug.LogError("UITechTreePanel: lockedTile is null!");

            // Clear existing
            if (nodeTilemap != null) nodeTilemap.ClearAllTiles();
            if (connectorTilemap != null) connectorTilemap.ClearAllTiles();
      

            List<Technology> allTech = GameManager.Instance.GetAllTechnologies();

            foreach (TechNodeView techNodeView in _techTreeNodes)
            {
                techNodeView.LabelInstance.gameObject.SetActive(false);
            }
            _techTreeNodes.Clear();
            foreach (Technology tech in allTech)
            {
                _techTreeNodes.Add(new TechNodeView { Technology = tech });
            }
            

            if (_techTreeNodes.Count == 0) return;

            DrawBackground();
            CalculateNodePositions();
            DrawNodesAndLabels();
            UpdateDetailsArea();

            if (metaUnlockText != null)
            {
                 metaUnlockText.transform.SetAsLastSibling();
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
         
            GameManager.Instance.UIManager.SetTimeScalePause();
            GameManager.OnTechnologyUnlocked += Refresh;
            GameManager.OnTechnologyResearchStarted += Refresh;
            grid.gameObject.SetActive(true);
            Refresh();
            CenterTilemapOnCamera();
        }

        public override void Close(bool forceClose = false)
        {
            if (panelState != UIState.Closed)
            {
                GameManager.Instance.UIManager.Resume();
            }

            foreach (TechNodeView techNodeView in _techTreeNodes)
            {
                techNodeView.LabelInstance.gameObject.SetActive(false);
            }
        
        
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
            Vector3Int cellPosition = nodeTilemap.WorldToCell(mouseWorldPosition);
            TechNodeView currentNode = _techTreeNodes.Find(n => n.Position == (Vector2Int)cellPosition);

            if (currentNode != _hoveredNode)
            {
                connectorTilemap.ClearAllTiles();
                
                if (currentNode != null)
                {
                    // Draw paths to dependencies (parents)
                    DrawPaths(new List<TechNodeView> { currentNode });

                    // Find and draw paths to dependents (children)
                    List<TechNodeView> children = _techTreeNodes.Where(n => n.Dependencies != null && n.Dependencies.Contains(currentNode.Id)).ToList();
                    DrawPaths(children);
                }
                
                _hoveredNode = currentNode;
            }
        }

        private void DrawPaths(List<TechNodeView> nodesToDraw)
        {
            foreach (TechNodeView node in nodesToDraw)
            {
                if (node.Dependencies == null || node.Dependencies.Count == 0) continue;

                List<TechNodeView> dependencyNodes = node.Dependencies
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
                int minDepY = dependencyNodes.Min(d => d.Position.y);
                int maxDepY = dependencyNodes.Max(d => d.Position.y);
                int busMinY = Mathf.Min(minDepY, node.Position.y);
                int busMaxY = Mathf.Max(maxDepY, node.Position.y);
                for (int y = busMinY; y <= busMaxY; y++)
                {
                    connectorTilemap.SetTile(new Vector3Int(busX, y, 0), connectorTile);
                }

                // Paths from each dependency to the bus
                foreach (TechNodeView dep in dependencyNodes)
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
    
            if (_techTreeNodes == null || nodeTilemap == null)
            {
                Debug.LogError($"UITechTreePanel: Cannot draw. nodes: {_techTreeNodes?.Count}, nodeTilemap: {nodeTilemap}");
                return;
            }

            

            foreach (TechNodeView node in _techTreeNodes)
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
                    case(Technology.State.Researching):
                        tile = researchingTile;
                        break;
                    case(Technology.State.Unlocked):
                        tile = researchedTitle;
                        break;
                    default:
                        throw new System.NotImplementedException();
                        
                }

                if (tile == null)
                {
                    throw new SystemException(
                        $"Could not find a tile for `{node.DisplayName}` - State: {node.Technology.CurrentState}");
                }
                nodeTilemap.SetTile((Vector3Int)node.Position, tile);

                if (node.LabelInstance == null)
                {
                    Vector3 worldPos = nodeTilemap.GetCellCenterWorld((Vector3Int)node.Position);
                    GameObject labelInstance = GameManager.Instance.prefabManager.Create("UITechTreeLabel",
                        worldPos + new Vector3(0, -1.5f, 0), nodeTilemap.transform.parent);
                    node.LabelInstance = labelInstance.GetComponentInChildren<TextMeshPro>();

                   
                }
                node.LabelInstance.text = node.DisplayName;


            }
            nodeTilemap.RefreshAllTiles();
        }

        private void CenterTilemapOnCamera()
        {
            if (_techTreeNodes == null || _techTreeNodes.Count == 0 || Camera.main == null) return;

            int minX = _techTreeNodes.Min(n => n.Position.x);
            int maxX = _techTreeNodes.Max(n => n.Position.x);
            int minY = _techTreeNodes.Min(n => n.Position.y);
            int maxY = _techTreeNodes.Max(n => n.Position.y);

            Vector3 worldCenter = new Vector3((minX + maxX + 1) / 2.0f, (minY + maxY + 1) / 2.0f, 0);
            Transform gridTransform = connectorTilemap.transform.parent;
            
            // Move the grid to the camera's current position, then offset it so the tree's center is at the camera's center
            Vector3 cameraPos = Camera.main.transform.position;
            gridTransform.position = new Vector3(cameraPos.x - worldCenter.x, cameraPos.y - worldCenter.y, gridTransform.position.z);
            
           
        }

        private void CalculateNodePositions()
        {
    
            if (_techTreeNodes == null || _techTreeNodes.Count == 0) return;

            // Pass 1: Determine X position (column) for each node.
            var calculationStack = new HashSet<string>();
            foreach (TechNodeView node in _techTreeNodes)
            {
                CalculateNodeXPosition(node, calculationStack);
            }

            // Group nodes by column for Y positioning
            Dictionary<int, List<TechNodeView>> nodesByColumn = _techTreeNodes.GroupBy(n => n.Position.x)
                                         .OrderBy(g => g.Key)
                                         .ToDictionary(g => g.Key, g => g.ToList());
            
            // Pass 2: Position Y values, starting with leaf nodes.
            HashSet<string> allDependencyIds = new HashSet<string>(_techTreeNodes.SelectMany(n => n.Dependencies ?? new List<string>()));
            List<TechNodeView> leafNodes = _techTreeNodes.Where(n => !allDependencyIds.Contains(n.Id)).OrderBy(n => n.Position.x).ToList();

            int currentY = 0;
            foreach (TechNodeView node in leafNodes)
            {
                node.Position.y = currentY;
                currentY += rowSpacing * 2;
            }

            // Position Y values from right to left, centering parents over children
            for (int i = nodesByColumn.Keys.Max(); i >= 0; i--)
            {
                if (!nodesByColumn.ContainsKey(i)) continue;
                
                foreach (TechNodeView node in nodesByColumn[i])
                {
                    List<TechNodeView> children = _techTreeNodes.Where(n => n.Dependencies != null && n.Dependencies.Contains(node.Id)).ToList();
                    if (children.Count > 0)
                    {
                        float avgChildY = (float)children.Average(c => c.Position.y);
                        node.Position.y = Mathf.RoundToInt(avgChildY);
                    }
                }
            }

            // Pass 3: Overlap Resolution
            foreach (int col in nodesByColumn.Keys)
            {
                List<TechNodeView> columnNodes = nodesByColumn[col].OrderBy(n => n.Position.y).ToList();
                for (int j = 1; j < columnNodes.Count; j++)
                {
                    TechNodeView prevNode = columnNodes[j - 1];
                    TechNodeView currNode = columnNodes[j];
                    
                    if (currNode.Position.y < prevNode.Position.y + rowSpacing)
                    {
                        currNode.Position.y = prevNode.Position.y + rowSpacing;
                    }
                }
            }

         
        }

        private int CalculateNodeXPosition(TechNodeView node, HashSet<string> stack)
        {
            if (node.Position.x != -1) return node.Position.x;

            if (stack.Contains(node.Id))
            {
                Debug.LogError($"Cycle detected involving node {node.Id}");
                return 0;
            }

            stack.Add(node.Id);

            if (node.Dependencies == null || node.Dependencies.Count == 0)
            {
                stack.Remove(node.Id);
                return node.Position.x = 0;
            }

            int maxDepX = 0;
            if (node.Dependencies != null && node.Dependencies.Count > 0)
            {
                var deps = node.Dependencies
                    .Select(depId => _techTreeNodes.Find(n => n.Id == depId))
                    .Where(n => n != null)
                    .ToList();
                
                if (deps.Count > 0)
                {
                    maxDepX = deps.Max(n => CalculateNodeXPosition(n, stack));
                }
            }

            stack.Remove(node.Id);
            node.Position.x = maxDepX + columnSpacing;
            return node.Position.x;
        }
    }
}