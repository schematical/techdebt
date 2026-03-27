using System;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DefaultNamespace;
using DefaultNamespace.Util.Analytics;
using Effects;
using Tutorial;
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
using Unity.Services.Analytics;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using UnityEngine.Analytics;
using UnityEngine.Serialization;

public class GameManager : MonoBehaviour, iModifiable
{
    public enum GameManagerState { MainMenu, Playing }
    private static GameManager _instance;
    public bool recordAnalytics = false;
    
    public enum GlobalNetworkPacketState { Running, Frozen}
    public static GameManager Instance
    {
        get
        {
            return _instance;
        }
    }
    
    public GameManagerState State =  GameManagerState.MainMenu;

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
    protected List<Technology> AllTechnologies;
    public Dictionary<WorldObjectType.Type, WorldObjectType> WorldObjectTypes = new Dictionary<WorldObjectType.Type, WorldObjectType>();
    public Map Map;
    [SerializeField] public GridManager gridManager;
    protected List<NetworkPacketData> NetworkPacketDatas  = new List<NetworkPacketData>();
    public StatsCollection Stats { get; private set; } = new StatsCollection();
    public MetaStatCollection MetaStats { get; private set; } = new MetaStatCollection();
    
    public Stats.StatsCollection GlobalStats { get; private set; } = new StatsCollection();
    
    public List<EventBase> Events { get; private set; } = new List<EventBase>();
    public List<EffectBase> Effects { get; private set; } = new List<EffectBase>();
    
    public List<EventBase> CurrentEvents { get; private set; } = new List<EventBase>();
    public static event System.Action OnStatsChanged;
    public static event System.Action OnDailyCostChanged;
    public static event System.Action<InfrastructureInstance, InfrastructureData.State?> OnInfrastructureStateChange;
    public static event System.Action<Technology, Technology.State> OnTechnologyStateChange;
    public static event System.Action OnCurrentEventsChanged;
    
    public static event System.Action<ReleaseBase, ReleaseBase.ReleaseState> OnReleaseChanged;
    public static event System.Action<GameLoopManager.GameState> OnPhaseChange;
    
    public GlobalNetworkPacketState NetworkPacketState = GlobalNetworkPacketState.Running;
    
    public TutorialManager TutorialManager;

    public Technology CurrentlyResearchingTechnology { get; private set; }
    public ModifierCollection Rewards { get; set; } = new  ModifierCollection();


    // --- Item Spawning ---
    private float _eventTimer;


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

  


