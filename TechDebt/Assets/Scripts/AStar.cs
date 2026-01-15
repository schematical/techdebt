using System;
using System.Collections.Generic;
using UnityEngine;

public static class AStar
{
    private class Node : IEquatable<Node>
    {
        public Vector2Int Position { get; }
        public int GCost { get; set; }
        public int HCost { get; set; }
        public int FCost => GCost + HCost;
        public Node Parent { get; set; }

        public Node(Vector2Int position)
        {
            Position = position;
        }

        public bool Equals(Node other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Position.Equals(other.Position);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Node)obj);
        }

        public override int GetHashCode()
        {
            return Position.GetHashCode();
        }
    }

    public static List<Vector2Int> FindPath(Vector2Int start, Vector2Int end, HashSet<Vector2Int> walkableNodes)
    {
        var openList = new List<Node>();
        var closedSet = new HashSet<Vector2Int>();
        var nodeMap = new Dictionary<Vector2Int, Node>();

        var startNode = new Node(start);
        var endNode = new Node(end);
        
        nodeMap[start] = startNode;

        openList.Add(startNode);

        while (openList.Count > 0)
        {
            var currentNode = openList[0];
            for (var i = 1; i < openList.Count; i++)
            {
                if (openList[i].FCost < currentNode.FCost || openList[i].FCost == currentNode.FCost && openList[i].HCost < currentNode.HCost)
                {
                    currentNode = openList[i];
                }
            }

            openList.Remove(currentNode);
            closedSet.Add(currentNode.Position);

            if (currentNode.Position == endNode.Position)
            {
                return RetracePath(startNode, currentNode);
            }

            foreach (var neighborPosition in GetNeighbors(currentNode, walkableNodes, startNode, endNode))
            {
                if (closedSet.Contains(neighborPosition))
                {
                    continue;
                }

                if (!nodeMap.TryGetValue(neighborPosition, out var neighborNode))
                {
                    neighborNode = new Node(neighborPosition);
                    nodeMap[neighborPosition] = neighborNode;
                }
                
                var newMovementCostToNeighbor = currentNode.GCost + GetDistance(currentNode, neighborNode);
                if (newMovementCostToNeighbor < neighborNode.GCost || !openList.Contains(neighborNode))
                {
                    neighborNode.GCost = newMovementCostToNeighbor;
                    neighborNode.HCost = GetDistance(neighborNode, endNode);
                    neighborNode.Parent = currentNode;

                    if (!openList.Contains(neighborNode))
                    {
                        openList.Add(neighborNode);
                    }
                }
            }
        }

        return null;
    }

    private static List<Vector2Int> RetracePath(Node startNode, Node endNode)
    {
        var path = new List<Vector2Int>();
        var currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode.Position);
            currentNode = currentNode.Parent;
        }

        path.Reverse();
        return path;
    }

    private static IEnumerable<Vector2Int> GetNeighbors(Node node, HashSet<Vector2Int> walkableNodes, Node startNode, Node endNode)
    {
        bool isStartOrEndNode = node.Position == startNode.Position || node.Position == endNode.Position;

        for (var x = -1; x <= 1; x++)
        {
            for (var y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0 || x != 0 && y != 0)
                {
                    continue;
                }
                
                if (isStartOrEndNode && y != 0)
                {
                    continue;
                }

                var neighborPosition = new Vector2Int(node.Position.x + x, node.Position.y + y);
                if (walkableNodes.Contains(neighborPosition))
                {
                    yield return neighborPosition;
                }
            }
        }
    }

    private static int GetDistance(Node nodeA, Node nodeB)
    {
        var dstX = Mathf.Abs(nodeA.Position.x - nodeB.Position.x);
        var dstY = Mathf.Abs(nodeA.Position.y - nodeB.Position.y);

        return 10 * (dstX + dstY);
    }
}
