using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using TMPro;

namespace UI
{
    public abstract class UIMapPanel : UIPanel
    {
        // Public fields for Unity Inspector assignments
        public Tilemap connectorTilemap;
        public Tilemap nodeTilemap;
        public Tilemap backgroundTilemap;

        public TileBase connectorTile;
        public Tile backgroundTile;
        
        public float panSpeed = 1f;
        public float zoomSpeed = 1f;
        public float minZoom = 5f;
        public float maxZoom = 20f;
        public Grid grid;

        // Internal state
        public class MapNodeView
        {
            public iUIMapNode Node;
            public Vector2Int Position = new Vector2Int(-1000, -1000);
            public string Id => Node.Id;
            public string DisplayName => Node.DisplayName;
            public List<string> DependencyIds => Node.DependencyIds;
            public TextMeshPro LabelInstance;
        }

        protected List<MapNodeView> _mapNodes = new List<MapNodeView>();
        protected MapNodeView _hoveredNode = null;
        protected MapNodeView _selectedNode = null;
        protected float _lastProgressUpdateTime = 0f;

        // Configuration for procedural layout
        protected int columnSpacing = 6;
        protected int rowSpacing = 4;

        public UnityAction<string> onNodeClicked;

        protected override void Update()
        {
            base.Update();
            HandleInput();
            UpdateHoverView();
            UpdateActiveProgress();
        }

        protected virtual void UpdateActiveProgress()
        {
            if (Time.time - _lastProgressUpdateTime < 0.5f) return;
            _lastProgressUpdateTime = Time.time;

            foreach (MapNodeView nodeView in _mapNodes)
            {
                if (nodeView.Node.CurrentState == MapNodeState.Active)
                {
                    float percentage = nodeView.Node.GetProgress() * 100f;
                    nodeView.LabelInstance.text = $"{nodeView.DisplayName}({Mathf.FloorToInt(percentage)}%)";
                }
            }
        }

        protected virtual void HandleInput()
        {
            if (Mouse.current == null) return;

            // Left-click detection
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                Vector2 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
                Vector3Int cellPosition = nodeTilemap.WorldToCell(mouseWorldPosition);
                MapNodeView clickedNode = _mapNodes.Find(n => n.Position == (Vector2Int)cellPosition);

                if (clickedNode != null)
                {
                    SelectNode(clickedNode);
                    onNodeClicked?.Invoke(clickedNode.Id);
                }
            }

            // Right-click and drag for panning
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

            // Zoom
            float scroll = Mouse.current.scroll.ReadValue().y;
            if (Mathf.Abs(scroll) > 0.1f)
            {
                float newSize = Camera.main.orthographicSize - scroll * zoomSpeed;
                Camera.main.orthographicSize = Mathf.Clamp(newSize, minZoom, maxZoom);
            }
        }

        protected virtual void SelectNode(MapNodeView nodeView)
        {
            _selectedNode = nodeView;
            nodeView.Node.OnSelected(this);
            UpdateDetailsArea();
        }

        public abstract void UpdateDetailsArea();

        public MapNodeView GetSelectedNode() => _selectedNode;

        public List<MapNodeView> GetAllNodes() => _mapNodes;

        public override void Show()
        {
            bool isInitialShow = !IsOpen();
            base.Show();
            GameManager.Instance.UIManager.ForcePause();
            GameManager.Instance.UIManager.victoryConditionListPanel.Close();
            grid.gameObject.SetActive(true);
            Refresh();
            GameManager.Instance.cameraController.DisableCameraInput();
            CenterTilemapOnCamera(isInitialShow);
        }

        public override void Close(bool forceClose = false)
        {
            
            foreach (MapNodeView nodeView in _mapNodes)
            {
                if (nodeView.LabelInstance != null)
                {
                    nodeView.LabelInstance.gameObject.SetActive(false);
                }
            }

            _mapNodes.Clear();
            if (nodeTilemap != null) nodeTilemap.ClearAllTiles();
            if (connectorTilemap != null) connectorTilemap.ClearAllTiles();
            if (backgroundTilemap != null) backgroundTilemap.ClearAllTiles();
            grid.gameObject.SetActive(false);

            if (!forceClose && panelState != UIState.Closed)
            {
                if (GameManager.Instance.State == GameManager.GameManagerState.Playing)
                {
                    GameManager.Instance.UIManager.victoryConditionListPanel.Show();
                }

                GameManager.Instance.cameraController.EnableCameraInput();
                GameManager.Instance.cameraController.RestoreSnap();
                GameManager.Instance.UIManager.StopForcePause();
            }
          
            
            base.Close(forceClose);
           
        }

