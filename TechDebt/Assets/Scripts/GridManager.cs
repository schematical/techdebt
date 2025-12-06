// GridManager.cs
using UnityEngine;
using UnityEngine.Tilemaps;

public class GridManager : MonoBehaviour
{
    // Shrunk the grid as requested and made these public for easier access.
    [SerializeField] public int gridWidth = 12;
    [SerializeField] public int gridHeight = 12;
    
    public Tile tilePrefab;
    
    // Public reference to the Grid component so other scripts can access it.
    public Grid gridComponent { get; private set; }

    void Start()
    {
        Debug.Log("GridManager Start() called.");
    }

    public void CreateGrid()
    {
        Debug.Log("GridManager: Starting grid creation...");

        if (tilePrefab == null)
        {
            Debug.LogError("FATAL: Tile Prefab is not assigned in the GridManager! Cannot create grid.");
            return;
        }

        GameObject gridObject = new GameObject("Grid");
        gridComponent = gridObject.AddComponent<Grid>(); // Store the reference
        gridComponent.cellLayout = GridLayout.CellLayout.Isometric;
        gridComponent.cellSize = new Vector3(1, 0.5f, 1);

        GameObject tilemapObject = new GameObject("Tilemap");
        tilemapObject.transform.SetParent(gridObject.transform);
        Tilemap tilemap = tilemapObject.AddComponent<Tilemap>();
        tilemapObject.AddComponent<TilemapRenderer>();
        
        Debug.Log($"Creating a {gridWidth}x{gridHeight} tilemap...");

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                tilemap.SetTile(new Vector3Int(x, y, 0), tilePrefab);
            }
        }
        
        Debug.Log("GridManager: Grid creation complete.");
    }
}
