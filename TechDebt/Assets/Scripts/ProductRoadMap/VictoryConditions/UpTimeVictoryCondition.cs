using System;
using System.Collections.Generic;
using UI;
using Random = UnityEngine.Random;


public class UpTimeVictoryCondition : MapLevelVictoryConditionBase
{

    protected float UpTimeRequirement = .5f;

    public UpTimeVictoryCondition(float requirement = .5f)
    {
        UpTimeRequirement = requirement;
    }
    public override VictoryConditionState GetState()
    {
        
        if (GetPacketFailedPercent() > UpTimeRequirement)
        {
            return VictoryConditionState.Failed;
        }

        return VictoryConditionState.Succeeded;
    }
    public override string GetDescription()
    {
        return $" Successfully serve {Math.Round(UpTimeRequirement * 100)}% of the NetworkPackets";
    }

    public override void Render(UIVictoryConditionListPanel victoryConditionListPanel)
    {
        UIPanelLineProgressBar line = victoryConditionListPanel.AddLine<UIPanelLineProgressBar>();
        line.SetPreText("Failed Packets: ");
        line.OnGetProgress = () =>
        {
            return GetPacketFailedPercent() / UpTimeRequirement;
        };
    }

    public float GetPacketFailedPercent()
    {
        if (GameManager.Instance.GetStatValue(StatType.PacketsSent) == 0)
        {
            return 0;
        }
        return GameManager.Instance.GetStatValue(StatType.PacketsFailed) /
            GameManager.Instance.GetStatValue(StatType.PacketsSent);
    }

   
}