// GameManager.cs
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Linq;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public static List<Server> AllServers = new List<Server>(); // Keep for existing Server.cs references

    // List to define all infrastructure in the game
    public List<InfrastructureData> AllInfrastructure;

    // Dictionary to hold all game stats
    public Dictionary<StatType, float> Stats { get; private set; }
    public static event System.Action OnStatsChanged;


    [SerializeField] private GridManager gridManager;
    [SerializeField] private TileBase floorTile;
    [SerializeField] private GameObject npcDevOpsPrefab; // Still needed for hiring NPCDevOps

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            InitializeStats();
        }
    }

    void Start()
    {
        // Ensure Managers exist
        if (FindObjectOfType<GameLoopManager>() == null)
        {
            gameObject.AddComponent<GameLoopManager>();
        }
        if (FindObjectOfType<UIManager>() == null)
        {
            gameObject.AddComponent<UIManager>();
        }

        AllServers.Clear();
        SetupGameScene();
    }

    private void InitializeStats()
    {
        Stats = new Dictionary<StatType, float>();
        Stats.Add(StatType.Money, 1000f);
        Stats.Add(StatType.EngineeringHours, 50f);
        Stats.Add(StatType.TechDebt, 0f);
        Stats.Add(StatType.ResearchPoints, 0f);
    }

    public void AddStat(StatType stat, float value)
    {
        if (Stats.ContainsKey(stat))
        {
            Stats[stat] += value;
            OnStatsChanged?.Invoke();
        }
    }

    public bool TrySpendStat(StatType stat, float value)
    {
        if (Stats.ContainsKey(stat) && Stats[stat] >= value)
        {
            Stats[stat] -= value;
            OnStatsChanged?.Invoke();
            return true;
        }
        return false;
    }

    public float GetStat(StatType stat)
    {
        return Stats.ContainsKey(stat) ? Stats[stat] : 0f;
    }

    void SetupGameScene()
    {
        if (floorTile == null || npcDevOpsPrefab == null)
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

        // Place initial infrastructure based on AllInfrastructure list
        foreach (var infra in AllInfrastructure)
        {
            if (infra.IsInitiallyUnlocked)
            {
                UnlockInfrastructure(infra);
            }
        }

        // Hire one initial NPCDevOps
        HireNPCDevOps(new NPCDevOpsData { DailyCost = 100 }); // Start with one for a fixed price
    }

    public void UnlockInfrastructure(InfrastructureData infraData)
    {
        if (infraData.IsUnlockedInGame)
        {
            Debug.LogWarning($"{infraData.DisplayName} is already unlocked.");
            return;
        }

        if (TrySpendStat(StatType.Money, infraData.UnlockCost))
        {
            Vector3 worldPos = gridManager.gridComponent.CellToWorld(new Vector3Int(infraData.GridPosition.x, infraData.GridPosition.y, 0));
            GameObject infraInstance = Instantiate(infraData.Prefab, worldPos, Quaternion.identity);
            infraData.Instance = infraInstance;
            infraData.IsUnlockedInGame = true;

            Server serverComponent = infraInstance.GetComponent<Server>();
            if (serverComponent != null)
            {
                AllServers.Add(serverComponent);
            }

            Debug.Log($"Unlocked {infraData.DisplayName} for ${infraData.UnlockCost}.");
            UIManager.Instance.RefreshBuildUI();
        }
        else
        {
            Debug.Log($"Not enough money to unlock {infraData.DisplayName}.");
        }
    }

    public List<NPCDevOpsData> GenerateNPCCandidates(int count)
    {
        var candidates = new List<NPCDevOpsData>();
        for (int i = 0; i < count; i++)
        {
            candidates.Add(new NPCDevOpsData
            {
                DailyCost = Random.Range(100, 201) // Random cost between 100 and 200
            });
        }
        return candidates;
    }

    public void HireNPCDevOps(NPCDevOpsData candidateData)
    {
        // For now, hiring is free beyond the daily cost, but you could add a hiring fee here.
        int randomX = Random.Range(0, gridManager.gridWidth);
        int randomY = Random.Range(0, gridManager.gridHeight);
        Vector3 worldPos = gridManager.gridComponent.CellToWorld(new Vector3Int(randomX, randomY, 0));
        
        GameObject npcObject = Instantiate(npcDevOpsPrefab, worldPos, Quaternion.identity);
        var npc = npcObject.GetComponent<NPCDevOps>();
        if (npc != null)
        {
            npc.Initialize(candidateData);
            Debug.Log($"Hired {npc.Data.Name} for a salary of ${npc.Data.DailyCost}/day.");
        }
    }
}