        public abstract void PopulateNodes();

        public virtual void Refresh()
        {
            if (nodeTilemap == null || connectorTilemap == null) 
            {
                return;
            }


            nodeTilemap.ClearAllTiles();
            connectorTilemap.ClearAllTiles();

            foreach (MapNodeView nodeView in _mapNodes)
            {
                if (nodeView.LabelInstance != null) nodeView.LabelInstance.gameObject.SetActive(false);
            }
            _mapNodes.Clear();

            PopulateNodes();

            if (_mapNodes.Count == 0) 
            {
                return;
            }

            DrawBackground();
            CalculateNodePositions();
            DrawNodesAndLabels();
            
            var visibleNodes = _mapNodes.Where(IsNodeVisible).ToList();
            DrawPaths(visibleNodes);
            UpdateDetailsArea();
            // PrintMapState(); // Uncomment to debug layout coordinates in the console
        }

        public void PrintMapState()
        {
            Debug.Log("--- Map State Snapshot ---");
            Debug.Log($"Nodes Count: {_mapNodes.Count}");
            foreach (MapNodeView node in _mapNodes)
            {
                Debug.Log($"- Node: {node.Id} (\"{node.DisplayName}\"), Position: {node.Position}, State: {node.Node.CurrentState}");
            }

            if (connectorTilemap != null)
            {
                BoundsInt bounds = connectorTilemap.cellBounds;
                List<string> connectionTiles = new List<string>();
                for (int x = bounds.xMin; x < bounds.xMax; x++)
                {
                    for (int y = bounds.yMin; y < bounds.yMax; y++)
                    {
                        Vector3Int pos = new Vector3Int(x, y, 0);
                        if (connectorTilemap.HasTile(pos))
                        {
                            connectionTiles.Add(pos.ToString());
                        }
                    }
                }
                Debug.Log($"Connection Tiles ({connectionTiles.Count}): {string.Join(", ", connectionTiles)}");
            }
            Debug.Log("-------------------------");
        }

        protected void DrawBackground()
        {
            if (backgroundTilemap == null || backgroundTile == null) return;
            backgroundTilemap.ClearAllTiles();
            for (int x = -64; x < 64; x++)
            {
                for (int y = -64; y < 64; y++)
                {
                    backgroundTilemap.SetTile(new Vector3Int(x, y, 0), backgroundTile);
                }
            }
        }

        protected void UpdateHoverView()
        {
            if (Camera.main == null || Mouse.current == null) return;

            Vector2 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            Vector3Int cellPosition = nodeTilemap.WorldToCell(mouseWorldPosition);
            MapNodeView currentNode = _mapNodes.Find(n => n.Position == (Vector2Int)cellPosition);

            if (currentNode != _hoveredNode)
            {
                connectorTilemap.ClearAllTiles();
                DrawPaths(_mapNodes.Where(IsNodeVisible).ToList());
                _hoveredNode = currentNode;
            }
        }

        protected void DrawPaths(List<MapNodeView> nodesToDraw)
        {
            foreach (MapNodeView nodeView in nodesToDraw)
            {
                if (nodeView.DependencyIds == null || nodeView.DependencyIds.Count == 0) continue;

                foreach (string depId in nodeView.DependencyIds)
                {
                    MapNodeView dep = _mapNodes.Find(n => n.Id == depId);
                    if (dep == null) continue;

                    // Ensure both nodes are in the nodesToDraw list (visible)
                    if (!nodesToDraw.Contains(dep)) continue;

                    DrawConnection((Vector3Int)dep.Position, (Vector3Int)nodeView.Position, nodeView.Node.Direction);
                }
            }
        }

