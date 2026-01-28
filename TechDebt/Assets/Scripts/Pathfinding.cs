// Pathfinding.cs
using UnityEngine;
using System.Collections.Generic;

public static class Pathfinding
{
    public static List<Vector3> FindPath(Vector3 startWorldPos, Vector3 targetWorldPos)
    {
        if (GridManager.Instance == null)
        {
            throw new System.Exception("Pathfinding requires a GridManager instance.");
        }

        Node startNode = GridManager.Instance.NodeFromWorldPoint(startWorldPos);
        Node targetNode = GridManager.Instance.NodeFromWorldPoint(targetWorldPos);

        if (startNode == null)
        {
            throw new System.Exception($"Start node is null for world position {startWorldPos}");
        }
        if (targetNode == null)
        {
            throw new System.Exception($"Target node is null for world position {targetWorldPos}");
        }
        if (!targetNode.isWalkable)
        {
            throw new System.Exception($"Target node at {targetWorldPos} is not walkable.");
        }

        List<Node> openSet = new List<Node>();
        HashSet<Node> closedSet = new HashSet<Node>();
        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            Node currentNode = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].fCost < currentNode.fCost || (openSet[i].fCost == currentNode.fCost && openSet[i].hCost < currentNode.hCost))
                {
                    currentNode = openSet[i];
                }
            }

            openSet.Remove(currentNode);
            closedSet.Add(currentNode);

            if (currentNode == targetNode)
            {
                return RetracePath(startNode, targetNode, targetWorldPos);
            }

            foreach (Node neighbour in GridManager.Instance.GetNeighbours(currentNode))
            {
                if (!neighbour.isWalkable || closedSet.Contains(neighbour))
                {
                    continue;
                }

                int newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour);
                if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                {
                    neighbour.gCost = newMovementCostToNeighbour;
                    neighbour.hCost = GetDistance(neighbour, targetNode);
                    neighbour.parent = currentNode;

                    if (!openSet.Contains(neighbour))
                    {
                        openSet.Add(neighbour);
                    }
                }
            }
        }
        throw new System.Exception($"No path found from {startWorldPos} to {targetWorldPos}");
    }

    private static List<Vector3> RetracePath(Node startNode, Node endNode, Vector3 targetWorldPos)
    {
        List<Node> path = new List<Node>();
        Node currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }
        
        List<Vector3> worldPath = new List<Vector3>();
        foreach(var node in path)
        {
            worldPath.Add(node.worldPosition);
        }

        worldPath.Reverse();
        worldPath.Add(targetWorldPos);
        return worldPath;
    }

    private static int GetDistance(Node nodeA, Node nodeB)
    {
        int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

        if (dstX > dstY)
            return 14 * dstY + 10 * (dstX - dstY);
        return 14 * dstX + 10 * (dstY - dstX);
    }
}