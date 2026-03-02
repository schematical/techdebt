using System.Collections.Generic;
using Stats;
using UnityEngine;

namespace Infrastructure
{
    public class WorldObjectType
    {
        public enum Type
        {
            Misc,
            InternetPipe,
            ApplicationServer,
            DedicadedDB,
            Redis,
            ALB,
            BinaryStorage,
            CDN,
            Queue,
            WorkerServer,
            BigDesk
        }
        
        public string DisplayName;
        public string PrefabId;
        public List<UnlockCondition> UnlockConditions;
        public List<InfrastructureDataNetworkPacket> networkPackets = new List<InfrastructureDataNetworkPacket>();

        public float DailyCost = 0;
        public float BuildTime = 0f;
        // public int LoadPerPacket = 0; 
        // public int CostPerPacket = 0;
        public float MaxLoad = 100;
        public float LoadRecoveryRate = 0f;
        public bool CanBeUpsized = false;
  
        public List<NetworkConnection> NetworkConnections; // Array of NetworkConnection objects
        public StatsCollection Stats { get; private set; } = new StatsCollection();
        public Vector3 interactionPositionOffset = new Vector3(0.0f, 1.0f, 0.0f);

        public void Initialize()
        {
            Stats.Add(new StatData(StatType.Infra_DailyCost, DailyCost));
            Stats.Add(new StatData(StatType.Infra_BuildTime, BuildTime));
            // Stats.Add(new StatData(StatType.Infra_LoadPerPacket, LoadPerPacket));
            Stats.Add(new StatData(StatType.Infra_MaxLoad, MaxLoad));
            Stats.Add(new StatData(StatType.Infra_LoadRecoveryRate, LoadRecoveryRate));
            Stats.Add(new StatData(StatType.TechDebt, 0f));
            Stats.Add(new StatData(StatType.Infra_MaxSize, 2)); // Todo get this number from a meta unlock.
            foreach (InfrastructureDataNetworkPacket networkPacket in networkPackets)
            {
                networkPacket.Init();
            }
        }

    }
}