        protected virtual bool IsNodeVisible(MapNodeView nodeView)
        {
            if (nodeView.Node.CurrentState == MapNodeState.Unlocked) return true;
            if (nodeView.Node.CurrentState == MapNodeState.Active) return true;
            if (nodeView.Node.CurrentState == MapNodeState.MetaLocked) return false;
            if (nodeView.DependencyIds == null || nodeView.DependencyIds.Count == 0) return true;

            // Visible if ALL dependencies are unlocked
            return nodeView.DependencyIds.All(depId => {
                MapNodeView dep = _mapNodes.Find(n => n.Id == depId);
                return dep != null && dep.Node.CurrentState == MapNodeState.Unlocked;
            });
        }

        protected void DrawConnection(Vector3Int start, Vector3Int end, MapNodeDirection direction)
        {
            List<Vector3Int> path = GetLShapePath(start, end, direction);
            if (IsPathBlocked(path, start, end))
            {
                path = FindPath(start, end);
            }
            if (path == null) path = GetLShapePath(start, end, direction);

            if (path != null)
            {
                int tilesPlaced = 0;
                foreach (Vector3Int p in path)
                {
                    if (p != start && p != end)
                    {
                        connectorTilemap.SetTile(p, connectorTile);
                        tilesPlaced++;
                    }
                }
               
            }
        }

        protected bool IsPathBlocked(List<Vector3Int> path, Vector3Int start, Vector3Int end)
        {
            if (path == null) return true;
            foreach (Vector3Int point in path)
            {
                if (point == start || point == end) continue;
                if (_mapNodes.Any(n => n.Position == (Vector2Int)point)) return true;
            }
            return false;
        }

        protected List<Vector3Int> GetLShapePath(Vector3Int start, Vector3Int end, MapNodeDirection direction)
        {
            List<Vector3Int> path = new List<Vector3Int> { start };
            int stepSize = 2;

            if (direction == MapNodeDirection.Left || direction == MapNodeDirection.Right)
            {
                int xDir = (direction == MapNodeDirection.Right) ? 1 : -1;
                int turnX = end.x - (xDir * stepSize);
                for (int x = start.x; x != turnX; x += xDir) path.Add(new Vector3Int(x + xDir, start.y, 0));
                int yDir = (end.y > start.y) ? 1 : -1;
                if (start.y != end.y) for (int y = start.y; y != end.y; y += yDir) path.Add(new Vector3Int(turnX, y + yDir, 0));
                for (int x = turnX; x != end.x; x += xDir) path.Add(new Vector3Int(x + xDir, end.y, 0));
            }
            else
            {
                int yDir = (direction == MapNodeDirection.Up) ? 1 : -1;
                int turnY = end.y - (yDir * stepSize);
                for (int y = start.y; y != turnY; y += yDir) path.Add(new Vector3Int(start.x, y + yDir, 0));
                int xDir = (end.x > start.x) ? 1 : -1;
                if (start.x != end.x) for (int x = start.x; x != end.x; x += xDir) path.Add(new Vector3Int(x + xDir, turnY, 0));
                for (int y = turnY; y != end.y; y += yDir) path.Add(new Vector3Int(end.x, y + yDir, 0));
            }
            return path;
        }

        protected List<Vector3Int> FindPath(Vector3Int start, Vector3Int end)
        {
            HashSet<Vector2Int> obstacles = new HashSet<Vector2Int>(_mapNodes.Select(n => n.Position));
            List<Vector3Int> openSet = new List<Vector3Int> { start };
            HashSet<Vector3Int> closedSet = new HashSet<Vector3Int>();
            Dictionary<Vector3Int, Vector3Int> cameFrom = new Dictionary<Vector3Int, Vector3Int>();
            Dictionary<Vector3Int, float> gScore = new Dictionary<Vector3Int, float> { [start] = 0 };
            Dictionary<Vector3Int, float> fScore = new Dictionary<Vector3Int, float> { [start] = Vector3Int.Distance(start, end) };

            while (openSet.Count > 0)
            {
                Vector3Int current = openSet.OrderBy(n => fScore.ContainsKey(n) ? fScore[n] : float.MaxValue).First();
                if (current == end) return ReconstructPath(cameFrom, current);

                openSet.Remove(current);
                closedSet.Add(current);

                foreach (Vector3Int neighbor in GetNeighbors(current))
                {
                    if (closedSet.Contains(neighbor)) continue;
                    if (obstacles.Contains((Vector2Int)neighbor) && neighbor != end) continue;

                    float tentativeGScore = gScore[current] + 1;
                    if (!gScore.ContainsKey(neighbor) || tentativeGScore < gScore[neighbor])
                    {
                        cameFrom[neighbor] = current;
                        gScore[neighbor] = tentativeGScore;
                        fScore[neighbor] = gScore[neighbor] + Vector3Int.Distance(neighbor, end);
                        if (!openSet.Contains(neighbor)) openSet.Add(neighbor);
                    }
                }
            }
            return null;
        }

