using System;
using System.Collections.Generic;
using UI;
using UnityEngine;
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
        UIPanelLineSectionText textSection = victoryConditionListPanel.AddLine<UIPanelLine>().Add<UIPanelLineSectionText>();
        textSection.text.text =
            $"Keep Failed Packets less than {Math.Round(UpTimeRequirement * 100)}%";
        UIPanelLineProgressBar line = victoryConditionListPanel.AddLine<UIPanelLineProgressBar>();
        line.SetPreText("Failed Packets: ");
        line.OnGetProgress = () =>
        {
            float progress = GetPacketFailedPercent();
            Color color = Color.white;
            if (progress > UpTimeRequirement)
            {
                color = Color.red;
            }else if (progress > .75 * UpTimeRequirement)
            {
                color = Color.orangeRed;
            }
            line.SetColor(color);
            textSection.text.color = color;
            return progress;
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