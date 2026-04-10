using System;
using System.Collections.Generic;
using UI;
using Random = UnityEngine.Random;


public class UpTimeVictoryCondition : MapLevelVictoryConditionBase
{

    protected float UpTimeRequirement = .5f;

    public override VictoryConditionState GetState()
    {
        
        if (GetPacketFailedPercent() > UpTimeRequirement)
        {
            return VictoryConditionState.Failed;
        }

        return VictoryConditionState.NotMet;
    }
    public override string GetDescription()
    {
        return $" Active : {GetState()}";
    }

    public override void Render(UIVictoryConditionListPanel victoryConditionListPanel)
    {
        UIPanelLineProgressBar line = victoryConditionListPanel.AddLine<UIPanelLineProgressBar>();
        line.SetPreText("Failed: ");
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