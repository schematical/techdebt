// NetworkPacketData.cs
using System;
using Stats;
using UnityEngine;
[Serializable]
public class NetworkPacketData: iModifiable
{
    [Serializable]
    public enum PType
    {
        Text,
        Image,
        BatchJob,
        MaliciousText,
        Purchase,
        PII,
        SQLInjection
    }
    // public NetworkPacketData(float probability)
    public StatsCollection Stats { get; } = new StatsCollection();
    public PType Type;
    public string prefabId;
    public float baseLoad = 20f;
    

    public NetworkPacketData(float probibility)
    {
        Stats.Add(new StatData(StatType.NetworkPacket_Probibility, probibility));
        Stats.Add(new StatData(StatType.NetworkPacket_LoadLatencyMultiplier, 10));
    }


    public float GetProbability()
    {
        return Stats.GetStatValue(StatType.NetworkPacket_Probibility);
    }
    // public int incomePerPacket = 0;
}