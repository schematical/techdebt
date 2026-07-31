// Pathfinding.cs

using System;
using UnityEngine;
using System.Collections.Generic;
using DefaultNamespace.Office;
using UnityEngine.Tilemaps;

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
        if (!targetNode.IsWalkable())
        {
            throw new System.Exception($"Target node at {targetWorldPos} is not walkable.");
        }

        List<Node> openSet = new List<Node>();
        HashSet<Node> closedSet = new HashSet<Node>();
        openSet.Add(startNode);
        int saftyCheck = 10000;
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

            foreach (Node neighbour in currentNode.GetNeighbours())
            {
                saftyCheck -= 1;
                if (saftyCheck < 0)
                {
                    throw new SystemException("We have maxed out the loop");
                }
                if (!neighbour.IsWalkable() || closedSet.Contains(neighbour))
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
            worldPath.Add(
                GameManager.Instance.gridManager.AdjustWorldPointZ(node.worldPosition)
            );
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
public class Node
{
    public int gridX, gridY;

    public Vector3 worldPosition;

    public int gCost; // Cost from the starting node
    public int hCost; // Heuristic cost to the end node
    public Node parent; // The previous node in the path

    public int fCost { get { return gCost + hCost; } }

    public Node(/* Vector3 _worldPos,*/ int _gridX, int _gridY)
    {
     
        // worldPosition = _worldPos;
        gridX = _gridX;
        gridY = _gridY;
    }

    public bool IsWalkable()
    {
        foreach (RoomBase roomBase in GameManager.Instance.Rooms)
        {
            if (roomBase.State == RoomBase.RoomState.Active)
            {
                TileBase tile = roomBase.WallTilemap.GetTile(new Vector3Int(gridX, gridY, 0));
                if (tile != null)
                {
                    // Debug.Log($"Found Tile: {gridX}, {gridY}");
                    return false;
                }
            }
        }
        TileBase skyTile = GameManager.Instance.gridManager.skyTilemap.GetTile(new Vector3Int(gridX, gridY, 0));
        if (skyTile != null)
        {
            // Debug.Log($"Found skyTile: {gridX}, {gridY}");
            return false;
        }
        TileBase floorTile = GameManager.Instance.gridManager.floorTilemap.GetTile(new Vector3Int(gridX, gridY, 0));
        bool isWalkable =  floorTile != null;
        if (floorTile == null)
        {
            Debug.Log($"Missed floor tile at  {gridX}, {gridY} this is a good thing. " + isWalkable);
        }
        return isWalkable;
    }
    public List<Node> GetNeighbours()
    {
        List<Node> neighbours = new List<Node>();
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0) continue;

                int checkX = gridX + x;
                int checkY = gridY + y;

                Node node = new Node(checkX, checkY);
                TileBase floorTile = GameManager.Instance.gridManager.floorTilemap.GetTile(new Vector3Int(gridX, gridY, 0));
                if (floorTile != null)
                {
                    if (checkX >= 0 && checkX < 64 && checkY >= 0 && checkY < 64)
                    {

                        neighbours.Add(node);
                    }
                    else
                    {
                        Debug.Log($"FAIL: Found {floorTile} at {checkX}, {checkY}");
                    }
                }
                    
                
            }
        }
        return neighbours;
    }
}