    public NetworkPacketData GetRandomNetworkPacketData()
    {
       
       
        float probTotal = 0f;
        foreach (NetworkPacketData npData in NetworkPacketDatas)
        {
            // Debug.Log($"{npData.Type} - Prob: {npData.probilitly} Total Before: {probTotal}");
            probTotal += npData.GetProbability();
        }
        float index = Random.Range(0, probTotal);
    
        float currFloor = 0;
        NetworkPacketData foundData = null;
        foreach (NetworkPacketData npData in NetworkPacketDatas)
        {
            if (
                index >= currFloor &&
                index < currFloor + npData.GetProbability()
            )
            {
                foundData = npData;
                break;
            }

            currFloor += npData.GetProbability();
        }

        if (foundData == null)
        {
            Debug.LogError($"GetRandomNetworkPacketData - Not found - probTotal: {probTotal} - currFloor: {currFloor}");
        }
        // Debug.Log($"GetRandomNetworkPacketData - {index} - {foundData.Type}");
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
                if (TutorialManager != null)
                {
                    TutorialManager.Trigger(TutorialStepId.NetworkPacket_Failed);
                }
            break;
            case(NetworkPacket.State.Stolen):
                IncrStat(StatType.PacketsFailed);
                int cost = 10;
                IncrStat(StatType.Money, cost * -1);
                UIManager.moneyPanel.ExplodeCoins(10);
                //TODO: Add cost to GameLoopManager.
            break;
            case(NetworkPacket.State.Running):
                float latency = packet.GetLatency();

                FloatingTextFactory.ShowText($"{Math.Round(packet.GetLatency()*100)}ms", packet.transform.position - new Vector3(0,1), Color.coral);
                IncrStat(StatType.TotalNetworkPacketLatency, latency);
            	 packetsServiced = IncrStat(StatType.PacketsSucceeded);
                break;
            default:
                throw new NotImplementedException();
        }
        packet.Reset();
         
    }

    public void CheckEvents()
    {
        float totalProb = 0;
        List<EventBase> possibleEvents = new List<EventBase>();
        foreach (EventBase e in Events)
        {
            if (e.IsPossible())
            {
                totalProb += e.GetProbability();
                possibleEvents.Add(e);
            }
        }

        float selectedIndex = Random.value * (totalProb + 10);
        if (selectedIndex > totalProb)
        {
            // Debug.Log($"Nothing Event Hit {selectedIndex} / {totalProb}");
            return;
        }
        float currIndex = 0;
       
        foreach (EventBase e in possibleEvents)
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
        CurrentEvents.Remove(e);
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
#if UNITY_WEBGL && !UNITY_EDITOR
        recordAnalytics = true;
        InitializationOptions options = new InitializationOptions();
        options.SetEnvironmentName("itch");
        UnityServices.InitializeAsync(options);
        Debug.Log("Starting Analytics: ");
#endif
        _instance = this;
        
        OnInfrastructureStateChange += HandleInfrastructureStateChange;
        OnTechnologyStateChange += HandleTechnologyStateChange;
        OnReleaseChanged += HandleReleaseChanged;
        
        UIManager.Initialize();
        ShowMainMenu();
    }

    public void ShowMainMenu()
    {
        State = GameManagerState.MainMenu;

        Reset();
        StartDemo();
        UIManager.ShowMainMenu();
    }

    public void Reset()
    {

        UIManager.Reset();
        cameraController.Reset();
        prefabManager.Reset();
        Releases.Clear();
        GameLoopManager.Reset();
        CurrentlyResearchingTechnology = null;
        if (TutorialManager != null)
        {
            TutorialManager.End();
            TutorialManager = null;
        }
        AvailableTasks.Clear();
        ReleaseBase.GlobalVersion = 0;
        foreach (NPCBase npc in AllNpcs)
        {
            npc.gameObject.SetActive(false);
        }

        AllNpcs.Clear();
        foreach (WorldObjectBase worldObjectBase in ActiveInfrastructure)
        {
            worldObjectBase.Reset();
        }
    }
    public void StartNewGame()
    {
        Analytics.CustomEvent("StartNewGame");
        State = GameManagerState.Playing;
        Reset();
        
        Initialize();

        
        SetupRun();
        TutorialManager = new TutorialManager();
        TutorialManager.Start();
        UpdateInfrastructureVisibility();
        InfrastructureUpdateNetworkTargets();
  
        cameraController.EnableCameraInput();
        UIManager.ShowGameUI();

        
     
        /*InfrastructureInstance productRoadMapInfra = GetInfrastructureInstanceByID("product-road-map");
        if (productRoadMapInfra.IsActive())
        {
            UIManager.productRoadMap.Show(UIProductRoadMap.State.Select);
        }
        else
        {*/
            Map.GetCurrentStage().SetSelectedLevel(0);
            // GameLoopManager.BeginPlanPhase();
       
        // }
        TutorialManager.StartNewGameCheck();
    }
    public void StartDemo()
    {
        Initialize();
        SetupRun();
        UnlockAllTechnologies();
        BuildAllWorldObjects();
        SetStat(StatType.Traffic, 100);
        UpdateInfrastructureVisibility();
        InfrastructureUpdateNetworkTargets();
        cameraController.DisableCameraInput();
        NPCDevOps npc = HireNPCDevOps(new NPCDevOpsData { DailyCost = 100 });
        for (int i = 0; i < 3; i++)
        {
            SpawnNPCBug();
        }
        InternetPipe internetPipe = GameManager.Instance.GetRandomInfrastructureInstanceByClass<InternetPipe>();

        GameObject npcGO = GameManager.Instance.prefabManager.Create("NPCXSS", internetPipe.transform.position);

        NPCXSS npcXSS = npcGO.GetComponent<NPCXSS>();
        npcXSS.Initialize();
        
        
        cameraController.ZoomToAndFollow(npc.transform);
        /*InfrastructureInstance productRoadMapInfra = GetInfrastructureInstanceByID("product-road-map");
        if (productRoadMapInfra.IsActive())
        {
            UIManager.productRoadMap.Show(UIProductRoadMap.State.Select);
        }
        else
        {*/
        Map.GetCurrentStage().SetSelectedLevel(0);
        GameLoopManager.BeginDemo();
        // }
    }

    private void BuildAllWorldObjects()
    {
        foreach (InfrastructureInstance worldObjectBase in ActiveInfrastructure)
        {
            worldObjectBase.SetState(InfrastructureData.State.Operational);
        }
    }

    void OnDestroy()
    {
        OnInfrastructureStateChange -= HandleInfrastructureStateChange;
        OnTechnologyStateChange -= HandleTechnologyStateChange;
        if (_instance == this)
        {
            _instance = null;
        }
    }

    private void HandleTechnologyStateChange(Technology tech, Technology.State previousState)
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




    private void FixedUpdate()
    {
        if (GameLoopManager.CurrentState != GameLoopManager.GameState.Play) return;
        if (
            UIManager.IsPausedState()
        )
        {
            return;
        }
      
        // Iterate over a copy of the list to prevent modification during enumeration errors.
        foreach (EffectBase effect in Effects.ToList())
        {
            effect.FixedUpdate();
        }

        TickNetworkPackets();

        // Delivery NPC Spawning Logic
        float techDebtAccumulationRate = GetStatValue(StatType.TechDebt_AccumulationRate);
        IncrStat(StatType.TechDebt, Time.fixedDeltaTime * techDebtAccumulationRate);
        _eventTimer -= Time.fixedDeltaTime;
        if (_eventTimer <= 0)
        {
            //Find Random Event
            CheckEvents();
            _eventTimer = GetStatValue(StatType.EventCheckEverySeconds);
        }
        
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

        float secondsBetweenPackets = GameLoopManager.GetDayDurationSeconds() / GetStatValue(StatType.Traffic);
        timeSinceLastPacket += Time.deltaTime;
    

        if (timeSinceLastPacket >= secondsBetweenPackets)
        {
            
            NetworkPacketData data = GetRandomNetworkPacketData();
            List<InternetPipe> instances = GetInfrastructureInstanceByClass<InternetPipe>();
            if (instances.Count == 0)
            {
                throw new SystemException("Cannot find any `InternetPipe` instances");
            }
            InternetPipe pipe = instances[Random.Range(0, instances.Count)];
            if (pipe.CurrConnections.Count == 0)
            {
                return;
            }
            timeSinceLastPacket = 0f;
            pipe.SendPacket(data);
         
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



    private void Initialize()
    {
 
        ActiveInfrastructure.Clear();
        AvailableTasks.Clear();
        Stats.Clear();
        Stats.Add(new StatData(StatType.Money, 230f)
        {
            IsModifiable = false,
            DisplayType =  StatData.StatDataDisplayType.Dollar
        });
       
        Stats.Add(new StatData(StatType.TechDebt, 0f)
        {
            IsModifiable = false,
            BelowZeroBehavior = StatData.StatDataBelowZeroBehavior.SetZero,
            DisplayType =  StatData.StatDataDisplayType.Percentage
        });
        Stats.Add(new StatData(StatType.Traffic, 30)
        {
        });
        Stats.Add(new StatData(StatType.PacketsSent, 0f)
        {
            IsModifiable = false,
        });
        Stats.Add(new StatData(StatType.PacketsSucceeded, 0f)
        {
            IsModifiable = false,
        });
        
        Stats.Add(new StatData(StatType.PacketsFailed, 0f)
        {
            IsModifiable = false,
        });
        Stats.Add(new StatData(StatType.TotalNetworkPacketLatency, 0f)
        {
            IsModifiable = false,
        });
        Stats.Add(new StatData(StatType.Difficulty, 1.15f)
        {
            DisplayType =  StatData.StatDataDisplayType.Percentage
        });
        Stats.Add(new StatData(StatType.ItemDropChance, 0.1f)
        {
            DisplayType =  StatData.StatDataDisplayType.Percentage
        });
        Stats.Add(new StatData(StatType.Release_Quality_Multiplier, 1)
        {
            
        });
        Stats.Add(new StatData(StatType.EventCheckEverySeconds, 15));
        Stats.Add(new StatData(StatType.Infra_InputValidation, 0.1f));
        Stats.Add(new StatData(StatType.AttackPossibility, 0f){
            DisplayType =  StatData.StatDataDisplayType.Percentage,
            IsModifiable = false,
        });
        Stats.Add(new StatData(StatType.AttackPossibilityAccumulationRate, 0.5f){
        });
        Stats.Add(new StatData(StatType.TechDebt_AccumulationRate, 0.01f){
            DisplayType =  StatData.StatDataDisplayType.Percentage
        });
        
        NetworkPacketData coin = new NetworkPacketData(0f)
        {
            Type = NetworkPacketData.PType.Purchase,
            baseLoad = 20,
            prefabId = "FileCoin"
        };
        coin.Stats.Add(new StatData(StatType.NetworkPacket_ValueMin, 10));
        coin.Stats.Add(new StatData(StatType.NetworkPacket_ValueMax, 20));
        NetworkPacketDatas = new List<NetworkPacketData>()
        {
            coin,
            new NetworkPacketData(45)
            {
                Type = NetworkPacketData.PType.Text,
                baseLoad = 10,
                prefabId = "NetworkPacket"
            },
            new NetworkPacketData(45)
            {
                Type = NetworkPacketData.PType.Image,
                baseLoad = 20,
                prefabId = "FileCat"
            },
            new NetworkPacketData(0)
            {
                Type = NetworkPacketData.PType.MaliciousText,
                baseLoad = 1000,
                prefabId = "NetworkPacketAttack"
            },
            new NetworkPacketData(0)
            {
                Type = NetworkPacketData.PType.BatchJob,
                baseLoad = 0,
                prefabId = "BatchJobNetworkPacket"
            },
            new NetworkPacketData(0)
            {
                Type = NetworkPacketData.PType.PII,
                baseLoad = 0,
                prefabId = "PIINetworkPacket"
            },
            new NetworkPacketData(0)
            {
                Type = NetworkPacketData.PType.SQLInjection,
                baseLoad = 0,
                prefabId = "SQLInjectionNetworkPacket"
            },
        };
 
        Events.Clear();
        Events.Add(new ItemDeliveryEvent());
        Events.Add(new SpawnBugEvent());
        Events.Add(new SpawnXSSEvent());
        Events.Add(new SpawnSQLInjectionEvent());

        Items.Clear();
        Items.Add(new ItemData() { Id = "NukeItem", Probability = 1});
        Items.Add(new ItemData() { Id = "FreezeTimeItem", Probability = 1});
        Items.Add(new ItemData() { Id = "EnergyDrinkItem", Probability = 1});
        

        WorldObjectTypes.Clear();
        WorldObjectTypes[WorldObjectType.Type.Misc] = new MiscWOType();
        WorldObjectTypes[WorldObjectType.Type.BigDesk] = new BigDeskWOType();
        WorldObjectTypes[WorldObjectType.Type.WhiteBoard] = new WhiteBoardWOType();
        WorldObjectTypes[WorldObjectType.Type.KanbanBoard] = new KanbanBoardWOType();
        WorldObjectTypes[WorldObjectType.Type.ProductRoadMap] = new ProductRoadMapWOType();
        WorldObjectTypes[WorldObjectType.Type.OrgChart] = new OrgChartMapWOType();
        WorldObjectTypes[WorldObjectType.Type.InternetPipe] = new InternetPipeWOType();
        WorldObjectTypes[WorldObjectType.Type.ApplicationServer] = new ApplicationServerWOType();
        WorldObjectTypes[WorldObjectType.Type.BinaryStorage] = new BinaryStorageWOType();
        WorldObjectTypes[WorldObjectType.Type.CDN] = new CDNWOType();
        WorldObjectTypes[WorldObjectType.Type.DedicatedDB] = new DedicatedDBWOType();
        WorldObjectTypes[WorldObjectType.Type.Redis] = new RedisWOType();
        WorldObjectTypes[WorldObjectType.Type.ALB] = new ALBWOType();
        WorldObjectTypes[WorldObjectType.Type.Queue] = new QueueWOType();
        WorldObjectTypes[WorldObjectType.Type.WorkerServer] = new WorkerServerWOType();
        WorldObjectTypes[WorldObjectType.Type.CodePipeline] = new CodePipelineWOType();
        WorldObjectTypes[WorldObjectType.Type.WaterCooler] = new WaterCoolerWOType();
        WorldObjectTypes[WorldObjectType.Type.WAF] = new WAFWOType();
        WorldObjectTypes[WorldObjectType.Type.SecretManager] = new SecretManagerWOType();
        WorldObjectTypes[WorldObjectType.Type.Cognito] = new CognitoWOType();
        WorldObjectTypes[WorldObjectType.Type.EmailService] = new EmailServiceWOType();
        WorldObjectTypes[WorldObjectType.Type.CloudWatchMetrics] = new CloudWatchMetricsWOType();
        WorldObjectTypes[WorldObjectType.Type.SNS] = new SNSWOType();
        
        foreach (WorldObjectType worldObjectType in WorldObjectTypes.Values)
        {
            worldObjectType.Initialize();
        }

        Map = new Map();
        Map.Randomize();
        
           
       
        AllTechnologies = MetaGameManager.GetAllTechnologies();
        foreach (Technology technology in AllTechnologies)
        {
            technology.CurrentResearchProgress = 0;
            technology.CurrentState = technology.OriginalState;
            
            
        }
        List<MetaChallengeBase> unlockedChallenges = MetaGameManager.GetUnlockedChallenges();
        foreach (MetaChallengeBase challenge in unlockedChallenges)
        {
            // Debug.Log($"Unlocked: {challenge.ChallengeID}");
            foreach (RewardBase reward in challenge.Rewards)
            {
                Technology technology = null;
                // Debug.Log($"- Reward: {reward.Type}");
                reward.Apply();
            }
        }

    }

    private void HandleReleaseChanged(ReleaseBase releaseBase, ReleaseBase.ReleaseState prevState)
    {

        InfrastructureInstance infra = GetInfrastructureInstanceByID("whiteboard");
        ReleaseBase openRelease = Releases.Find((r) => r.State != ReleaseBase.ReleaseState.DeploymentCompleted && r.State != ReleaseBase.ReleaseState.Failed);
        if (
            openRelease != null ||
            !infra.IsActive()
        ){
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


    public float GetStatValue(StatType stat) => Stats.Stats[stat].Value;

 

    public float CalculateTotalDailyCost()
    {
        float totalCost = 0;
        foreach (var infra in ActiveInfrastructure)
        {
            if (infra.IsActive())
            {
                totalCost += infra.GetDailyCost();
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

    void SetupRun()
    {
        _eventTimer = GetStatValue(StatType.EventCheckEverySeconds);
        
        gridManager.Init();

        // Center the camera on the board
        /*Vector3 centerWorld = gridManager.grid.CellToWorld(new Vector3Int(gridManager.gridWidth / 2, gridManager.gridHeight / 2, 0));
        Camera.main.transform.position = new Vector3(centerWorld.x, centerWorld.y, Camera.main.transform.position.z);
        */

        // Zoom out to show the entire board, using the maxZoom from CameraController
        if (Camera.main.orthographic)
        {
            CameraController cameraController = FindObjectOfType<CameraController>();
            Camera.main.orthographicSize = 12f; 
            
        }

        if (Camera.main.GetComponent<Physics2DRaycaster>() == null)
        {
            Camera.main.gameObject.AddComponent<Physics2DRaycaster>();
        }
        
        foreach (InfrastructureData infraData in AllInfrastructure)
        {
            Vector3 worldPos = gridManager.grid.CellToWorld(new Vector3Int(infraData.GridPosition.x, infraData.GridPosition.y, 0));
            Vector3 adjustedWorldPos = gridManager.AdjustWorldPointZ(worldPos);
            GameObject instanceGO = Instantiate(infraData.Prefab, adjustedWorldPos, Quaternion.identity);

            InfrastructureInstance infraInstance = instanceGO.GetComponent<InfrastructureInstance>();

            if (infraInstance == null)
            {
                throw new SystemException($"Missing `InfrastructureInstance` Component for `{infraData.Id}`.");
            }
            infraInstance.GridPosition = infraData.GridPosition; // TODO: Remove this hackyness.
            infraData.CurrentState = infraData.InitialState;
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
                //Debug.Log($"Infrastructure '{infraData.DisplayName}' is now Active.");
                infraInstance.SetState(InfrastructureData.State.Locked);
                instanceGO.SetActive(false);
            }
            
        }
        /*InfrastructureInstance bossDesk = GetInfrastructureInstanceByID("boss-desk");
        if (bossDesk != null)
        {
            GameObject npcGO = prefabManager.Create("BossNPC", bossDesk.GetInteractionPosition());
            BossNPC bossNPC = npcGO.GetComponent<BossNPC>();
            bossNPC.Initialize();
            
        }
        else
        {
            Debug.LogError("Could not find 'boss-desk to spawn BossNPC.");
        }*/
        InfrastructureInstance desk = GetInfrastructureInstanceByID("desk");
        GameObject sGO = prefabManager.Create("SchematicalBot",
            desk.transform.position + new Vector3(-4, 0));
        NPCSchematicalBot schematicalBot = sGO.GetComponent<NPCSchematicalBot>();
        schematicalBot.Initialize();
  
        cameraController.ZoomTo(desk.transform);
        
      
  
   
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
        return AreUnlockConditionsMet(unlockConditions);
    }
    public bool AreUnlockConditionsMet( List<UnlockCondition> unlockConditions) {
        if (unlockConditions == null || unlockConditions.Count == 0)
        {
            return true;
        }
        
        foreach (UnlockCondition condition in unlockConditions)
        {
            if (!condition.IsUnlocked())
            {
                return false;
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

    public NPCDevOps HireNPCDevOps(NPCDevOpsData candidateData)
    {
        WorldObjectBase door = GetInfrastructureInstanceByID("door");
        if (door == null)
        {
            Debug.LogError("Cannot hire NPC because 'door' infrastructure was not found.");
            return null;
        }

        GameObject npcObject = prefabManager.Create("NPCDevOps", door.transform.position + new Vector3(-0.5f,-0.5f, -0.5f));
        if (npcObject == null)
        {
            Debug.LogError("Failed to create 'NPCDevOps' from PrefabManager. Is the prefab configured?");
            return null;;
        }

        NPCDevOps npc = npcObject.GetComponent<NPCDevOps>();
        npc.Initialize(candidateData);
        
        NotifyDailyCostChanged();
        return npc;
    }

  

    public void SelectTechnologyForResearch(Technology tech)
    {
        if (tech == null || tech.CurrentState != Technology.State.Locked)
        {
            Debug.LogWarning($"Technology_Locked '{tech?.DisplayName}' cannot be researched because its state is not 'Active'.");
            return;
        }

        if (CurrentlyResearchingTechnology != null)
        {
            CurrentlyResearchingTechnology.OnInterrupt();
            CurrentlyResearchingTechnology = null;
        }

        if (tech.UnlockConditions != null)
        {
            foreach (UnlockCondition condition in tech.UnlockConditions)
            {
            
                if (!condition.IsUnlocked())
                {
                    return;
                }
            }
        }

        CurrentlyResearchingTechnology = tech;
        Technology.State previousState = tech.CurrentState;
        tech.CurrentState = Technology.State.Researching;
        OnTechnologyStateChange?.Invoke(tech, previousState);

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

        if (CurrentlyResearchingTechnology.CurrentResearchProgress >= CurrentlyResearchingTechnology.ResearchTime)
        {
            Technology.State previousState = CurrentlyResearchingTechnology.CurrentState;
            CurrentlyResearchingTechnology.CurrentState = Technology.State.Unlocked;
            OnTechnologyStateChange?.Invoke(CurrentlyResearchingTechnology, previousState);
            
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
                Technology.State previousState = tech.CurrentState;
                tech.CurrentState = Technology.State.Unlocked;
                OnTechnologyStateChange?.Invoke(tech, previousState);
            }
        }
        CurrentlyResearchingTechnology = null;
    }

    // Helper method to get a Technology_Locked by its ID
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
        if (targets.Count == 0)
        {
            // throw new Exception($"No world object found for type '{type}'");
            return null;// This is a legit possibility.
        }
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
        MetaProgressData newMetaState = MetaGameManager.GetUpdatedMetaStats(WorldObjectTypes.Values.ToList());
        List<MetaChallengeBase> newlyPassedChallenges = MetaGameManager.CheckChallengeProgress(prevMetaState, newMetaState);
        MetaGameManager.SaveProgress(newMetaState);
        //TODO: Move this to be queued to show at the end of the run.
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

    public void AddModifier(RewardBase modifierBase)
    {
        Rewards.Rewards.Add(modifierBase);
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
    public NetworkPacketData GetNetworkPacketDataByType( NetworkPacketData.PType pType)
    {
        NetworkPacketData networkPacketData = NetworkPacketDatas.Find((data => data.Type == pType));
        if (networkPacketData == null)
        {
            foreach (NetworkPacketData networkPacketData2 in GameManager.Instance.GetNetworkPacketDatas())
            {
                Debug.LogError($"NetworkPacketData {networkPacketData2.Type}");
            }
            throw new System.Exception("No network packet found `NetworkPacketData.PType.Purchase`");
        };
        return networkPacketData;
    }

    public ReleaseBase GetCurrentRelease()
    {
        if (Releases.Count == 0)
        {
            return null;
        }
        return Releases[Releases.Count - 1];
    }

    public void InfrastructureUpdateNetworkTargets()
    {
        foreach (InfrastructureInstance infrastructureInstance in ActiveInfrastructure)
        {
            infrastructureInstance.UpdateNetworkTargets();
        }
    }

    public List<Technology> GetAllTechnologies()
    {
        return AllTechnologies;
    }


    public void RecordEvent(Unity.Services.Analytics.Event myEvent)
    {
        if (!recordAnalytics)
        {
            Debug.Log("Skipping Analytics");
            return;
        }
        AnalyticsService.Instance.RecordEvent(myEvent);
        Debug.Log("AnalyticsService.Instance.RecordEvent: " + myEvent);
    }
}