        private List<Vector3Int> GetNeighbors(Vector3Int p)
        {
            return new List<Vector3Int> { p + Vector3Int.up, p + Vector3Int.down, p + Vector3Int.left, p + Vector3Int.right };
        }

        private List<Vector3Int> ReconstructPath(Dictionary<Vector3Int, Vector3Int> cameFrom, Vector3Int current)
        {
            List<Vector3Int> totalPath = new List<Vector3Int> { current };
            while (cameFrom.ContainsKey(current)) { current = cameFrom[current]; totalPath.Add(current); }
            totalPath.Reverse();
            return totalPath;
        }

        protected virtual void DrawNodesAndLabels()
        {
            int visibleCount = 0;
            foreach (MapNodeView nodeView in _mapNodes)
            {
                bool visible = IsNodeVisible(nodeView);
                if (!visible) 
                {
                   
                    continue;
                }
                visibleCount++;

                UnityEngine.Tilemaps.TileBase tile = nodeView.Node.GetTile();
                if (tile == null)
                {
                    Debug.LogWarning($"Node {nodeView.DisplayName} (Id: {nodeView.Id}) returned a null tile.");
                }

                if (nodeView.Position.x == -1000)
                {
                    Debug.LogWarning($"Node {nodeView.DisplayName} ({nodeView.Id}) has default position (-1000)!");
                }

                nodeTilemap.SetTile((Vector3Int)nodeView.Position, tile);

                if (nodeView.LabelInstance == null)
                {
                    Vector3 worldPos = nodeTilemap.GetCellCenterWorld((Vector3Int)nodeView.Position);
                    GameObject labelInstance = GameManager.Instance.prefabManager.Create("UITechTreeLabel",
                        worldPos + new Vector3(0, -1.5f, 0), nodeTilemap.transform.parent);
                    labelInstance.name = $"Node_{nodeView.DisplayName}";
                    nodeView.LabelInstance = labelInstance.GetComponentInChildren<TextMeshPro>();
                }
                
                if (nodeView.LabelInstance != null)
                {
                    nodeView.LabelInstance.text = nodeView.DisplayName;
                    nodeView.LabelInstance.gameObject.SetActive(true);
                }
                else
                {
                    Debug.LogError($"LabelInstance is null for node {nodeView.DisplayName}");
                }
            }
            nodeTilemap.RefreshAllTiles();
        }

        protected virtual void CenterTilemapOnCamera(bool zoomToFit = false)
        {
            var visibleNodes = _mapNodes.Where(IsNodeVisible).ToList();
            if (visibleNodes.Count == 0 || Camera.main == null) return;

            // Center on the first visible root node or the first visible node
            MapNodeView targetNode = visibleNodes.FirstOrDefault(n => n.DependencyIds == null || n.DependencyIds.Count == 0) ?? visibleNodes[0];

            Transform gridTransform = connectorTilemap.transform.parent;
            // Absolute local positioning: Snap grid so the target node's local position aligns with world (0,0)
            Vector3 localPos = nodeTilemap.GetCellCenterLocal((Vector3Int)targetNode.Position);
            Vector3 targetCenter = Vector3.zero;
            gridTransform.position = targetCenter - localPos;
            gridTransform.position = new Vector3(gridTransform.position.x, gridTransform.position.y, 0);

            float zoom = 10f;
            if (zoomToFit)
            {
                float maxAbsX = 0f;
                float maxAbsY = 0f;
                foreach (MapNodeView node in visibleNodes)
                {
                    // Calculate relative distance in local space
                    Vector3 nodeLocalPos = nodeTilemap.GetCellCenterLocal((Vector3Int)node.Position);
                    Vector3 relativePos = nodeLocalPos - localPos;
                    maxAbsX = Mathf.Max(maxAbsX, Mathf.Abs(relativePos.x));
                    maxAbsY = Mathf.Max(maxAbsY, Mathf.Abs(relativePos.y));
                }

                float padding = 4f;
                float aspect = Camera.main.aspect;
                zoom = Mathf.Max(maxAbsY + padding, (maxAbsX + padding) / aspect);
                zoom = Mathf.Clamp(zoom, minZoom, maxZoom);
            }

            GameManager.Instance.cameraController.SnapTo(targetCenter, zoom);
        }

