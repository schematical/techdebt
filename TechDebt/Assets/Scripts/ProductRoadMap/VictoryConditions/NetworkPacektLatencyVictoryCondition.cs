
using System;
using System.Collections.Generic;
using Stats;
using UI;
using UnityEngine;
using Random = UnityEngine.Random;


public class NetworkPacketLatencyVictoryCondition: MapLevelVictoryConditionBase
{
    public StatsCollection Stats = new StatsCollection();

    public static float GetAvgLatency()
    {
        // float packetsFailed = GameManager.Instance.GetStatValue(StatType.PacketsFailed);
        float packetsServiced = GameManager.Instance.GetStatValue(StatType.PacketsSucceeded);
        if (packetsServiced == 0)
        {
            return 0;
        }
        float totalLatency = GameManager.Instance.GetStatValue(StatType.TotalNetworkPacketLatency);
        float avgLatency = totalLatency / (packetsServiced); // + packetsFailed);
        return avgLatency;
    }
    public NetworkPacketLatencyVictoryCondition()
    {
        Stats.Add(new StatData(StatType.VictoryCondition_NetworkPacketLatency, 10));
    }

    public override VictoryConditionState GetState()
    {
        float avgLatency = GetAvgLatency();

        if (avgLatency > Stats.GetStatValue(StatType.VictoryCondition_NetworkPacketLatency))
        {
            return VictoryConditionState.Failed;
        }
        return VictoryConditionState.Succeeded;
    }
    

    public override string GetDescription()
    {
        return $"Avg Latency Less Than {Stats.GetStatValue(StatType.VictoryCondition_NetworkPacketLatency) * 100}ms";
    }
    public override void Render(UIVictoryConditionListPanel victoryConditionListPanel)
    {
        UIPanelLineProgressBar line = victoryConditionListPanel.AddLine<UIPanelLineProgressBar>();
        line.SetPreText("Latency: ");
        line.OnGetProgress = () =>
        {
              float avgLatency = GetAvgLatency();
              return avgLatency / Stats.GetStatValue(StatType.VictoryCondition_NetworkPacketLatency);
        };
    }
   
}