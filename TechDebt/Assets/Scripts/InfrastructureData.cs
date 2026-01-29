using UnityEngine;
using System;
using System.Collections.Generic;
using Stats;

[Serializable]
public class InfrastructureData
{
    public enum State { Locked, Unlocked, Planned, Operational, Frozen }

    public string ID = Guid.NewGuid().ToString(); // Unique identifier for this instance
    public string DisplayName;
    public string Type; // e.g., "WebServer", "Database", "LoadBalancer"
    public GameObject Prefab;
    public Vector2Int GridPosition;
    public UnlockCondition[] UnlockConditions;
    public List<InfrastructureDataNetworkPacket> networkPackets = new List<InfrastructureDataNetworkPacket>();

    public float DailyCost = 100;
    public float BuildTime = 5f;
    public int LoadPerPacket = 20; 
    public int CostPerPacket = 0;
    public float MaxLoad = 100;
    public float LoadRecoveryRate = 50f;
    public bool CanBeUpsized = false;
    public StatsCollection Stats { get; private set; } = new StatsCollection();

  

    public State CurrentState = State.Locked;
    public List<NetworkConnection> NetworkConnections; // Array of NetworkConnection objects
    public Vector3 interactionPositionOffset = Vector3.zero;
}

[Serializable]
public class InfrastructureDataNetworkPacket
{
    public enum NCRouteType { Return, End }
    public NetworkPacketData.PType PacketType;
    public int loadPerPacket = 20;
    public int cost = 0;
    public StatsCollection Stats = new StatsCollection();
    public NCRouteType RouteType = NCRouteType.Return;
    public void Init()
    {
        Stats.Add(new StatData(StatType.Infra_LoadPerPacket, loadPerPacket));
        Stats.Add(new StatData(StatType.Infra_PacketCost, cost));
    }
}