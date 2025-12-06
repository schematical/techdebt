// GameManager.cs
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static List<Server> AllServers = new List<Server>();

    [SerializeField] private GridManager gridManager;
    [SerializeField] private TileBase floorTile;
    [SerializeField] private GameObject npcDevOpsPrefab;
    [SerializeField] private GameObject serverPrefab;

    void Start()
    {
        AllServers.Clear(); // Clear list on start
        SetupGameScene();
    }

    void SetupGameScene()
    {
        if (floorTile == null || npcDevOpsPrefab == null || serverPrefab == null)
        {
            Debug.LogError("FATAL: A prefab or tile is not assigned in the GameManager Inspector!");
            return;
        }

        if (gridManager == null)
        {
            gridManager = FindObjectOfType<GridManager>();
            if (gridManager == null)
            {
                 Debug.Log("GridManager not found in scene, creating one.");
                 GameObject gridManagerObject = new GameObject("GridManager");
                 gridManager = gridManagerObject.AddComponent<GridManager>();
            }
        }
        
        gridManager.tilePrefab = floorTile as Tile;
        gridManager.CreateGrid();
        
        Camera mainCamera = Camera.main;
        if (mainCamera != null && gridManager.gridComponent != null)
        {
            Vector3Int centerCell = new Vector3Int(gridManager.gridWidth / 2, gridManager.gridHeight / 2, 0);
            Vector3 centerWorldPos = gridManager.gridComponent.CellToWorld(centerCell);
            mainCamera.transform.position = new Vector3(centerWorldPos.x, centerWorldPos.y, mainCamera.transform.position.z);
            mainCamera.orthographicSize = 5f; 
        }

        // Place Servers and add them to the static list
        PlaceServer(8, 8);
        PlaceServer(9, 6);
        PlaceServer(2, 7);

        // Place NPCDevOps at random starting positions
        PlaceNPCDevOpsAtRandom(0);
        PlaceNPCDevOpsAtRandom(1);
    }

    void PlaceServer(int x, int y)
    {
        Vector3 worldPos = gridManager.gridComponent.CellToWorld(new Vector3Int(x, y, 0));
        GameObject serverObj = Instantiate(serverPrefab, worldPos, Quaternion.identity);
        AllServers.Add(serverObj.GetComponent<Server>());
        Debug.Log($"Placed Server at grid cell ({x},{y}).");
    }

    void PlaceNPCDevOpsAtRandom(int devIndex)
    {
        int randomX = Random.Range(0, gridManager.gridWidth);
        int randomY = Random.Range(0, gridManager.gridHeight);
        Vector3 worldPos = gridManager.gridComponent.CellToWorld(new Vector3Int(randomX, randomY, 0));
        Instantiate(npcDevOpsPrefab, worldPos, Quaternion.identity);
        Debug.Log($"Placed NPCDevOps {devIndex} at random grid cell ({randomX},{randomY}).");
    }
}