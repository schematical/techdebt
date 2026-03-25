using System;
using System.Collections.Generic;
using System.Linq;
using DefaultNamespace;
using MetaChallenges;
using Stats;
using Tutorial;
using Unity.VisualScripting;
using UnityEngine;

namespace Infrastructure
{
    public class WorldObjectType: iModifiable
    {
        public enum Type
        {
            Misc,
            InternetPipe,
            ApplicationServer,
            DedicatedDB,
            Redis,
            ALB,
            BinaryStorage,
            CDN,
            Queue,
            WorkerServer,
            BigDesk,
            WhiteBoard,
            KanbanBoard,
            ProductRoadMap,
            OrgChart,
            WaterCooler,
            CodePipeline,
            WAF,
            SecretManager,
            Cognito,
            EmailService,
            CloudWatchMetrics,
            SNS
        }
        
        public string DisplayName;
        public string PrefabId;
        public TutorialStepId  TutorialStepId = TutorialStepId.None;
        public List<UnlockCondition> UnlockConditions;
        public List<InfrastructureDataNetworkPacket> networkPackets = new List<InfrastructureDataNetworkPacket>();

        public float DailyCost = 0;
        public float BuildTime = 0f;
        // public int LoadPerPacket = 0; 
        // public int CostPerPacket = 0;
        public float MaxLoad = 100;
        public float LoadRecoveryRate = 0f;
        public bool CanBeUpsized = false;
        public bool ShowInGlobalDisplay = false;
  
        public List<NetworkConnection> NetworkConnections; // Array of NetworkConnection objects
        public StatsCollection Stats { get; private set; } = new StatsCollection();
        public Vector3 interactionPositionOffset = new Vector3(0.0f, 1.0f, 0.0f);
        public Type type;
        public MetaStatCollection metaStatCollection = new MetaStatCollection();
        protected string sizeTechnologyPrefix = null;
        
        public string GetTypeAsId()
        {
            return System.Text.RegularExpressions.Regex.Replace(type.ToString(), "([a-z])([A-Z])", "$1-$2").ToLower();
        }
        public void Initialize()
        {
            Stats.Add(new StatData(StatType.Infra_DailyCost, DailyCost));
            Stats.Add(new StatData(StatType.Infra_BuildTime, BuildTime));
            // Stats.Add(new StatData(StatType.Infra_LoadPerPacket, LoadPerPacket));
            Stats.Add(new StatData(StatType.Infra_MaxLoad, MaxLoad));
            Stats.Add(new StatData(StatType.Infra_LoadRecoveryRate, LoadRecoveryRate));
            Stats.Add(new StatData(StatType.TechDebt, 0f));
            Stats.Add(new StatData(StatType.Infra_MaxSize, 2)); // Todo get this number from a meta unlock.
            Stats.Add(new StatData(StatType.Infra_LatencyStartsAtLoad, 0.1f) { DisplayType = StatData.StatDataDisplayType.Percentage}); 
            foreach (InfrastructureDataNetworkPacket networkPacket in networkPackets)
            {
                networkPacket.Init();
            }
        }

        public void IncrMetaStat(MetaStat metaStat, int value = 1)
        {
            metaStatCollection.Incr(metaStat, value);
        }

        public int GetMetaStat(MetaStat infraMaxSize)
        {
            return metaStatCollection.Get(infraMaxSize);
        }

        public void SetMetaStat(MetaStat infraMaxSize, int currentSizeLevel)
        {
            metaStatCollection.Set(infraMaxSize, currentSizeLevel);
        }

        public bool IsUnlocked()
        {
            return GameManager.Instance.AreUnlockConditionsMet(UnlockConditions);
        }

        public bool DisplayInGlobalUI()
        {
            if (!IsUnlocked())
            {
                return false;
            }

            return ShowInGlobalDisplay;
        }

        public InfraSize GetMaxSize()
        {
            
            InfraSize size = InfraSize.Small;

            if (sizeTechnologyPrefix == null)
            {
                Debug.Log($"No `sizeTechnologyPrefix` set returning {size}");
                return size;
            }

            foreach (InfraSize _size in Enum.GetValues(typeof(InfraSize)))
            {
                if (_size == InfraSize.Small)
                {
                    continue;
                }
                string technologyId = $"{sizeTechnologyPrefix}-size-{_size.ToString().ToLower()}";
                Debug.Log($"GetMaxSize: {technologyId}");
                Technology technology = GameManager.Instance.GetTechnologyByID(technologyId);
                if (technology == null)
                {
                    Debug.LogWarning($"{technologyId} not found - setting {DisplayName} Max Size to {size}");
                    return size;
                }
                if (!technology.IsUnlocked())
                {
                    return size;
                }
                size = _size;
            }
            Debug.Log($"GetMaxSize End: {size}");
            return size;
        }
    }
}