using UnityEngine;
using System;
using System.Collections.Generic;
using Infrastructure;
using Stats;
using UnityEngine.Serialization;

[Serializable]
public class InfrastructureData
{
    public enum State { Locked, Unlocked, Planned, Operational, Frozen }
    
    [FormerlySerializedAs("ID")] public string Id = Guid.NewGuid().ToString(); // Unique identifier for this instance
    public WorldObjectType.Type worldObjectType;
    public GameObject Prefab;
    public Vector3Int GridPosition;
    public State InitialState = State.Locked;
    public State CurrentState = State.Locked;
    public List<UnlockCondition> UnlockConditions;
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