        // Procedural Layout Logic
        protected class LayoutNode
        {
            public MapNodeView View;
            public List<LayoutNode> Children = new List<LayoutNode>();
            public Vector2Int Position;
            public int MinX, MaxX, MinY, MaxY; // Subtree bounding box relative to this node
        }

        protected void CalculateNodePositions()
        {
            if (_mapNodes.Count == 0) return;
            foreach (var node in _mapNodes) node.Position = new Vector2Int(-1000, -1000);

            // Find all roots (nodes with no dependencies)
            var roots = _mapNodes.Where(n => n.Node.DependencyIds == null || n.Node.DependencyIds.Count == 0).ToList();
            
            // Special case for tech tree root if it exists
            MapNodeView techRoot = _mapNodes.Find(n => n.Node.Id == "application-server");
            if (techRoot != null && !roots.Contains(techRoot))
            {
                roots.Insert(0, techRoot);
            }

            if (roots.Count == 0)
            {
                Debug.LogError("CalculateNodePositions: Could not find any root nodes.");
                return;
            }


            HashSet<string> visited = new HashSet<string>();
            int currentYOffset = 0;

            foreach (var root in roots)
            {
                if (visited.Contains(root.Id)) continue;

                LayoutNode rootLayout = BuildLayoutTree(root, visited);
                CalculateSubtreeMetrics(rootLayout);
                
                Vector2Int rootPos = new Vector2Int(0, currentYOffset);
                AssignPositions(rootLayout, rootPos);
                
                // Adjust Y offset for the next root based on the breadth of this tree
                int treeBreadth = Mathf.Max(rowSpacing * 2, rootLayout.MaxY - rootLayout.MinY + rowSpacing * 2);
                currentYOffset += treeBreadth;
            }
            
            
        }

        private LayoutNode BuildLayoutTree(MapNodeView view, HashSet<string> visited)
        {
            visited.Add(view.Id);
            LayoutNode node = new LayoutNode { View = view };
            var sortedChildren = _mapNodes.Where(n => n.DependencyIds != null && n.DependencyIds.Contains(view.Id))
                .OrderBy(n => n.Id).ToList();

            foreach (var childView in sortedChildren)
            {
                if (!visited.Contains(childView.Id)) node.Children.Add(BuildLayoutTree(childView, visited));
            }
            return node;
        }

