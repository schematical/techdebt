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
        public Tile researchedTitle;
        public Tile researchingTile;
        public TileBase connectorTile;
        public Tile backgroundTile;
        public float panSpeed = 1f;
        public float zoomSpeed = 1f;
        public float minZoom = 5f;
        public float maxZoom = 20f;
        public Grid grid;
   

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
        private int columnSpacing = 6;
        private int rowSpacing = 4;

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
                Close();
            }

            _selectedNode = node;
            UpdateDetailsArea();
       
        }

        private void UpdateDetailsArea()
        {

            CleanUp();
            if (_selectedNode == null)
            {
                AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = "Select a technology to see details.";

                AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text =
                    "Unlock more tech by completing Challenges. \n\n Read more in the Challenges section of the Main Menu inbetween runs.";
                return;
            }

            Technology tech = _selectedNode.Technology;
            UIPanelLineSectionText header = AddLine<UIPanelLine>().Add<UIPanelLineSectionText>();
            header.h1(tech.DisplayName);
        
            AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text =  $"{tech.Description}\n\n";
            AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text =  $"Cost: {tech.ResearchPointCost} RP\n";

            string reqs = "Requires: None";
            if (tech.RequiredTechnologies != null && tech.RequiredTechnologies.Count == 0)
            {
                reqs =
                    $"Requires: {string.Join(", ", tech.RequiredTechnologies.Select(reqId => GameManager.Instance.GetTechnologyByID(reqId)?.DisplayName ?? "Unknown"))}";
            }
                
            AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text =  reqs;

            if (tech.CurrentState == Technology.State.Locked)
            {
                bool prerequisitesMet = true;
                if (tech.RequiredTechnologies != null)
                {
                    prerequisitesMet = tech.RequiredTechnologies.All(reqId =>
                        GameManager.Instance.GetTechnologyByID(reqId)?.CurrentState == Technology.State.Unlocked);
                }

                if (!prerequisitesMet)
                    AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text =  "Prerequisites not met.";
                else
                    AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = "Click again to start research.";
            }
            else if (tech.CurrentState == Technology.State.Researching)
            {
                float percentage = (tech.CurrentResearchProgress / tech.ResearchPointCost) * 100f;
                AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text =  $"Researching: {Mathf.FloorToInt(percentage)}%";
            }
            else if (tech.CurrentState == Technology.State.Unlocked)
            {
                AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text =  "Fully Researched";
            }

            AddButton("Start Research", () =>
            {
                SelectNode(_selectedNode);
             
            });

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
                if (techNodeView.LabelInstance != null)
                {
                    techNodeView.LabelInstance.gameObject.SetActive(false);
                }
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
            DrawPaths(_techTreeNodes.Where(IsNodeVisible).ToList());
            UpdateDetailsArea();
            

           
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
         
            GameManager.Instance.UIManager.ForcePause();
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
                GameManager.Instance.UIManager.StopForcePause();
            }

            foreach (TechNodeView techNodeView in _techTreeNodes)
            {
                if (techNodeView.LabelInstance != null)
                {
                    techNodeView.LabelInstance.gameObject.SetActive(false);
                }
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
                DrawPaths(_techTreeNodes.Where(IsNodeVisible).ToList());
                
                _hoveredNode = currentNode;
            }
        }

        private void DrawPaths(List<TechNodeView> nodesToDraw)
        {
            foreach (TechNodeView node in nodesToDraw)
            {
                if (node.Dependencies == null || node.Dependencies.Count == 0) continue;

                foreach (var depId in node.Dependencies)
                {
                    TechNodeView dep = _techTreeNodes.Find(n => n.Id == depId);
                    if (dep == null) continue;

                    // Only draw connectors that are connected to unlocked tech
                    if (dep.Technology.CurrentState != Technology.State.Unlocked && node.Technology.CurrentState != Technology.State.Unlocked) continue;

                    Vector3Int start = (Vector3Int)dep.Position;
                    Vector3Int end = (Vector3Int)node.Position;

                    DrawConnection(start, end, node.Technology.Direction);
                }
            }
        }

        private bool IsNodeVisible(TechNodeView node)
        {
            if (node.Technology.CurrentState == Technology.State.Unlocked) return true;
            if (node.Technology.CurrentState == Technology.State.Researching) return true;

            // Roots are always visible
            if (node.Dependencies == null || node.Dependencies.Count == 0) return true;

            // Visible if ANY parent is Unlocked
            return node.Dependencies.Any(depId =>
            {
                var dep = _techTreeNodes.Find(n => n.Id == depId);
                return dep != null && dep.Technology.CurrentState == Technology.State.Unlocked;
            });
        }

        private void DrawConnection(Vector3Int start, Vector3Int end, Technology.TechTreeDirection direction)
        {
            // Try preferred L-shape first
            List<Vector3Int> path = GetLShapePath(start, end, direction);
            
            // If preferred path is blocked by an obstacle (other than start/end), try A*
            if (IsPathBlocked(path, start, end))
            {
                path = FindPath(start, end);
            }
            
            // If A* failed or wasn't needed, use the (potentially blocked) L-shape as fallback/default
            if (path == null) path = GetLShapePath(start, end, direction);

            foreach (var p in path)
            {
                // Don't draw the connector on top of the start or end nodes
                if (p != start && p != end)
                    connectorTilemap.SetTile(p, connectorTile);
            }
        }

        private bool IsPathBlocked(List<Vector3Int> path, Vector3Int start, Vector3Int end)
        {
            if (path == null) return true;
            
            foreach (var point in path)
            {
                if (point == start || point == end) continue;
                
                // Check if any node is at this position
                if (_techTreeNodes.Any(n => n.Position == (Vector2Int)point))
                {
                    return true;
                }
            }
            return false;
        }

        private List<Vector3Int> GetLShapePath(Vector3Int start, Vector3Int end, Technology.TechTreeDirection direction)
        {
            List<Vector3Int> path = new List<Vector3Int>();
            path.Add(start);
            
            int stepSize = 2; // Move 2 units in the designated direction at the end

            if (direction == Technology.TechTreeDirection.Left || direction == Technology.TechTreeDirection.Right)
            {
                // Horizontal Step Path
                int xDir = (direction == Technology.TechTreeDirection.Right) ? 1 : -1;
                int turnX = end.x - (xDir * stepSize);
                
                // 1. Horizontal move to turnX
                for (int x = start.x; x != turnX; x += xDir)
                    path.Add(new Vector3Int(x + xDir, start.y, 0));
                
                // 2. Vertical move to end.y
                int yDir = (end.y > start.y) ? 1 : -1;
                if (start.y != end.y)
                {
                    for (int y = start.y; y != end.y; y += yDir)
                        path.Add(new Vector3Int(turnX, y + yDir, 0));
                }
                
                // 3. Final horizontal move to end.x
                for (int x = turnX; x != end.x; x += xDir)
                    path.Add(new Vector3Int(x + xDir, end.y, 0));
            }
            else
            {
                // Vertical Step Path (Up or Down)
                int yDir = (direction == Technology.TechTreeDirection.Up) ? 1 : -1;
                int turnY = end.y - (yDir * stepSize);
                
                // 1. Vertical move to turnY
                for (int y = start.y; y != turnY; y += yDir)
                    path.Add(new Vector3Int(start.x, y + yDir, 0));
                
                // 2. Horizontal move to end.x
                int xDir = (end.x > start.x) ? 1 : -1;
                if (start.x != end.x)
                {
                    for (int x = start.x; x != end.x; x += xDir)
                        path.Add(new Vector3Int(x + xDir, turnY, 0));
                }
                
                // 3. Final vertical move to end.y
                for (int y = turnY; y != end.y; y += yDir)
                    path.Add(new Vector3Int(end.x, y + yDir, 0));
            }
            return path;
        }

        private List<Vector3Int> FindPath(Vector3Int start, Vector3Int end)
        {
            HashSet<Vector2Int> obstacles = new HashSet<Vector2Int>(_techTreeNodes.Select(n => n.Position));
            
            // Priority queue simulated with a list
            List<Vector3Int> openSet = new List<Vector3Int> { start };
            HashSet<Vector3Int> closedSet = new HashSet<Vector3Int>();
            
            Dictionary<Vector3Int, Vector3Int> cameFrom = new Dictionary<Vector3Int, Vector3Int>();
            Dictionary<Vector3Int, float> gScore = new Dictionary<Vector3Int, float>();
            gScore[start] = 0;
            
            Dictionary<Vector3Int, float> fScore = new Dictionary<Vector3Int, float>();
            fScore[start] = GetHeuristic(start, end);
            
            while (openSet.Count > 0)
            {
                // Get node with lowest fScore
                Vector3Int current = openSet.OrderBy(n => fScore.ContainsKey(n) ? fScore[n] : float.MaxValue).First();
                
                if (current == end)
                {
                    return ReconstructPath(cameFrom, current);
                }
                
                openSet.Remove(current);
                closedSet.Add(current);
                
                foreach (Vector3Int neighbor in GetNeighbors(current))
                {
                    if (closedSet.Contains(neighbor)) continue;
                    
                    // Allow moving through end node, but not other obstacles
                    if (obstacles.Contains((Vector2Int)neighbor) && neighbor != end) continue;
                    
                    float tentativeGScore = gScore[current] + 1; // Distance is 1
                    
                    if (!gScore.ContainsKey(neighbor) || tentativeGScore < gScore[neighbor])
                    {
                        cameFrom[neighbor] = current;
                        gScore[neighbor] = tentativeGScore;
                        fScore[neighbor] = gScore[neighbor] + GetHeuristic(neighbor, end);
                        
                        if (!openSet.Contains(neighbor))
                            openSet.Add(neighbor);
                    }
                }
            }
            return null; // No path found
        }

        private float GetHeuristic(Vector3Int a, Vector3Int b)
        {
            // Manhattan distance
            return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
        }

        private List<Vector3Int> GetNeighbors(Vector3Int p)
        {
            return new List<Vector3Int>
            {
                p + Vector3Int.up,
                p + Vector3Int.down,
                p + Vector3Int.left,
                p + Vector3Int.right
            };
        }

        private List<Vector3Int> ReconstructPath(Dictionary<Vector3Int, Vector3Int> cameFrom, Vector3Int current)
        {
            List<Vector3Int> totalPath = new List<Vector3Int> { current };
            while (cameFrom.ContainsKey(current))
            {
                current = cameFrom[current];
                totalPath.Add(current);
            }
            totalPath.Reverse();
            return totalPath;
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
                if (!IsNodeVisible(node))
                {
                    if (node.LabelInstance != null) node.LabelInstance.gameObject.SetActive(false);
                    continue;
                }

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
                    labelInstance.name = $"TechNode_{node.DisplayName}";
                    node.LabelInstance = labelInstance.GetComponentInChildren<TextMeshPro>();
                }
                node.LabelInstance.text = node.DisplayName;
                node.LabelInstance.gameObject.SetActive(true);

            }
            nodeTilemap.RefreshAllTiles();
        }

        private void CenterTilemapOnCamera()
        {
            var visibleNodes = _techTreeNodes.Where(IsNodeVisible).ToList();
            if (visibleNodes.Count == 0 || Camera.main == null) return;

            int minX = visibleNodes.Min(n => n.Position.x);
            int maxX = visibleNodes.Max(n => n.Position.x);
            int minY = visibleNodes.Min(n => n.Position.y);
            int maxY = visibleNodes.Max(n => n.Position.y);

            Vector3 worldCenter = new Vector3((minX + maxX + 1) / 2.0f, (minY + maxY + 1) / 2.0f, 0);
            Transform gridTransform = connectorTilemap.transform.parent;
            
            // Move the grid to the camera's current position, then offset it so the tree's center is at the camera's center
            Vector3 cameraPos = Camera.main.transform.position;
            gridTransform.position = new Vector3(cameraPos.x - worldCenter.x, cameraPos.y - worldCenter.y, gridTransform.position.z);
            
           
        }

        private class LayoutNode
        {
            public TechNodeView View;
            public List<LayoutNode> Children = new List<LayoutNode>();
            public Vector2Int Position; // Relative to parent
            public int SubtreeBreadth; // Size in the perpendicular axis
            // We need to know the 'depth' (main axis) and 'breadth' (perp axis) relative to the incoming direction.
            // But since direction changes per child, we track the global bounds relative to this node.
            public int MinPerpOffset;
            public int MaxPerpOffset;
        }

        private void CalculateNodePositions()
        {
            if (_techTreeNodes == null || _techTreeNodes.Count == 0) return;

            // Reset positions
            foreach (var node in _techTreeNodes)
            {
                node.Position = new Vector2Int(-1000, -1000);
            }

            TechNodeView root = _techTreeNodes.Find(n => n.Id == "application-server") ?? _techTreeNodes.Find(n => n.Dependencies == null || n.Dependencies.Count == 0);
            if (root == null) return;

            // Build Tree (DAG -> Tree conversion for layout)
            HashSet<string> visited = new HashSet<string>();
            LayoutNode rootLayout = BuildLayoutTree(root, visited);

            // Calculate Sizes
            CalculateSubtreeMetrics(rootLayout);

            // Assign Positions
            root.Position = Vector2Int.zero;
            AssignPositions(rootLayout, root.Position);
        }

        private LayoutNode BuildLayoutTree(TechNodeView view, HashSet<string> visited)
        {
            visited.Add(view.Id);
            LayoutNode node = new LayoutNode { View = view };

            // Find children: Nodes that list this view as a dependency
            // Note: Since a node can have multiple dependencies, checking visited ensures it's only placed once in the hierarchy.
            var potentialChildren = _techTreeNodes.Where(n => n.Dependencies != null && n.Dependencies.Contains(view.Id));
            
            // Sort children to ensure consistent order (e.g. by name or ID)
            var sortedChildren = potentialChildren.OrderBy(n => n.Id).ToList();

            foreach (var childView in sortedChildren)
            {
                if (!visited.Contains(childView.Id))
                {
                    node.Children.Add(BuildLayoutTree(childView, visited));
                }
            }
            return node;
        }

        private void CalculateSubtreeMetrics(LayoutNode node)
        {
            if (node.Children.Count == 0)
            {
                node.MinPerpOffset = 0;
                node.MaxPerpOffset = 0;
                return;
            }

            // Group by direction
            var grouped = node.Children.GroupBy(c => c.View.Technology.Direction);
            
            // Calculate total breadth required for each direction group
            foreach (var group in grouped)
            {
                Technology.TechTreeDirection dir = group.Key;
                int perpSpacing = (dir == Technology.TechTreeDirection.Left || dir == Technology.TechTreeDirection.Right) ? rowSpacing * 2 : columnSpacing;

                List<LayoutNode> children = group.ToList();
                
                // Calculate total breadth
                int totalBreadth = 0;
                List<int> childBreadths = new List<int>();
                
                foreach (var child in children)
                {
                    CalculateSubtreeMetrics(child); // Recursive call
                    int b = child.MaxPerpOffset - child.MinPerpOffset;
                    if (b == 0) b = 2; // Minimum size for a node
                    childBreadths.Add(b);
                    totalBreadth += b;
                }
                
                totalBreadth += (children.Count - 1) * perpSpacing;
                
                // Now assign relative positions (perp offset)
                int currentPos = -totalBreadth / 2;
                int mainDistModifier = (children.Count > 1) ? 2 : 0;
                
                for(int i=0; i<children.Count; i++)
                {
                    var child = children[i];
                    int breadth = childBreadths[i];
                    
                    // Position is center of the block
                    int centerOffset = currentPos + breadth / 2;

                    int mainDistVal = (dir == Technology.TechTreeDirection.Up || dir == Technology.TechTreeDirection.Down) ? rowSpacing * 2 : columnSpacing;
                    mainDistVal += mainDistModifier;
                    
                    Vector2Int dirVec = GetDirectionVector(dir);
                    Vector2Int perpVec = GetPerpendicularVector(dir);
                    child.Position = dirVec * mainDistVal + perpVec * centerOffset;
                    
                    currentPos += breadth + perpSpacing;
                }
                
                if (dir == Technology.TechTreeDirection.Up || dir == Technology.TechTreeDirection.Down)
                {
                    // These children contribute to our horizontal breadth.
                    int half = totalBreadth / 2;
                    node.MinPerpOffset = Mathf.Min(node.MinPerpOffset, -half);
                    node.MaxPerpOffset = Mathf.Max(node.MaxPerpOffset, half);
                }
            }
            
            // Ensure non-zero size for the node itself
            if (node.MinPerpOffset == 0 && node.MaxPerpOffset == 0)
            {
                 node.MinPerpOffset = -1;
                 node.MaxPerpOffset = 1;
            }
        }

        private void AssignPositions(LayoutNode node, Vector2Int currentPos)
        {
            node.View.Position = currentPos;
            foreach (var child in node.Children)
            {
                AssignPositions(child, currentPos + child.Position);
            }
        }

        private Vector2Int GetDirectionVector(Technology.TechTreeDirection dir)
        {
            switch (dir)
            {
                case Technology.TechTreeDirection.Up: return Vector2Int.up;
                case Technology.TechTreeDirection.Down: return Vector2Int.down;
                case Technology.TechTreeDirection.Left: return Vector2Int.left;
                case Technology.TechTreeDirection.Right: return Vector2Int.right;
                default: return Vector2Int.right;
            }
        }

        private Vector2Int GetPerpendicularVector(Technology.TechTreeDirection dir)
        {
            switch (dir)
            {
                case Technology.TechTreeDirection.Up: 
                case Technology.TechTreeDirection.Down: return Vector2Int.right;
                case Technology.TechTreeDirection.Left:
                case Technology.TechTreeDirection.Right: return Vector2Int.up;
                default: return Vector2Int.up;
            }
        }
    }
}