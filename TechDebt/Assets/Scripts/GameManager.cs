// GameManager.cs
using UnityEngine;
using UnityEngine.Tilemaps;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GridManager gridManager; // Reference to our GridManager
    [SerializeField] private TileBase floorTile; // Assign a tile from ImportedSprites in Editor
    [SerializeField] private GameObject npcDevOpsPrefab; // Assign NPCDevOps prefab in Editor
    [SerializeField] private GameObject serverPrefab; // Assign Server prefab in Editor

    void Start()
    {
        Debug.Log("GameManager Start() called.");
        SetupGameScene();
    }

    void SetupGameScene()
    {
        Debug.Log("GameManager: Starting scene setup...");

        // --- Reference Checks ---
        if (floorTile == null) {
            Debug.LogError("FATAL: Floor Tile is not assigned in the GameManager Inspector!");
            return;
        }
        if (npcDevOpsPrefab == null) {
            Debug.LogError("FATAL: NPCDevOps Prefab is not assigned in the GameManager Inspector!");
            return;
        }
        if (serverPrefab == null) {
            Debug.LogError("FATAL: Server Prefab is not assigned in the GameManager Inspector!");
            return;
        }

        // --- GridManager Setup ---
        if (gridManager == null)
        {
            Debug.Log("GridManager not found in scene, creating one.");
            GameObject gridManagerObject = new GameObject("GridManager");
            gridManager = gridManagerObject.AddComponent<GridManager>();
        }
        
        gridManager.tilePrefab = floorTile as Tile;
        if (gridManager.tilePrefab == null) {
            Debug.LogError("FATAL: The 'floorTile' asset could not be cast to a 'Tile'. Please make sure you have assigned a Tile asset, not a Sprite, in the GameManager's Inspector.");
            return;
        }
        
        gridManager.CreateGrid();
        Debug.Log("Grid created successfully.");

        // --- Initial Camera Setup ---
        // The new CameraController script will handle user movement.
        // This just sets the initial position and zoom.
        Camera mainCamera = Camera.main;
        if (mainCamera != null && gridManager.gridComponent != null)
        {
            Vector3Int centerCell = new Vector3Int(gridManager.gridWidth / 2, gridManager.gridHeight / 2, 0);
            Vector3 centerWorldPos = gridManager.gridComponent.CellToWorld(centerCell);
            mainCamera.transform.position = new Vector3(centerWorldPos.x, centerWorldPos.y, mainCamera.transform.position.z);
            mainCamera.orthographicSize = 5f; 
            Debug.Log($"Camera initially centered at world position ({centerWorldPos.x}, {centerWorldPos.y}).");
        }

        // --- Object Placement ---
        Debug.Log("Placing NPCDevOps...");
        Instantiate(npcDevOpsPrefab, gridManager.gridComponent.CellToWorld(new Vector3Int(3, 3, 0)), Quaternion.identity);
        Instantiate(npcDevOpsPrefab, gridManager.gridComponent.CellToWorld(new Vector3Int(4, 5, 0)), Quaternion.identity);
        Debug.Log("NPCDevOps placed.");

        Debug.Log("Placing Servers...");
        Instantiate(serverPrefab, gridManager.gridComponent.CellToWorld(new Vector3Int(8, 8, 0)), Quaternion.identity);
        Instantiate(serverPrefab, gridManager.gridComponent.CellToWorld(new Vector3Int(9, 6, 0)), Quaternion.identity);
        Debug.Log("Servers placed.");

        Debug.Log("GameManager: Scene setup complete.");
    }
}
