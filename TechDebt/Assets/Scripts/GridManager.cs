// GridManager.cs
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    [SerializeField] public int gridWidth = 64;
    [SerializeField] public int gridHeight = 64;
    
    public Tile tilePrefab;
    public Grid gridComponent { get; private set; }
    
    private Node[,] nodeGrid;
    private Tilemap tilemap;

    void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    public void CreateGrid()
    {
        nodeGrid = new Node[gridWidth, gridHeight];

        GameObject gridObject = new GameObject("Grid");
        gridComponent = gridObject.AddComponent<Grid>();
        gridComponent.cellLayout = GridLayout.CellLayout.Isometric;
        gridComponent.cellSize = new Vector3(1, 0.5f, 1);

        GameObject tilemapObject = new GameObject("Tilemap");
        tilemapObject.transform.SetParent(gridObject.transform);
        tilemap = tilemapObject.AddComponent<Tilemap>();
        tilemapObject.AddComponent<TilemapRenderer>();
        
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                Vector3Int cellPosition = new Vector3Int(x, y, 0);
                tilemap.SetTile(cellPosition, tilePrefab);
                Vector3 worldPoint = gridComponent.CellToWorld(cellPosition);
                // For now, all tiles are walkable
                nodeGrid[x, y] = new Node(true, worldPoint, x, y);
            }
        }
    }

    public Node NodeFromWorldPoint(Vector3 worldPosition)
    {
        if (nodeGrid == null)
        {
            throw new System.Exception("Node grid is not initialized! Cannot find node.");
        }
        
        Vector3Int cellPos = gridComponent.WorldToCell(worldPosition);

        // Clamp the cell position to be within the grid bounds
        cellPos.x = Mathf.Clamp(cellPos.x, 0, gridWidth - 1);
        cellPos.y = Mathf.Clamp(cellPos.y, 0, gridHeight - 1);

        return nodeGrid[cellPos.x, cellPos.y];
    }

    public List<Node> GetNeighbours(Node node)
    {
        List<Node> neighbours = new List<Node>();
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0) continue;

                int checkX = node.gridX + x;
                int checkY = node.gridY + y;

                if (checkX >= 0 && checkX < gridWidth && checkY >= 0 && checkY < gridHeight)
                {
                    neighbours.Add(nodeGrid[checkX, checkY]);
                }
            }
        }
        return neighbours;
    }
}