        private void CalculateSubtreeMetrics(LayoutNode node)
        {
            // Initialize bounds to include just the current node
            node.MinX = node.MaxX = node.MinY = node.MaxY = 0;

            if (node.Children.Count == 0) return;

            var grouped = node.Children.GroupBy(c => c.View.Node.Direction);
            foreach (var group in grouped)
            {
                MapNodeDirection dir = group.Key;
                int perpSpacing = (dir == MapNodeDirection.Left || dir == MapNodeDirection.Right) ? rowSpacing * 2 : columnSpacing;
                List<LayoutNode> children = group.ToList();
                
                float totalBreadth = 0;
                List<float> childBreadths = new List<float>();

                foreach (var child in children)
                {
                    CalculateSubtreeMetrics(child);
                    // Determine breadth perpendicular to growth direction
                    float b = (dir == MapNodeDirection.Up || dir == MapNodeDirection.Down) 
                        ? Mathf.Max(2f, child.MaxX - child.MinX) 
                        : Mathf.Max(2f, child.MaxY - child.MinY);
                    childBreadths.Add(b);
                    totalBreadth += b;
                }
                totalBreadth += (children.Count - 1) * perpSpacing;

                int mainDistModifier = (children.Count > 1) ? 2 : 0;
                int baseMainDist = ((dir == MapNodeDirection.Up || dir == MapNodeDirection.Down) ? rowSpacing * 2 : columnSpacing) + mainDistModifier;
                
                // Collision avoidance: Push the entire group further until its bounding box is clear of existing node content
                int pushOffset = 0;
                bool collision = true;
                int safetyGuard = 0;
                int padding = 2;

                while (collision && safetyGuard < 50)
                {
                    safetyGuard++;
                    collision = false;
                    int currentMainDist = baseMainDist + pushOffset;
                    Vector2Int dirVec = GetDirectionVector(dir);
                    Vector2Int perpVec = GetPerpendicularVector(dir);
                    float groupStartPos = -totalBreadth / 2f;

                    // Check each child in the group at its potential position
                    for (int i = 0; i < children.Count; i++)
                    {
                        var child = children[i];
                        float centerOffset = groupStartPos + childBreadths[i] / 2f;
                        Vector2Int potentialPos = dirVec * currentMainDist + perpVec * Mathf.RoundToInt(centerOffset);
                        
                        // Check if child subtree bounds at this position overlap with current node subtree bounds
                        if (RectsOverlap(
                            child.MinX + potentialPos.x, child.MaxX + potentialPos.x,
                            child.MinY + potentialPos.y, child.MaxY + potentialPos.y,
                            node.MinX, node.MaxX, node.MinY, node.MaxY, padding))
                        {
                            collision = true;
                            pushOffset += 2;
                            break;
                        }
                        groupStartPos += childBreadths[i] + perpSpacing;
                    }
                }

                // Finalize positions and merge into node bounds
                int finalMainDist = baseMainDist + pushOffset;
                float currentPos = -totalBreadth / 2f;
                for (int i = 0; i < children.Count; i++)
                {
                    var child = children[i];
                    float centerOffset = currentPos + childBreadths[i] / 2f;
                    child.Position = GetDirectionVector(dir) * finalMainDist + GetPerpendicularVector(dir) * Mathf.RoundToInt(centerOffset);
                    
                    // Merge child subtree bounds into node bounds
                    node.MinX = Mathf.Min(node.MinX, child.MinX + child.Position.x);
                    node.MaxX = Mathf.Max(node.MaxX, child.MaxX + child.Position.x);
                    node.MinY = Mathf.Min(node.MinY, child.MinY + child.Position.y);
                    node.MaxY = Mathf.Max(node.MaxY, child.MaxY + child.Position.y);
                    
                    currentPos += childBreadths[i] + perpSpacing;
                }
            }
        }

        private bool RectsOverlap(int minX1, int maxX1, int minY1, int maxY1, int minX2, int maxX2, int minY2, int maxY2, int padding)
        {
            return minX1 - padding <= maxX2 && maxX1 + padding >= minX2 &&
                   minY1 - padding <= maxY2 && maxY1 + padding >= minY2;
        }

        private void AssignPositions(LayoutNode node, Vector2Int currentPos)
        {
            node.View.Position = currentPos;
            foreach (var child in node.Children) AssignPositions(child, currentPos + child.Position);
        }

        private Vector2Int GetDirectionVector(MapNodeDirection dir) => dir switch { MapNodeDirection.Up => Vector2Int.up, MapNodeDirection.Down => Vector2Int.down, MapNodeDirection.Left => Vector2Int.left, MapNodeDirection.Right => Vector2Int.right, _ => Vector2Int.right };
        private Vector2Int GetPerpendicularVector(MapNodeDirection dir) => (dir == MapNodeDirection.Up || dir == MapNodeDirection.Down) ? Vector2Int.right : Vector2Int.up;

        public MapNodeView GetNodeById(string id)
        {
            return _mapNodes.FirstOrDefault(n => n.Id == id);
        }
    }
}