using System;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DefaultNamespace;
using Effects;
using Events;
using Infrastructure;
using Items;
using JetBrains.Annotations;
using MetaChallenges;
using NPCs;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;
using Stats;
using UI;
using UnityEngine.Serialization;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    
    public enum GlobalNetworkPacketState { Running, Frozen}
    public static GameManager Instance
    {
        get
        {
            return _instance;
        }
    }

    public List<InfrastructureInstance> ActiveInfrastructure = new List<InfrastructureInstance>();
    public List<ItemData> Items = new List<ItemData>();
    public UIManager UIManager;
    public FloatingTextFactory FloatingTextFactory;
    public GameLoopManager GameLoopManager;
    public PrefabManager prefabManager;
    public CameraController cameraController;
    public SpriteManager SpriteManager;
    public List<NetworkPacket> activePackets = new List<NetworkPacket>();
    public List<InfrastructureData> AllInfrastructure;
    public List<Technology> AllTechnologies;
    public Dictionary<WorldObjectType.Type, WorldObjectType> WorldObjectTypes = new Dictionary<WorldObjectType.Type, WorldObjectType>();
   
    [SerializeField] public GridManager gridManager;
    protected List<NetworkPacketData> NetworkPacketDatas  = new List<NetworkPacketData>(){
        new NetworkPacketData() {
            Type = NetworkPacketData.PType.Text,
            baseLoad = 20,
            probilitly = 10,
            prefabId = "FileCoin"
        },
        new NetworkPacketData() {
           Type = NetworkPacketData.PType.Text,
           baseLoad = 10,
           probilitly = 45,
           prefabId = "NetworkPacket"
       },
       new NetworkPacketData() {
           Type = NetworkPacketData.PType.Image,
           baseLoad = 20,
           probilitly = 45,
           prefabId = "FileCat"
       },
       new NetworkPacketData() {
           Type = NetworkPacketData.PType.MaliciousText,
           baseLoad = 1000,
           probilitly = 0,
           prefabId = "NetworkPacketAttack"
       },
       new NetworkPacketData() {
           Type = NetworkPacketData.PType.BatchJob,
           baseLoad = 0,
           probilitly = 0,
           prefabId = "BatchJobNetworkPacket"
       },
       new NetworkPacketData() {
           Type = NetworkPacketData.PType.PII,
           baseLoad = 0,
           probilitly = 10,
           prefabId = "PIINetworkPacket"
       },
        
    };
    public StatsCollection Stats { get; private set; } = new StatsCollection();
    public MetaStatCollection MetaStats { get; private set; } = new MetaStatCollection();
    
    public Stats.StatsCollection GlobalStats { get; private set; } = new StatsCollection();
    
    public List<EventBase> Events { get; private set; } = new List<EventBase>();
    public List<EffectBase> Effects { get; private set; } = new List<EffectBase>();
    
    public List<EventBase> CurrentEvents { get; private set; } = new List<EventBase>();
    public static event System.Action OnStatsChanged;
    public static event System.Action OnDailyCostChanged;
    public static event System.Action<InfrastructureInstance, InfrastructureData.State?> OnInfrastructureStateChange;
    public static event System.Action<Technology> OnTechnologyUnlocked;
    public static event System.Action<Technology> OnTechnologyResearchStarted;
    public static event System.Action OnCurrentEventsChanged;
    
    public static event System.Action<ReleaseBase, ReleaseBase.ReleaseState> OnReleaseChanged;
    public static event System.Action<GameLoopManager.GameState> OnPhaseChange;
    
    public GlobalNetworkPacketState NetworkPacketState = GlobalNetworkPacketState.Running;
    
    public TutorialEvent Tutorial;

    public Technology CurrentlyResearchingTechnology { get; private set; }
    public ModifierCollection Modifiers { get; set; } = new  ModifierCollection();


    // --- Item Spawning ---
    private float _itemDropTimer;


    // --- Task Management ---
    public List<NPCTask> AvailableTasks = new List<NPCTask>();
    public List<NPCBase> AllNpcs = new List<NPCBase>();
    public List<ReleaseBase> Releases = new List<ReleaseBase>();
    private float timeSinceLastPacket = 0f;
    public void AddTask(NPCTask task)
    {
        AvailableTasks.Add(task);
        task.OnQueued();
        // Optional: Sort the list when a new task is added
        AvailableTasks = AvailableTasks.OrderByDescending(t => t.Priority).ToList();
    }

    public NPCTask GetHighestPriorityTask()
    {
        return AvailableTasks
            .Where(t => t.CurrentState == NPCTask.State.Queued && !t.IsAssigned)
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

    public void IncreaseTaskPriority(NPCTask task)
    {
        // The list is sorted descending, so "increasing" means moving to a lower index.
        int index = AvailableTasks.IndexOf(task);
        if (index > 0)
        {
            NPCTask otherTask = AvailableTasks[index - 1];
            if (task.Priority < otherTask.Priority)
            {
                // Standard case: swap priorities
                (otherTask.Priority, task.Priority) = (task.Priority, otherTask.Priority);
            }
            else
            {
                // Equal priority case: increment to ensure it moves up
                task.Priority = otherTask.Priority + 1;
            }
            AvailableTasks = AvailableTasks.OrderByDescending(t => t.Priority).ToList();
        }
    }

    public void DecreaseTaskPriority(NPCTask task)
    {
        // The list is sorted descending, so "decreasing" means moving to a higher index.
        int index = AvailableTasks.IndexOf(task);
        if (index < AvailableTasks.Count - 1 && index != -1)
        {
            NPCTask otherTask = AvailableTasks[index + 1];
            if (task.Priority > otherTask.Priority)
            {
                // Standard case: swap priorities
                (otherTask.Priority, task.Priority) = (task.Priority, otherTask.Priority);
            }
            else
            {
                // Equal priority case: decrement to ensure it moves down
                task.Priority = otherTask.Priority - 1;
            }
            AvailableTasks = AvailableTasks.OrderByDescending(t => t.Priority).ToList();
        }
    }

    public void CreateUseItemTask(ItemBase item)
    {
        var useTask = new UseItemTask(item);
        AddTask(useTask);
    }


    public NetworkPacketData GetNetworkPacketData()
    {
       
       
        float probTotal = 0f;
        foreach (NetworkPacketData npData in NetworkPacketDatas)
        {
            // Debug.Log($"{npData.Type} - Prob: {npData.probilitly} Total Before: {probTotal}");
            probTotal += npData.probilitly;
        }
        float index = Random.Range(0, probTotal);
        float currFloor = 0;
        NetworkPacketData foundData = null;
        foreach (NetworkPacketData npData in NetworkPacketDatas)
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
        NetworkPacket packet = prefabManager.Create(data.prefabId, origin.GetInteractionPosition()).GetComponent<NetworkPacket>();
        packet.Initialize(data, fileName, size, origin);
        activePackets.Add(packet);
        IncrStat(StatType.PacketsSent);
        return packet;
    }

    public void DestroyPacket(NetworkPacket packet)
    {
        activePackets.Remove(packet);
        packet.gameObject.SetActive(false); // Deactivate instead of destroying

        float packetsServiced = -1;
		switch (packet.CurrentState) {
            case(NetworkPacket.State.Failed):
                IncrStat(StatType.PacketsFailed);
                Sprite sprite = packet.GetComponent<SpriteRenderer>().sprite;
                UIManager.ShowPacketFail(sprite);
            break;
            case(NetworkPacket.State.Stolen):
                IncrStat(StatType.PacketsFailed);
                int cost = 10;
                IncrStat(StatType.Money, cost * -1);
                UIManager.moneyPanel.ExplodeCoins(10);
            break;
            case(NetworkPacket.State.Running):
            	 packetsServiced = IncrStat(StatType.PacketsServiced);
                break;
            default:
                throw new NotImplementedException();
        }
        packet.Reset();
         
    }

    public void CheckEvents()
    {
        if (CurrentEvents.Count() > 0)
        {
            return;
        }
        int totalProb = 0;
        List<EventBase> possibleEvents = new List<EventBase>();
        foreach (var e in Events)
        {
            if (e.IsPossible())
            {
                totalProb += e.GetProbability();
                possibleEvents.Add(e);
            }
        }
        int selectedIndex =  Random.Range(0, totalProb + 1);
        int currIndex = 0;
        foreach (var e in possibleEvents)
        {
   
            if (
                selectedIndex >= currIndex && 
                selectedIndex < currIndex + e.GetProbability()
            )
            {
                TriggerEvent(e);
                break;
            }

            currIndex += e.GetProbability();
        }
    }

    public void TriggerEvent(EventBase e)
    {
        e.Apply();
 
        CurrentEvents.Add(e);
        OnCurrentEventsChanged?.Invoke();
    }

    /*void Update()
    {
        if (Keyboard.current.kKey.wasPressedThisFrame && CurrentEvents.Count > 0)
        {
            EndEvent(CurrentEvents[0]);
        }
    }*/

    public void EndEvent(EventBase e)
    {
        Tutorial = null;
        CurrentEvents.Remove(e);
        if (!string.IsNullOrEmpty(e.EventEndText))
        {
            UIManager.ShowAlert(e.EventEndText);
        }
        OnCurrentEventsChanged?.Invoke();
    }



 

    public void NotifyInfrastructureStateChange(InfrastructureInstance instance, InfrastructureData.State previousState)
    {
        OnInfrastructureStateChange?.Invoke(instance, previousState);
        
		foreach(var activeInfra in ActiveInfrastructure) {
			activeInfra.OnInfrastructureStateChange(instance, previousState);
		}
    }



    void Awake()
    {

        _instance = this;
        
        

        Initialize();
          
          
        List<MetaChallengeBase> unlockedChallenges = MetaGameManager.GetUnlockedChallenges();
        AllTechnologies = MetaGameManager.GetAllTechnologies();
        
       
        foreach (MetaChallengeBase challenge in unlockedChallenges)
        {
            switch (challenge.RewardType)
            {
                case(MetaChallengeBase.MetaChallengeRewardType.Technology):
                    Technology technology = AllTechnologies.Find((t => t.TechnologyID == challenge.RewardId));
                    if (technology == null)
                    {
                        throw new SystemException($"Technology '{challenge.RewardId}' is null.");
                    }

                    if (technology.CurrentState == Technology.State.MetaLocked)
                    {
                        technology.CurrentState = Technology.State.Locked;
                    }

                    break;
                case(MetaChallengeBase.MetaChallengeRewardType.StartingStatValue):
                    StatType statType;
                    Enum.TryParse<StatType>(challenge.RewardId, out statType);
                    
                    Stats.AddModifier(statType, new StatModifier(
                        $"metaChallenge_{challenge.RewardId}",
                        challenge.RewardValue
                    ));
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

       
      
        OnInfrastructureStateChange += HandleInfrastructureStateChange;
        OnTechnologyUnlocked += HandleTechnologyUnlocked;
    
        ActiveInfrastructure.Clear();
        SetupGameScene();
    }

    void OnDestroy()
    {
        OnInfrastructureStateChange -= HandleInfrastructureStateChange;
        OnTechnologyUnlocked -= HandleTechnologyUnlocked;
        if (_instance == this)
        {
            _instance = null;
        }
    }

    private void HandleTechnologyUnlocked(Technology tech)
    {
        UpdateInfrastructureVisibility();
        bool hasMoreToResearch = false;
        foreach (Technology technology in AllTechnologies)
        {
            if (technology.CurrentState == Technology.State.Locked)
            {
                hasMoreToResearch = true;
            }
        }

        if (hasMoreToResearch)
        {
            GetInfrastructureInstanceByID("desk").ShowAttentionIcon();
        }
    }

    private void HandleInfrastructureStateChange(InfrastructureInstance instance, InfrastructureData.State? previousState)
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
        HireNPCDevOps(new NPCDevOpsData { DailyCost = 100 });
        // --- Technology Debugging ---
        UIManager.SetupUIInfrastructure();
        GameLoopManager.BeginPlanPhase();
        
        // --- Item Spawning ---
        _itemDropTimer = GetStat(StatType.ItemDropCheck);
    }

    void Update()
    {

        if (GameLoopManager.CurrentState != GameLoopManager.GameState.Play) return;

        // Delivery NPC Spawning Logic
        _itemDropTimer -= Time.deltaTime;
        if (_itemDropTimer <= 0)
        {
            float dropChance = GetStat(StatType.ItemDropChance);
            if (Random.value <= dropChance)
            {
                SpawnDeliveryNPC();
            }
            _itemDropTimer = GetStat(StatType.ItemDropCheck); // Reset timer
        }
    }

    private void FixedUpdate()
    {
        if (Tutorial != null && CurrentEvents.Count == 0)
        {
            TriggerEvent(Tutorial);
            
        }
        // Iterate over a copy of the list to prevent modification during enumeration errors.
        foreach (var effect in Effects.ToList())
        {
            effect.FixedUpdate();
        }

        TickNetworkPackets();
    }

    private void TickNetworkPackets()
    {
       // Ensure packet generation only runs during the Play phase and the pipe is operational
       if (
           GameLoopManager.CurrentState != GameLoopManager.GameState.Play
       )
       {
           return;
       }

        float secondsBetweenPackets = GameLoopManager.GetDayDurationSeconds() / GetStat(StatType.Traffic);
        timeSinceLastPacket += Time.deltaTime;
    

        if (timeSinceLastPacket >= secondsBetweenPackets)
        {
            
            NetworkPacketData data = GetNetworkPacketData();
            List<InternetPipe> instances = GetInfrastructureInstanceByClass<InternetPipe>();
            if (instances.Count == 0)
            {
                throw new SystemException("Cannot find any `InternetPipe` instances");
            }
            InternetPipe pipe = instances[Random.Range(0, instances.Count)];
            timeSinceLastPacket = 0f;
            pipe.SendPacket(data);
         
        }
        
    }

    private void SpawnDeliveryNPC()
    {
        var door = GetInfrastructureInstanceByID("door");
        if (door == null)
        {
            Debug.LogError("Cannot spawn DeliveryNPC because 'door' infrastructure was not found.");
            return;
        }

        GameObject npcGO = prefabManager.Create("DeliveryNPC", door.transform.position);
        if (npcGO == null)
        {
            Debug.LogError("Failed to create 'DeliveryNPC' from PrefabManager. Is the prefab configured?");
            return;
        }

        var deliveryNpc = npcGO.GetComponent<DeliveryNPC>();
        if (deliveryNpc != null)
        {
            deliveryNpc.Initialize(door.transform.position);
            var deliveryTask = new DeliverItemTask();
            deliveryNpc.AssignTask(deliveryTask);
        }
        else
        {
            Debug.LogError("'DeliveryNPC' prefab is missing the DeliveryNPC component.");
        }
    }

    public NPCBug SpawnNPCBug()
    {
        var door = GetInfrastructureInstanceByID("server1");
        if (door == null)
        {
            throw new SystemException("Cannot spawn NPCBug because 'server' infrastructure was not found.");
        }

        GameObject npcGO = prefabManager.Create("NPCBugMinor", door.transform.position);
        if (npcGO == null)
        {
            throw new SystemException("Failed to create 'NPCBugMinor' from PrefabManager. Is the prefab configured?");
        }

        NPCBug npcBug = npcGO.GetComponent<NPCBug>();
        npcBug.Initialize();
        return npcBug;
    }

    public bool HasOpenBugs()
    {
        foreach (ReleaseBase releaseBase in Releases)
        {
            if (releaseBase.HasOpenBugs())
            {
                return true;
            }
        }

        return false;
    }

    private void Initialize()
    {
        OnReleaseChanged += HandleReleaseChanged;
        Stats.Add(new StatData(StatType.Money, 100f));
        Stats.Add(new StatData(StatType.TechDebt, 0f));
        Stats.Add(new StatData(StatType.Traffic, 30));
        Stats.Add(new StatData(StatType.PacketsSent, 0f));
        Stats.Add(new StatData(StatType.PacketsServiced, 0f));
        Stats.Add(new StatData(StatType.PacketsFailed, 0f));
        Stats.Add(new StatData(StatType.DailyIncome, 40f));
        Stats.Add(new StatData(StatType.Difficulty, 1.25f));
        Stats.Add(new StatData(StatType.PRR, 0.5f));
        Stats.Add(new StatData(StatType.ItemDropChance, 0.1f));
        Stats.Add(new StatData(StatType.ItemDropCheck, 15));
        Stats.Add(new StatData(StatType.Infra_InputValidation, 0.1f));
        Stats.Add(new StatData(StatType.AttackPossibility, 0f));

        Tutorial = new TutorialEvent();
        
        Events.Add(new NothingEvent());
        Events.Add(new SlowSalesWeekEvent());
        // Events.Add(new DeploymentEvent());
        Events.Add(new AttackStartEvent());
        Events.Add(new DDoSEvent());
        Events.Add(new LeakedSecretEvent());
        Events.Add(new LeakedUserCredsEvent());
        Events.Add(new LeakedUserCredsEvent());
        
        Items.Add(new ItemData() { Id = "NukeItem", Probability = 1});
        Items.Add(new ItemData() { Id = "FreezeTimeItem", Probability = 1});
        Items.Add(new ItemData() { Id = "EnergyDrinkItem", Probability = 1});
        WorldObjectTypes[WorldObjectType.Type.Misc] = new WorldObjectType()
        {
            DisplayName = "Misc",
        };
        WorldObjectTypes[WorldObjectType.Type.InternetPipe] = new WorldObjectType()
        {
            DisplayName = "Internet Pipe",
            PrefabId = "InternetPipe",
            NetworkConnections = new List<NetworkConnection>()
            {
                new NetworkConnection()
                {
                    worldObjectType = WorldObjectType.Type.ApplicationServer,
                    networkPacketType = NetworkPacketData.PType.Text
                },
                new NetworkConnection()
                {
                    worldObjectType = WorldObjectType.Type.ApplicationServer,
                    networkPacketType = NetworkPacketData.PType.Image
                },
                new NetworkConnection()
                {
                    worldObjectType = WorldObjectType.Type.ApplicationServer,
                    networkPacketType = NetworkPacketData.PType.PII
                },
                new NetworkConnection()
                {
                    worldObjectType = WorldObjectType.Type.ApplicationServer,
                    networkPacketType = NetworkPacketData.PType.MaliciousText
                },
                
                
                new NetworkConnection()
                {
                    worldObjectType = WorldObjectType.Type.ALB,
                    networkPacketType = NetworkPacketData.PType.Text,
                    priority = 6
                },
                new NetworkConnection()
                {
                    worldObjectType = WorldObjectType.Type.ApplicationServer,
                    networkPacketType = NetworkPacketData.PType.PII,
                    priority = 6
                },
                new NetworkConnection()
                {
                    worldObjectType = WorldObjectType.Type.ApplicationServer,
                    networkPacketType = NetworkPacketData.PType.MaliciousText,
                    priority = 6
                },
                
                new NetworkConnection()
                {
                    worldObjectType = WorldObjectType.Type.ApplicationServer,
                    networkPacketType = NetworkPacketData.PType.Image,
                    priority = 7
                }
                
            }
        };
        
        
        WorldObjectTypes[WorldObjectType.Type.ApplicationServer] = new WorldObjectType()
        {
            DisplayName = "Application Server",
            PrefabId = "ServerPrefab",
            BuildTime = 30,
            DailyCost = 30,
            CanBeUpsized = true,
            LoadPerPacket = 25,
            NetworkConnections = new List<NetworkConnection>()
            {
                new NetworkConnection()
                {
                    worldObjectType = WorldObjectType.Type.DedicadedDB,
                    networkPacketType = NetworkPacketData.PType.Text
                },
                new NetworkConnection()
                {
                    worldObjectType = WorldObjectType.Type.DedicadedDB,
                    networkPacketType = NetworkPacketData.PType.PII
                },
                new NetworkConnection()
                {
                    worldObjectType = WorldObjectType.Type.DedicadedDB,
                    networkPacketType = NetworkPacketData.PType.MaliciousText
                },
                new NetworkConnection()
                {
                    worldObjectType = WorldObjectType.Type.Redis,
                    networkPacketType = NetworkPacketData.PType.Text
                },
                new NetworkConnection()
                {
                    worldObjectType = WorldObjectType.Type.Queue,
                    networkPacketType = NetworkPacketData.PType.Text
                },

                
            }
        };
        
        WorldObjectTypes[WorldObjectType.Type.BinaryStorage] = new WorldObjectType()
        {
            DisplayName = "Binary Storage",
            PrefabId = "S3Bucket",
            BuildTime = 30,
            DailyCost = 30,
            CanBeUpsized = true,
            LoadPerPacket = 25,
            
            UnlockConditions = new List<UnlockCondition>()
            {
                new UnlockCondition()
                {
                    Type = UnlockCondition.ConditionType.Technology,
                    TechnologyID = "binary-storage"
                }
            }
            
        };
        WorldObjectTypes[WorldObjectType.Type.CDN] = new WorldObjectType()
        {
            DisplayName = "CDN",
            PrefabId = "CloudFront",
            BuildTime = 30,
            DailyCost = 30,
            CanBeUpsized = true,
            LoadPerPacket = 1,
            UnlockConditions = new List<UnlockCondition>()
            {
                new UnlockCondition()
                {
                    Type = UnlockCondition.ConditionType.Technology,
                    TechnologyID = "cdn"
                }
            }
            
        };
        WorldObjectTypes[WorldObjectType.Type.DedicadedDB] = new WorldObjectType()
        {
            DisplayName = "Database",
            PrefabId = "DedicatedDB",
            BuildTime = 30,
            DailyCost = 30,
            CanBeUpsized = true,
            LoadPerPacket = 25,
            UnlockConditions = new List<UnlockCondition>()
            {
                new UnlockCondition()
                {
                    Type = UnlockCondition.ConditionType.Technology,
                    TechnologyID = "dedicated-db"
                }
            }
            
        };
        WorldObjectTypes[WorldObjectType.Type.Redis] = new WorldObjectType()
        {
            DisplayName = "Key Value Store",
            PrefabId = "Redis",
            BuildTime = 30,
            DailyCost = 30,
            CanBeUpsized = true,
            LoadPerPacket = 25,
            UnlockConditions = new List<UnlockCondition>()
            {
                new UnlockCondition()
                {
                    Type = UnlockCondition.ConditionType.Technology,
                    TechnologyID = "redis"
                }
            }
            
        };
        WorldObjectTypes[WorldObjectType.Type.ALB] = new WorldObjectType()
        {
            DisplayName = "Load Balancer",
            PrefabId = "ServerALB",
            BuildTime = 30,
            DailyCost = 30,
            CanBeUpsized = false,
            LoadPerPacket = 1,
            UnlockConditions = new List<UnlockCondition>()
            {
                new UnlockCondition()
                {
                    Type = UnlockCondition.ConditionType.Technology,
                    TechnologyID = "load-balancer"
                }
            }
            
        };
        WorldObjectTypes[WorldObjectType.Type.Queue] = new WorldObjectType()
        {
            DisplayName = "Queue",
            PrefabId = "SQS",
            BuildTime = 30,
            DailyCost = 30,
            CanBeUpsized = true,
            LoadPerPacket = 25,
            UnlockConditions = new List<UnlockCondition>()
            {
                new UnlockCondition()
                {
                    Type = UnlockCondition.ConditionType.Technology,
                    TechnologyID = "sqs"
                }
            }
            
        };
        WorldObjectTypes[WorldObjectType.Type.WorkerServer] = new WorldObjectType()
        {
            DisplayName = "Background Worker",
            PrefabId = "ServerWorker",
            BuildTime = 30,
            DailyCost = 30,
            CanBeUpsized = true,
            LoadPerPacket = 25,
            UnlockConditions = new List<UnlockCondition>()
            {
                new UnlockCondition()
                {
                    Type = UnlockCondition.ConditionType.Technology,
                    TechnologyID = "sqs"
                }
            }
        };
        foreach (WorldObjectType worldObjectType in WorldObjectTypes.Values)
        {
            worldObjectType.Initialize();
        }


    }

    private void HandleReleaseChanged(ReleaseBase releaseBase, ReleaseBase.ReleaseState prevState)
    {

        InfrastructureInstance infra = GetInfrastructureInstanceByID("whiteboard");
            ReleaseBase openRelease = Releases.Find((r) => r.State != ReleaseBase.ReleaseState.DeploymentCompleted && r.State != ReleaseBase.ReleaseState.Failed);
            if (openRelease != null)
            {
                infra.HideAttentionIcon();
       
            }
            else
            {
                infra.ShowAttentionIcon();
            }

            
       
               
       
    }

    public float IncrStat(StatType stat, float value = 1)
    {
       
        var statval = Stats.Stats[stat].IncrStat(value);
        OnStatsChanged?.Invoke();
        return statval;
    }
  
    public void SetStat(StatType stat, float value)
    {
        Stats.Stats[stat].SetBaseValue(value);
        Stats.Stats[stat].RefreshValue();
        OnStatsChanged?.Invoke();
    }


    public float GetStat(StatType stat) => Stats.Stats[stat].Value;

 

    public float CalculateTotalDailyCost()
    {
        float totalCost = 0;
        foreach (var infra in ActiveInfrastructure)
        {
            if (infra.IsActive())
            {
                totalCost += infra.GetWorldObjectType().Stats.GetStatValue(StatType.Infra_DailyCost);
            }
        }
        
   
        return totalCost;
    }
    
    public void NotifyDailyCostChanged() => OnDailyCostChanged?.Invoke();

    public void UpdateInfrastructureVisibility()
    {
        foreach (InfrastructureInstance instance in ActiveInfrastructure)
        {   
            InfrastructureData infraData = instance.data;
            if (infraData.CurrentState == InfrastructureData.State.Locked && !instance.gameObject.activeSelf)
            {
                if (AreUnlockConditionsMet(instance))
                {
                    instance.gameObject.SetActive(true);
                    instance.GetComponent<InfrastructureInstance>().SetState(InfrastructureData.State.Unlocked);
                }
            }
        }
    }

    void SetupGameScene()
    {
     

        /*if (gridManager == null)
        {
            gridManager = FindObjectOfType<GridManager>();
            if (gridManager == null)
            {
                 gridManager = new GameObject("GridManager").AddComponent<GridManager>();
            }
        }*/
        

        gridManager.Init();

        // Center the camera on the board
        Vector3 centerWorld = gridManager.grid.CellToWorld(new Vector3Int(gridManager.gridWidth / 2, gridManager.gridHeight / 2, 0));
        Camera.main.transform.position = new Vector3(centerWorld.x, centerWorld.y, Camera.main.transform.position.z);

        // Zoom out to show the entire board, using the maxZoom from CameraController
        if (Camera.main.orthographic)
        {
            CameraController cameraController = FindObjectOfType<CameraController>();
            if (cameraController != null)
            {
                // Access maxZoom via reflection or make maxZoom public if needed
                // For now, assume a reasonable default or make maxZoom public in CameraController
                // I will add a temporary public accessor to CameraController.cs to make this work for now.
                Camera.main.orthographicSize = 12f; // Using the default maxZoom value from CameraController
            }
            else
            {
                Debug.LogWarning("CameraController not found, using default max zoom value.");
                Camera.main.orthographicSize = 12f; // Fallback to a reasonable default
            }
        }

        if (Camera.main.GetComponent<Physics2DRaycaster>() == null)
        {
            Camera.main.gameObject.AddComponent<Physics2DRaycaster>();
        }
        
        foreach (var infraData in AllInfrastructure)
        {
            Vector3 worldPos = gridManager.grid.CellToWorld(new Vector3Int(infraData.GridPosition.x, infraData.GridPosition.y, 0));
            Vector3 adjustedWorldPos = gridManager.AdjustWorldPointZ(worldPos);
            GameObject instanceGO = Instantiate(infraData.Prefab, adjustedWorldPos, Quaternion.identity);

            /*if (instanceGO.GetComponent<Collider2D>() == null)
            {
                var boxCollider = instanceGO.AddComponent<BoxCollider2D>();
                var spriteRenderer = instanceGO.GetComponentInChildren<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    boxCollider.size = spriteRenderer.bounds.size;
                }
            }*/

            InfrastructureInstance infraInstance = instanceGO.GetComponent<InfrastructureInstance>();

            if (infraInstance == null)
            {
                throw new SystemException($"Missing `InfrastructureInstance` Component for `{infraData.Id}`.");
            }
            infraInstance.GridPosition = infraData.GridPosition; // TODO: Remove this hackyness.
            infraInstance.Initialize(infraData);
           
            ActiveInfrastructure.Add(infraInstance);
            // Debug.Log($"Infrastructure '{infraData.DisplayName}' CHECK {infraData.CurrentState }.");
            if (infraData.CurrentState == InfrastructureData.State.Operational)
            {
                // Debug.Log($"Infrastructure '{infraData.DisplayName}' is now Operational.");
            }
            else if (AreUnlockConditionsMet(infraInstance))
            {
                //Debug.Log($"Infrastructure '{infraData.DisplayName}' is now UNLOCKED.");
                infraInstance.SetState(InfrastructureData.State.Unlocked);
            }
            else 
            {
                //Debug.Log($"Infrastructure '{infraData.DisplayName}' is now Locked.");
                infraInstance.SetState(InfrastructureData.State.Locked);
                instanceGO.SetActive(false);
            }
            
        }
        InfrastructureInstance desk = GetInfrastructureInstanceByID("boss-desk");
        if (desk != null)
        {
            GameObject npcGO = prefabManager.Create("BossNPC", desk.GetInteractionPosition());
            BossNPC bossNPC = npcGO.GetComponent<BossNPC>();
            bossNPC.Initialize();
            
        }
        else
        {
            Debug.LogError("Could not find 'boss-desk to spawn BossNPC.");
        }
        
   
    }

    public bool AreUnlockConditionsMet(InfrastructureInstance infrastructureInstance)
    {
        WorldObjectType worldObjectType = infrastructureInstance.GetWorldObjectType();
    
        List<UnlockCondition> unlockConditions = new List<UnlockCondition>();
        if (worldObjectType.UnlockConditions != null && worldObjectType.UnlockConditions.Count > 0)
        {
            unlockConditions.AddRange(worldObjectType.UnlockConditions);
        }

        if (infrastructureInstance.data.UnlockConditions.Count > 0)
        {
            unlockConditions.AddRange(infrastructureInstance.data.UnlockConditions);
        }

        if (unlockConditions.Count == 0)
        {
   
            return true;
        }
        
        foreach (UnlockCondition condition in unlockConditions)
        {
            switch (condition.Type)
            {
                default:
                case(UnlockCondition.ConditionType.Technology):
                    
                    Technology technology = GetTechnologyByID(condition.TechnologyID);
                    if (technology == null)
                    {
                        return false;
                        // throw new SystemException(
                        //     $"Cannot find Technology {condition.TechnologyID} - {infraData.ID}");
                    } 
                    
                    if(technology.CurrentState != Technology.State.Unlocked) return false;
                    break;
            }
        }
        return true;
    }

    public void PlanInfrastructure(InfrastructureInstance infra)
    {
        if (infra.data.CurrentState != InfrastructureData.State.Unlocked) return; // MUST BE UNLOCKED TO PLAN

        if (!AreUnlockConditionsMet(infra))
        {
            Debug.Log("Unlock conditions not met for this infrastructure.");
            return;
        }

        infra.GetComponent<InfrastructureInstance>().SetState(InfrastructureData.State.Planned);
        Debug.Log($"Successfully planned {infra.data.Id}.");
        UIManager.worldObjectDetailPanel.Close();
    }
    
    public void RequestInfrastructureResize(InfrastructureInstance instance, int sizeChange)
    {
        var resizeTask = new ResizeTask(instance, sizeChange);
        AddTask(resizeTask);
        UIManager.worldObjectDetailPanel.Close();
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
        var door = GetInfrastructureInstanceByID("door");
        if (door == null)
        {
            Debug.LogError("Cannot hire NPC because 'door' infrastructure was not found.");
            return;
        }

        GameObject npcObject = prefabManager.Create("NPCDevOps", door.transform.position);
        if (npcObject == null)
        {
            Debug.LogError("Failed to create 'NPCDevOps' from PrefabManager. Is the prefab configured?");
            return;
        }

        NPCDevOps npc = npcObject.GetComponent<NPCDevOps>();
        npc.Initialize(candidateData);
        
        NotifyDailyCostChanged();
    }

    public void ResetNPCs()
    {
        foreach (var npc in FindObjectsOfType<NPCDevOps>())
        {
            Destroy(npc.gameObject);
        }
        AllNpcs.Clear();
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
        OnTechnologyResearchStarted?.Invoke(tech);

        // Remove any existing research tasks
        AvailableTasks.RemoveAll(task => task is ResearchTask);
        
        // Add a new research task for the selected technology
        AddTask(new ResearchTask(tech));
        GetInfrastructureInstanceByID("desk").HideAttentionIcon();
    }

    public void ApplyResearchProgress(float researchGained)
    {
        if (CurrentlyResearchingTechnology == null) return;

        CurrentlyResearchingTechnology.CurrentResearchProgress += researchGained;

        if (CurrentlyResearchingTechnology.CurrentResearchProgress >= CurrentlyResearchingTechnology.ResearchPointCost)
        {
            CurrentlyResearchingTechnology.CurrentState = Technology.State.Unlocked;
            OnTechnologyUnlocked?.Invoke(CurrentlyResearchingTechnology);
            
            CurrentlyResearchingTechnology = null;
        }
    }

    public void UnlockAllTechnologies()
    {
        AllTechnologies = MetaGameManager.GetAllTechnologies();
        foreach (var tech in AllTechnologies)
        {
            if (tech.CurrentState != Technology.State.Unlocked)
            {
                tech.CurrentState = Technology.State.Unlocked;
                OnTechnologyUnlocked?.Invoke(tech);
            }
        }
        CurrentlyResearchingTechnology = null;
    }

    // Helper method to get a Technology by its ID
    public Technology GetTechnologyByID(string id)
    {
        return AllTechnologies.FirstOrDefault(t => t.TechnologyID == id);
    }
    public InfrastructureInstance GetInfrastructureInstanceByID(string id)
    {
        return ActiveInfrastructure.FirstOrDefault(t => t.data.Id == id);
    }
    public List<InfrastructureInstance> GetWorldObjectByType(WorldObjectType.Type type)
    {
        return ActiveInfrastructure.FindAll(t => t.Type == type);
    }

    public InfrastructureInstance GetRandomWorldObjectByType(WorldObjectType.Type type)
    {
        List<InfrastructureInstance> targets = ActiveInfrastructure.FindAll(t =>
        {
            return t.IsActive() && t.Type == type;
        });
        int i = Random.Range(0, targets.Count);
        return targets[i];
    }
    public void AddEffect(EffectBase effectBase)
    {
        Effects.Add(effectBase);
    }

    public void InvokeOnPhaseChange(GameLoopManager.GameState state)
    {
        OnPhaseChange?.Invoke(state);
    }

    public void InvokeReleaseChanged(ReleaseBase releaseBase, ReleaseBase.ReleaseState state)
    {
        OnReleaseChanged?.Invoke(releaseBase, state);
    }
    

    public void UpdateMetaProgress()
    {
        MetaProgressData prevMetaState = MetaGameManager.LoadProgress();
        MetaProgressData newMetaState = MetaGameManager.GetUpdatedMetaStats(ActiveInfrastructure);
        List<MetaChallengeBase> newlyPassedChallenges = MetaGameManager.CheckChallengeProgress(prevMetaState, newMetaState);
        MetaGameManager.SaveProgress(newMetaState);
        if (newlyPassedChallenges.Count > 0)
        {
            string alertText = "";
            foreach (MetaChallengeBase challenge in newlyPassedChallenges)
            {
                alertText += $"Unlocked: {challenge.DisplayName!}\n";
            } 

            UIManager.ShowAlert(alertText);
        }

    }

    public List<ReleaseBase> GetOpenReleases()
    {
        List<ReleaseBase> releases = new List<ReleaseBase>();
        foreach (ReleaseBase release in Releases)
        {
            switch (release.State)
            {
                case(ReleaseBase.ReleaseState.DeploymentCompleted):
                case(ReleaseBase.ReleaseState.Failed):
                    break;
                default:
                    releases.Add(release);
                    break;
            }
        }
        return releases;
    }

    public void AddModifier(ModifierBase modifierBase)
    {

        Modifiers.Modifiers.Add(modifierBase);
        modifierBase.Apply();
    }
    public T GetRandomInfrastructureInstanceByClass<T>() where T : class
    {
        List<T> results = GetInfrastructureInstanceByClass<T>();
        if (results.Count == 0)
        {
            return null;
        }

        int i = Random.Range(0, results.Count);
        return results[i];
    }
    public List<T> GetInfrastructureInstanceByClass<T>()
    {
        List<T> results = new List<T>();
        foreach (WorldObjectBase wo in ActiveInfrastructure)
        {
            T component = wo.GetComponent<T>();
            if (component != null)
            {
                results.Add(component);
            }
        }

        return results;
    }

    public List<InfrastructureInstance> GetInfrastructureInstancesByType(Type type)
    {
        List<InfrastructureInstance> results = new List<InfrastructureInstance>();
        foreach (InfrastructureInstance instance in ActiveInfrastructure)
        {
            if (instance.GetType() == type)
            {
                results.Add(instance);
            }
        }
        return results;
    }

    public void HideAllAttentionIcons()
    {
        foreach (WorldObjectBase worldObjectBase in ActiveInfrastructure)
        {
            worldObjectBase.HideAttentionIcon();
        }
    }

    public List<NetworkPacketData> GetNetworkPacketDatas()
    {
        return NetworkPacketDatas;
    }
}