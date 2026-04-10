
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

        if (FailIfNotMet)
        {
            return VictoryConditionState.Failed;
        }
        else
        {
            return VictoryConditionState.NotMet;
        }

    }

    public override string GetDescription()
    {
        return $"Money > {Requirement} : {GetState()}";
    }

    public override void Render(UIVictoryConditionListPanel victoryConditionListPanel)
    {
        UIPanelLine line = victoryConditionListPanel.AddLine<UIPanelLine>();
        line.Add<UIPanelLineSectionText>().text.text =
            $"Remaining Budget: ${GameManager.Instance.GetStatValue(StatType.Money)}";
    }
}