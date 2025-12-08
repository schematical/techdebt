// GameManager.cs

using System;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.EventSystems;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    public static GameManager Instance
    {
        get
        {
            if (_instance == null) _instance = FindObjectOfType<GameManager>();
            return _instance;
        }
    }
  
    public static List<Server> AllServers = new List<Server>();
    
    public UIManager UIManager;

    public GameLoopManager GameLoopManager;

    public List<InfrastructureData> AllInfrastructure;
    public List<Technology> AllTechnologies;

    public Dictionary<StatType, float> Stats { get; private set; }
    public static event System.Action OnStatsChanged;
    public static event System.Action OnDailyCostChanged;
    public static event System.Action<InfrastructureInstance> OnInfrastructureBuilt;
    public static event System.Action<Technology> OnTechnologyUnlocked;

    // --- Task Management ---
    public List<NPCTask> AvailableTasks = new List<NPCTask>();

    public void AddTask(NPCTask task)
    {
        AvailableTasks.Add(task);
        // Optional: Sort the list when a new task is added
        AvailableTasks = AvailableTasks.OrderByDescending(t => t.Priority).ToList();
    }

    public NPCTask RequestTask(NPCDevOps npc)
    {
        if (npc == null) return null;
        
        // Find a task that is not already completed or assigned
        NPCTask availableTask = AvailableTasks
            .Where(t => t.CurrentStatus == NPCTask.Status.Pending && !t.IsAssigned)
            .OrderByDescending(t => t.Priority)
            .FirstOrDefault();

        if (availableTask != null)
        {
            if (availableTask.TryAssign(npc))
            {
                return availableTask;
            }
        }
        return null;
    }

    public void CompleteTask(NPCTask task)
    {
        if (AvailableTasks.Contains(task))
        {
            AvailableTasks.Remove(task);
        }
    }
    // -----------------------

    // --- Packet Management ---
    public GameObject packetPrefab;
    private List<NetworkPacket> activePackets = new List<NetworkPacket>();
    public int SuccessfulPacketRoundTrips { get; private set; } = 0;

    public void NotifyPacketRoundTripComplete()
    {
        SuccessfulPacketRoundTrips++;
    }

    public int GetAndResetPacketRoundTripCount()
    {
        int count = SuccessfulPacketRoundTrips;
        SuccessfulPacketRoundTrips = 0;
        return count;
    }
    
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
        }
    }

    public void UnregisterReceiver(string id)
    {
        if (receiverRegistry.ContainsKey(id))
        {
            receiverRegistry.Remove(id);
        }
    }

    public IDataReceiver GetReceiver(string id)
    {
        IDataReceiver receiver;
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
        _instance = this;

        // Force reset of all infrastructure data to its initial state on load.
        // This is the definitive fix for ensuring a clean state after a game over.
        foreach (var infraData in AllInfrastructure)
        {
            infraData.CurrentState = infraData.IsInitiallyUnlocked ? InfrastructureData.State.Operational : InfrastructureData.State.Locked;
        }

        // Force reset of all technology data to its initial state on load.
        foreach (var tech in AllTechnologies)
        {
            tech.CurrentState = Technology.State.Locked;
        }

        InitializeStats();
        OnInfrastructureBuilt += HandleInfrastructureBuilt;
    
        AllServers.Clear();
        SetupGameScene();
    }

    void OnDestroy()
    {
        OnInfrastructureBuilt -= HandleInfrastructureBuilt;
        if (_instance == this)
        {
            _instance = null;
        }
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
                Stats[StatType.Traffic] = 1; // Set traffic rate
                OnStatsChanged?.Invoke();
            }
        }
    }

    void Start()
    {
        /*GameLoopManager = GetComponent<GameLoopManager>();
        if (GameLoopManager == null)
        {
            throw new SystemException("Missing `GameLoopManager` reference in GameManager.");
        }*/
        // --- Technology Debugging ---
        Debug.Log($"GameManager Start: Found {AllTechnologies.Count} technologies.");
        UIManager.SetupUIInfrastructure();
        GameLoopManager.BeginBuildPhase();
        // Create a default packet prefab if one isn't assigned
        if (packetPrefab == null)
        {
            packetPrefab = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            packetPrefab.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
            packetPrefab.GetComponent<Renderer>().material.color = Color.cyan;
            packetPrefab.AddComponent<NetworkPacket>();
            packetPrefab.SetActive(false); 
        }
    }

    private void InitializeStats()
    {
        Stats = new Dictionary<StatType, float>();
        Stats.Add(StatType.Money, 50f);
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

    public void TrySpendStat(StatType stat, float value)
    {
        if (!Stats.ContainsKey(stat))
        {
            throw new System.Exception($"Can't find stat");
        } 
    
        Stats[stat] -= value;
        OnStatsChanged?.Invoke();
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
                    if (GameManager.Instance.GameLoopManager.currentDay < condition.RequiredValue) return false;
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
        UIManager.HideTooltip();
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

    public void ResetNPCs()
    {
        foreach (var npc in FindObjectsOfType<NPCDevOps>())
        {
            Destroy(npc.gameObject);
        }
        Debug.Log("All NPCDevOps instances have been destroyed.");
    }

    public bool TryUnlockTechnology(Technology tech)
    {
        if (tech == null || tech.CurrentState == Technology.State.Unlocked)
        {
            return false;
        }

        // Check if all required technologies are unlocked (by ID)
        foreach (var requiredTechID in tech.RequiredTechnologies)
        {
            Technology requiredTech = GetTechnologyByID(requiredTechID);
            if (requiredTech == null)
            {
                Debug.LogWarning($"Required technology with ID '{requiredTechID}' not found for {tech.DisplayName}.");
                return false; // Prerequisite not found
            }
            if (requiredTech.CurrentState != Technology.State.Unlocked)
            {
                Debug.Log($"Cannot unlock {tech.DisplayName}. Prerequisite {requiredTech.DisplayName} is not unlocked.");
                return false; // Prerequisite not met
            }
        }

        // Check for sufficient research points
        if (GetStat(StatType.ResearchPoints) >= tech.ResearchPointCost)
        {
            TrySpendStat(StatType.ResearchPoints, tech.ResearchPointCost);
            tech.CurrentState = Technology.State.Unlocked;
            OnTechnologyUnlocked?.Invoke(tech);
            Debug.Log($"Technology '{tech.DisplayName}' unlocked!");
            return true;
        }
        else
        {
            Debug.Log($"Not enough Research Points to unlock {tech.DisplayName}. Current RP: {GetStat(StatType.ResearchPoints)}, Cost: {tech.ResearchPointCost}");
            return false;
        }
    }

    // Helper method to get a Technology by its ID
    public Technology GetTechnologyByID(string id)
    {
        return AllTechnologies.FirstOrDefault(t => t.TechnologyID == id);
    }
}