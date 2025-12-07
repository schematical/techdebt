// GameManager.cs
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.EventSystems;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public static List<Server> AllServers = new List<Server>();

    public List<InfrastructureData> AllInfrastructure;

    public Dictionary<StatType, float> Stats { get; private set; }
    public static event System.Action OnStatsChanged;
    public static event System.Action OnDailyCostChanged;
    public static event System.Action<InfrastructureInstance> OnInfrastructureBuilt;

    // --- Packet Management ---
    public GameObject packetPrefab;
    private List<NetworkPacket> activePackets = new List<NetworkPacket>();
    
    public void CreatePacket(string fileName, int size, Vector3 startPosition, IDataReceiver destination)
    {
        GameObject packetGO = Instantiate(packetPrefab, startPosition, Quaternion.identity);
        packetGO.SetActive(true);
        NetworkPacket packet = packetGO.GetComponent<NetworkPacket>();
        if (packet == null)
        {
            packet = packetGO.AddComponent<NetworkPacket>();
        }
        
        packet.Initialize(fileName, size, startPosition, destination);
        activePackets.Add(packet);
    }

    public void DestroyPacket(NetworkPacket packet)
    {
        activePackets.Remove(packet);
        Destroy(packet.gameObject);
    }
    // -----------------------

    // --- Network Routing ---
    private Dictionary<string, IDataReceiver> receiverRegistry = new Dictionary<string, IDataReceiver>();

    public void RegisterReceiver(string id, IDataReceiver receiver)
    {
        if (receiverRegistry.ContainsKey(id))
        {
            Debug.LogWarning($"A receiver with ID '{id}' is already registered. Overwriting.");
            receiverRegistry[id] = receiver;
        }
        else
        {
            receiverRegistry.Add(id, receiver);
            Debug.Log($"Registered receiver '{id}'.");
        }
    }

    public void UnregisterReceiver(string id)
    {
        if (receiverRegistry.ContainsKey(id))
        {
            receiverRegistry.Remove(id);
            Debug.Log($"Unregistered receiver '{id}'.");
        }
    }

    public IDataReceiver GetReceiver(string id)
    {
        IDataReceiver receiver;
        Debug.Log("GetReceiver:" + id);
        receiverRegistry.TryGetValue(id, out receiver);
        return receiver;
    }
    // ---------------------

    public bool isQuitting = false; // To prevent issues when unregistering during quit

    void OnApplicationQuit()
    {
        isQuitting = true;
    }

    public void NotifyInfrastructureBuilt(InfrastructureInstance instance)
    {
        OnInfrastructureBuilt?.Invoke(instance);
    }


    [SerializeField] private GridManager gridManager;
    [SerializeField] private TileBase floorTile;
    [SerializeField] private GameObject npcDevOpsPrefab;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            Debug.Log("DIAGNOSTIC: GameManager instance is now assigned.");
            InitializeStats();
            OnInfrastructureBuilt += HandleInfrastructureBuilt;
        }
    }

    void OnDestroy()
    {
        OnInfrastructureBuilt -= HandleInfrastructureBuilt;
    }

    private void HandleInfrastructureBuilt(InfrastructureInstance instance)
    {
        // Check if the new building is a server
        if (instance is Server)
        {
            // Check if this is the FIRST operational server
            int operationalServerCount = AllInfrastructure.Count(infra =>
                infra.CurrentState == InfrastructureData.State.Operational && infra.Prefab.GetComponent<Server>() != null);

            if (operationalServerCount == 1)
            {
                Debug.Log("First server is operational! Starting traffic at 1 packet/sec.");
                Stats[StatType.Traffic] = 1; // Set traffic rate
                OnStatsChanged?.Invoke();
            }
        }
    }

    void Start()
    {
        // Create a default packet prefab if one isn't assigned
        if (packetPrefab == null)
        {
            packetPrefab = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            packetPrefab.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
            packetPrefab.GetComponent<Renderer>().material.color = Color.cyan;
            packetPrefab.AddComponent<NetworkPacket>();
            packetPrefab.SetActive(false); 
        }

        if (FindObjectOfType<GameLoopManager>() == null) gameObject.AddComponent<GameLoopManager>();
        if (FindObjectOfType<UIManager>() == null) gameObject.AddComponent<UIManager>();
        if (FindObjectOfType<MouseInteractionManager>() == null) gameObject.AddComponent<MouseInteractionManager>();
        
        AllServers.Clear();
        SetupGameScene();
    }

    private void InitializeStats()
    {
        Stats = new Dictionary<StatType, float>();
        Stats.Add(StatType.Money, 1000f);
        Stats.Add(StatType.TechDebt, 0f);
        Stats.Add(StatType.ResearchPoints, 0f);
        Stats.Add(StatType.Traffic, 0f);
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

    public float GetStat(StatType stat) => Stats.ContainsKey(stat) ? Stats[stat] : 0f;

    public float CalculateTotalDailyCost()
    {
        float totalCost = 0;
        foreach (var infra in AllInfrastructure)
        {
            if (infra.CurrentState == InfrastructureData.State.Operational)
            {
                totalCost += infra.DailyCost;
            }
        }
        
        foreach (var npc in FindObjectsOfType<NPCDevOps>())
        {
            totalCost += npc.Data.DailyCost;
        }
        return totalCost;
    }
    
    public void NotifyDailyCostChanged() => OnDailyCostChanged?.Invoke();

    public void UpdateInfrastructureVisibility()
    {
        foreach (var infraData in AllInfrastructure)
        {
            if (infraData.CurrentState == InfrastructureData.State.Locked && !infraData.Instance.activeSelf)
            {
                if (AreUnlockConditionsMet(infraData))
                {
                    infraData.Instance.SetActive(true);
                    infraData.Instance.GetComponent<InfrastructureInstance>().SetState(InfrastructureData.State.Unlocked);
                    Debug.Log($"Infrastructure '{infraData.DisplayName}' is now UNLOCKED.");
                }
            }
        }
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
                 Debug.Log("GridManager not found, creating one.");
                 gridManager = new GameObject("GridManager").AddComponent<GridManager>();
            }
        }
        
        gridManager.tilePrefab = floorTile as Tile;
        gridManager.CreateGrid();

        if (Camera.main.GetComponent<Physics2DRaycaster>() == null)
        {
            Camera.main.gameObject.AddComponent<Physics2DRaycaster>();
        }
        
        foreach (var infraData in AllInfrastructure)
        {
            Vector3 worldPos = gridManager.gridComponent.CellToWorld(new Vector3Int(infraData.GridPosition.x, infraData.GridPosition.y, 0));
            GameObject instanceGO = Instantiate(infraData.Prefab, worldPos, Quaternion.identity);
            infraData.Instance = instanceGO;

            if (instanceGO.GetComponent<Collider2D>() == null)
            {
                var boxCollider = instanceGO.AddComponent<BoxCollider2D>();
                var spriteRenderer = instanceGO.GetComponentInChildren<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    boxCollider.size = spriteRenderer.bounds.size;
                }
            }

            var infraInstance = instanceGO.GetComponent<InfrastructureInstance>();
            if (infraInstance != null)
            {
                infraInstance.Initialize(infraData);
                if (infraData.IsInitiallyUnlocked)
                {
                    infraInstance.SetState(InfrastructureData.State.Operational);
                    if (instanceGO.GetComponent<Server>() != null) AllServers.Add(instanceGO.GetComponent<Server>());
                }
                else
                {
                    if (AreUnlockConditionsMet(infraData))
                    {
                        infraInstance.SetState(InfrastructureData.State.Unlocked);
                    }
                    else
                    {
                        infraInstance.SetState(InfrastructureData.State.Locked);
                        instanceGO.SetActive(false);
                    }
                }
            }
            else
            {
                Debug.LogError($"Prefab for '{infraData.DisplayName}' is missing the InfrastructureInstance script!");
            }
        }
    }

    public bool AreUnlockConditionsMet(InfrastructureData infraData)
    {
        if (infraData.UnlockConditions == null || infraData.UnlockConditions.Length == 0) return true;

        foreach (var condition in infraData.UnlockConditions)
        {
            switch (condition.Type)
            {
                case UnlockCondition.ConditionType.Day:
                    if (GameLoopManager.Instance.currentDay < condition.RequiredValue) return false;
                    break;
            }
        }
        return true;
    }

    public void PlanInfrastructure(InfrastructureData infraData)
    {
        if (infraData.CurrentState != InfrastructureData.State.Unlocked) return; // MUST BE UNLOCKED TO PLAN

        if (!AreUnlockConditionsMet(infraData))
        {
            Debug.Log("Unlock conditions not met for this infrastructure.");
            return;
        }

        infraData.Instance.GetComponent<InfrastructureInstance>().SetState(InfrastructureData.State.Planned);
        Debug.Log($"Successfully planned {infraData.DisplayName}.");
        UIManager.Instance.HideTooltip();
    }
    
    public List<NPCDevOpsData> GenerateNPCCandidates(int count)
    {
        var candidates = new List<NPCDevOpsData>();
        for (int i = 0; i < count; i++)
        {
            candidates.Add(new NPCDevOpsData { DailyCost = UnityEngine.Random.Range(100, 201) });
        }
        return candidates;
    }

    public void HireNPCDevOps(NPCDevOpsData candidateData)
    {
        int randomX = Random.Range(0, gridManager.gridWidth);
        int randomY = Random.Range(0, gridManager.gridHeight);
        Vector3 worldPos = gridManager.gridComponent.CellToWorld(new Vector3Int(randomX, randomY, 0));
        
        GameObject npcObject = Instantiate(npcDevOpsPrefab, worldPos, Quaternion.identity);
        npcObject.GetComponent<NPCDevOps>().Initialize(candidateData);
        
        NotifyDailyCostChanged();
    }
}