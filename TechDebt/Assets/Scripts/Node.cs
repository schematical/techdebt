// Node.cs
using UnityEngine;

public class Node
{
    public int gridX, gridY;
    public bool isWalkable;
    public Vector3 worldPosition;

    public int gCost; // Cost from the starting node
    public int hCost; // Heuristic cost to the end node
    public Node parent; // The previous node in the path

    public int fCost { get { return gCost + hCost; } }

    public Node(bool _isWalkable, Vector3 _worldPos, int _gridX, int _gridY)
    {
        isWalkable = _isWalkable;
        worldPosition = _worldPos;
        gridX = _gridX;
        gridY = _gridY;
    }
}
