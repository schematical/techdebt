using System;
using System.Collections.Generic;
using UI;
using Random = UnityEngine.Random;


public class InfraActiveVictoryCondition : MapLevelVictoryConditionBase
{
    public string TargetId;


    public override VictoryConditionState GetState()
    {
        InfrastructureInstance infrastructureInstance = GameManager.Instance.GetInfrastructureInstanceByID(TargetId);
        if (infrastructureInstance.IsActive())
        {
            return VictoryConditionState.Succeeded;
        }

        return VictoryConditionState.NotMet;
    }
    public override string GetDescription()
    {
        return $"{TargetId} Active : {GetState()}";
    }
    public override void Render(UIVictoryConditionListPanel victoryConditionListPanel)
    {
        UIPanelLine line = victoryConditionListPanel.AddLine<UIPanelLine>();
        line.Add<UIPanelLineSectionText>().text.text =
            GetDescription();
    }
}