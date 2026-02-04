// GridManager.cs
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using UnityEngine.Serialization;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    public int gridWidth = 64;
    public int gridHeight = 64;
    
   public Tile floorTilePrefab;
   public Tile shadowTilePrefab;
   public IsometricRuleTile skyTilePrefab;
    public Grid grid { get; private set; }
    private Node[,] nodeGrid;
    public Tilemap floorTilemap;
    public Tilemap skyTilemap;

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

    public void Init()
    {
        nodeGrid = new Node[gridWidth, gridHeight];
        grid = GetComponent<Grid>();
        grid.cellLayout = GridLayout.CellLayout.Isometric;
        grid.cellSize = new Vector3(1, 0.5f, 1);


        int border = 20;
        for (int x = 0 - border; x < gridWidth + border; x++)
        {
            for (int y = 0 - border; y < gridHeight + border; y++)
            {
                Vector3Int cellPosition = new Vector3Int(x, y, 0);
                if (x < 0 || x >= gridWidth || y < 0 || y >= gridHeight)
                {
                    skyTilemap.SetTile(cellPosition, skyTilePrefab);   
                }
                else
                {
                    floorTilemap.SetTile(cellPosition, floorTilePrefab);
                    Vector3 worldPoint = grid.CellToWorld(cellPosition);
                    // For now, all tiles are walkable
                    nodeGrid[x, y] = new Node(true, worldPoint, x, y);
                }

     
            }
        }
    }

    public void UpdateTileState(Vector3Int pos, bool isWalkable)
    {
        Debug.Log($"Setting {pos} to {isWalkable}");
        floorTilemap.SetTile(pos, isWalkable ? floorTilePrefab : shadowTilePrefab);
        nodeGrid[pos.x, pos.y].isWalkable = isWalkable;
    }

    public Node NodeFromWorldPoint(Vector3 worldPosition)
    {
        if (nodeGrid == null)
        {
            throw new System.Exception("Node grid is not initialized! Cannot find node.");
        }
        
        Vector3Int cellPos = grid.WorldToCell(worldPosition);

        // Clamp the cell position to be within the grid bounds
        cellPos.x = Mathf.Clamp(cellPos.x, 5, gridWidth - 6);
        cellPos.y = Mathf.Clamp(cellPos.y, 5, gridHeight - 6);
       
        return nodeGrid[cellPos.x, cellPos.y];
    }

    public Vector3 AdjustWorldPointZ(Vector3 worldPosition)
    {
        // Vector3 screenPoint = Camera.main.WorldToScreenPoint(worldPosition);
        Vector3 newWorldPoint = new Vector3(
            worldPosition.x,
            worldPosition.y,
            worldPosition.y / 1000
        );
        return newWorldPoint;
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