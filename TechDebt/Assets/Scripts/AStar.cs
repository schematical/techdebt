using System.Collections.Generic;
using UnityEngine;

public static class AStar
{
    private class Node
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
    }

    public static List<Vector2Int> FindPath(Vector2Int start, Vector2Int end, HashSet<Vector2Int> walkableNodes)
    {
        var openList = new List<Node>();
        var closedList = new HashSet<Node>();

        var startNode = new Node(start);
        var endNode = new Node(end);

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
            closedList.Add(currentNode);

            if (currentNode.Position == endNode.Position)
            {
                return RetracePath(startNode, currentNode);
            }

            foreach (var neighbor in GetNeighbors(currentNode, walkableNodes))
            {
                if (closedList.Contains(neighbor))
                {
                    continue;
                }

                var newMovementCostToNeighbor = currentNode.GCost + GetDistance(currentNode, neighbor);
                if (newMovementCostToNeighbor < neighbor.GCost || !openList.Contains(neighbor))
                {
                    neighbor.GCost = newMovementCostToNeighbor;
                    neighbor.HCost = GetDistance(neighbor, endNode);
                    neighbor.Parent = currentNode;

                    if (!openList.Contains(neighbor))
                    {
                        openList.Add(neighbor);
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

    private static IEnumerable<Node> GetNeighbors(Node node, HashSet<Vector2Int> walkableNodes)
    {
        for (var x = -1; x <= 1; x++)
        {
            for (var y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0 || x != 0 && y != 0)
                {
                    continue;
                }

                var neighborPosition = new Vector2Int(node.Position.x + x, node.Position.y + y);
                if (walkableNodes.Contains(neighborPosition))
                {
                    yield return new Node(neighborPosition);
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
