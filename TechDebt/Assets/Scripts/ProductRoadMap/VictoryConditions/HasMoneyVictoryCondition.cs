
using System;
using System.Collections.Generic;
using UI;
using Random = UnityEngine.Random;


public class HasMoneyVictoryCondition: MapLevelVictoryConditionBase
{
    public int Requirement = 0;


    public override VictoryConditionState GetState()
    {
        if (GameManager.Instance.GetStatValue(StatType.Money) > Requirement)
        {
            return VictoryConditionState.Succeeded;
        }
        return VictoryConditionState.Failed;
    }

    public override string GetDescription()
    {
        return $"Have more than ${Requirement} in your budget";
    }

    public override void Render(UIVictoryConditionListPanel victoryConditionListPanel)
    {
        UIPanelLine line = victoryConditionListPanel.AddLine<UIPanelLine>();
        UIPanelLineSectionText lineText = line.Add<UIPanelLineSectionText>();
        lineText.OnFixedUpdate((lineSection) =>
        {
            (lineSection as UIPanelLineSectionText).text.text =
                $"Remaining Budget: ${GameManager.Instance.GetStatValue(StatType.Money)}";
        });
    }
}