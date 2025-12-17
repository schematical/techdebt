using System;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using UnityEngine.EventSystems;
using Random = UnityEngine.Random;
using Stats;

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
  
    public List<InfrastructureInstance> ActiveInfrastructure = new List<InfrastructureInstance>();
    
    public UIManager UIManager;
    public FloatingTextFactory FloatingTextFactory;
    public GameLoopManager GameLoopManager;
    public float desiredTimeScale = 1f;

    public List<InfrastructureData> AllInfrastructure;
    public List<Technology> AllTechnologies;
    public List<NetworkPacketData> NetworkPacketDatas  = new List<NetworkPacketData>();
    public Stats.StatsCollection Stats { get; private set; } = new StatsCollection();
    public static event System.Action OnStatsChanged;
    public static event System.Action OnDailyCostChanged;
    public static event System.Action<InfrastructureInstance> OnInfrastructureBuilt;
    public static event System.Action<Technology> OnTechnologyUnlocked;
    public static event System.Action<Technology> OnTechnologyResearchStarted;

    public Technology CurrentlyResearchingTechnology { get; private set; }

    // --- Task Management ---
    public List<NPCTask> AvailableTasks = new List<NPCTask>();

    public void AddTask(NPCTask task)
    {
        AvailableTasks.Add(task);
        // Optional: Sort the list when a new task is added
        AvailableTasks = AvailableTasks.OrderByDescending(t => t.Priority).ToList();
    }

    public NPCTask GetHighestPriorityTask()
    {
        return AvailableTasks
            .Where(t => t.CurrentStatus == NPCTask.Status.Pending && !t.IsAssigned)
            .OrderByDescending(t => t.Priority)
            .FirstOrDefault();
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


    public NetworkPacketData GetNetworkPacketData()
    {
        float probTotal = 0f;
        foreach (var npData in NetworkPacketDatas)
        {
            probTotal += npData.probilitly;
        }

        float index = Random.Range(0, probTotal);
        float currFloor = 0;
        NetworkPacketData foundData = null;
        foreach (var npData in NetworkPacketDatas)
        {
            if (
                index >= currFloor &&
                index < currFloor + npData.probilitly
            )
            {
                foundData = npData;
                break;
            }
            currFloor += npData.probilitly;
        }

        if (foundData == null)
        {
            Debug.LogError($"GetNetworkPacketData - Not found - probTotal: {probTotal} - currFloor: {currFloor}");
        }
        return foundData;
    }
    public NetworkPacket CreatePacket(NetworkPacketData data, string fileName, int size, InfrastructureInstance origin)
    {
        
        
        GameObject packetGO = Instantiate(data.prefab, origin.transform.position, Quaternion.identity);
        packetGO.SetActive(true);
        NetworkPacket packet = packetGO.GetComponent<NetworkPacket>();
        packet.Initialize(data, fileName, size, origin);
      
        activePackets.Add(packet);
		IncrStat(StatType.PacketsSent);
        return packet;
    }

    

    public void DestroyPacket(NetworkPacket packet)
    {
        activePackets.Remove(packet);
        Destroy(packet.gameObject);
        float packetsServiced = -1;
		if (packet.CurrentState == NetworkPacket.State.Failed) {
            float packetsFailed = IncrStat(StatType.PacketsFailed);
            return;
        }
		 packetsServiced = IncrStat(StatType.PacketsServiced);

        // --- Calculate Income & Expenses ---
      	float packetIncome = GetStat(StatType.PacketIncome);
        IncrStat(StatType.Money, packetIncome);
        FloatingTextFactory.ShowText($"+${packetIncome}%", packet.transform.position, new Color(0f, 1f, 0f));//  + new Vector3(0, 1, 3));
		int incrAfter = (int) Math.Floor(40 * GetStat(StatType.Traffic));
		if(packetsServiced % incrAfter == 0) {
        	float traffic = GetStat(StatType.Traffic);
			float difficulty = GetStat(StatType.Difficulty);
        	SetStat(StatType.Traffic, traffic * difficulty);
            // SetStat(StatType.PRR, GetStat(StatType.PRR) * difficulty);
		}
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
		foreach(var activeInfra in ActiveInfrastructure) {
			activeInfra.OnInfrastructureBuilt(instance);
		}
    }


    [SerializeField] private GridManager gridManager;
    [SerializeField] private TileBase floorTile;
    [SerializeField] private GameObject npcDevOpsPrefab;

    void Awake()
    {
        _instance = this;

        // Force reset of all infrastructure data to its initial state on load.
        // This is the definitive fix for ensuring a clean state after a game over.
    

        // Force reset of all technology data to its initial state on load.
        foreach (var tech in AllTechnologies)
        {
            tech.CurrentState = Technology.State.Locked;
        }

        InitializeStats();
        OnInfrastructureBuilt += HandleInfrastructureBuilt;
        OnTechnologyUnlocked += HandleTechnologyUnlocked;
    
        ActiveInfrastructure.Clear();
        SetupGameScene();
    }

    void OnDestroy()
    {
        OnInfrastructureBuilt -= HandleInfrastructureBuilt;
        OnTechnologyUnlocked -= HandleTechnologyUnlocked;
        if (_instance == this)
        {
            _instance = null;
        }
    }

    private void HandleTechnologyUnlocked(Technology tech)
    {
        Debug.Log($"Technology '{tech.DisplayName}' unlocked. Checking for newly available infrastructure...");
        UpdateInfrastructureVisibility();
    }

    private void HandleInfrastructureBuilt(InfrastructureInstance instance)
    {
        // Check if the new building is a server
        /* if (instance is Server)
        {
            // Check if this is the FIRST operational server
            int operationalServerCount = AllInfrastructure.Count(infra =>
                infra.CurrentState == InfrastructureData.State.Operational && infra.Prefab.GetComponent<Server>() != null);

            if (operationalServerCount == 1)
            {
                Stats[StatType.Traffic] = 1; // Set traffic rate
                OnStatsChanged?.Invoke();
            }
        } */
    }

    void Start()
    {
        /*GameLoopManager = GetComponent<GameLoopManager>();
        if (GameLoopManager == null)
        {
            throw new SystemException("Missing `GameLoopManager` reference in GameManager.");
        }*/
        Time.timeScale = 0;
        HireNPCDevOps(new NPCDevOpsData { DailyCost = 100 });
        // --- Technology Debugging ---
        Debug.Log($"GameManager Start: Found {AllTechnologies.Count} technologies.");
        UIManager.SetupUIInfrastructure();
        GameLoopManager.BeginBuildPhase();
    }

    private void InitializeStats()
    {
       
        
        Stats.Add(new StatData(StatType.Money, 200f));
        Stats.Add(new StatData(StatType.TechDebt, 0f));
        Stats.Add(new StatData(StatType.Traffic, 0.25f));
        Stats.Add(new StatData(StatType.PacketsSent, 0f));
        Stats.Add(new StatData(StatType.PacketsServiced, 0f));
        Stats.Add(new StatData(StatType.PacketsFailed, 0f));
        Stats.Add(new StatData(StatType.PacketIncome, 10f));
        Stats.Add(new StatData(StatType.Difficulty, 1.5f));
        Stats.Add(new StatData(StatType.PRR, 0.5f));
    }
    
	public float IncrStat(StatType stat, float value = 1)
    {
       
        return Stats.Stats[stat].IncrStat(value);
    }
  
    public void SetStat(StatType stat, float value)
    {
        Stats.Stats[stat].SetBaseValue(value);
        Stats.Stats[stat].UpdateValue();
    }


    public float GetStat(StatType stat) => Stats.Stats[stat].Value;

    public void SetDesiredTimeScale(float scale)
    {
        desiredTimeScale = scale;
        Time.timeScale = scale;
    }

    public float CalculateTotalDailyCost()
    {
        float totalCost = 0;
        foreach (var infra in AllInfrastructure)
        {
            if (infra.CurrentState == InfrastructureData.State.Operational)
            {
                totalCost += infra.Stats.GetStatValue(StatType.Infra_DailyCost);
            }
        }
        
        foreach (var npc in FindObjectsOfType<NPCDevOps>())
        {
            totalCost += npc.Data.Stats.GetStatValue(StatType.NPC_DailyCost);
        }
        return totalCost;
    }
    
    public void NotifyDailyCostChanged() => OnDailyCostChanged?.Invoke();

    public void UpdateInfrastructureVisibility()
    {
        foreach (var instance in ActiveInfrastructure)
        {   
            InfrastructureData infraData = instance.data;
            if (infraData.CurrentState == InfrastructureData.State.Locked && !instance.gameObject.activeSelf)
            {
                if (AreUnlockConditionsMet(infraData))
                {
                    instance.gameObject.SetActive(true);
                    instance.GetComponent<InfrastructureInstance>().SetState(InfrastructureData.State.Unlocked);
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
            Debug.Log($"1111 Initialized infrastructure {infraData.ID} {infraData.CurrentState}");
            Vector3 worldPos = gridManager.gridComponent.CellToWorld(new Vector3Int(infraData.GridPosition.x, infraData.GridPosition.y, 0));
            GameObject instanceGO = Instantiate(infraData.Prefab, worldPos, Quaternion.identity);

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
                ActiveInfrastructure.Add(infraInstance);
                infraInstance.Initialize(infraData);
                if (infraData.CurrentState == InfrastructureData.State.Operational)
                {
                 
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
                case(UnlockCondition.ConditionType.Technology):
                    Technology technology = GetTechnologyByID(condition.TechnologyID);
                    if (technology == null)
                    {
                        throw new SystemException(
                            $"Cannot find Technology {condition.TechnologyID} - {infraData.ID}");
                    } 
                    
                    if(technology.CurrentState != Technology.State.Unlocked) return false;
                    
                    break;
                default:
                    if (GetStat(condition.StatType) < condition.RequiredValue) return false;
                    break;
            }
        }
        return true;
    }

    public void PlanInfrastructure(InfrastructureInstance infra)
    {
        if (infra.data.CurrentState != InfrastructureData.State.Unlocked) return; // MUST BE UNLOCKED TO PLAN

        if (!AreUnlockConditionsMet(infra.data))
        {
            Debug.Log("Unlock conditions not met for this infrastructure.");
            return;
        }

        infra.GetComponent<InfrastructureInstance>().SetState(InfrastructureData.State.Planned);
        Debug.Log($"Successfully planned {infra.data.DisplayName}.");
        UIManager.HideInfrastructureDetail();
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

    public void SelectTechnologyForResearch(Technology tech)
    {
        if (tech == null || tech.CurrentState != Technology.State.Locked)
        {
            Debug.LogWarning($"Technology '{tech?.DisplayName}' cannot be researched because its state is not 'Locked'.");
            return;
        }

        if (CurrentlyResearchingTechnology != null)
        {
            Debug.Log($"Cannot start research on '{tech.DisplayName}'. Another technology ('{CurrentlyResearchingTechnology.DisplayName}') is already being researched.");
            return;
        }

        foreach (var requiredTechID in tech.RequiredTechnologies)
        {
            Technology requiredTech = GetTechnologyByID(requiredTechID);
            if (requiredTech == null || requiredTech.CurrentState != Technology.State.Unlocked)
            {
                Debug.Log($"Cannot research '{tech.DisplayName}'. Prerequisite '{requiredTech?.DisplayName ?? requiredTechID}' is not unlocked.");
                return;
            }
        }

        CurrentlyResearchingTechnology = tech;
        tech.CurrentState = Technology.State.Researching;
        Debug.Log($"'{tech.DisplayName}' is now being researched.");
        OnTechnologyResearchStarted?.Invoke(tech);

        // Remove any existing research tasks
        AvailableTasks.RemoveAll(task => task is ResearchTask);
        
        // Add a new research task for the selected technology
        AddTask(new ResearchTask(tech));
    }

    public void ApplyResearchProgress(float researchGained)
    {
        if (CurrentlyResearchingTechnology == null) return;

        CurrentlyResearchingTechnology.CurrentResearchProgress += researchGained;

        if (CurrentlyResearchingTechnology.CurrentResearchProgress >= CurrentlyResearchingTechnology.ResearchPointCost)
        {
            Debug.Log($"Technology '{CurrentlyResearchingTechnology.DisplayName}' unlocked!");
            CurrentlyResearchingTechnology.CurrentState = Technology.State.Unlocked;
            OnTechnologyUnlocked?.Invoke(CurrentlyResearchingTechnology);
            
            CurrentlyResearchingTechnology = null;
        }
    }

    // Helper method to get a Technology by its ID
    public Technology GetTechnologyByID(string id)
    {
        return AllTechnologies.FirstOrDefault(t => t.TechnologyID == id);
    }
    public InfrastructureInstance GetInfrastructureInstanceByID(string id)
    {
        return ActiveInfrastructure.FirstOrDefault(t => t.data.ID == id);
    }
}