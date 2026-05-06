using System;
using System.Collections.Generic;
using UI;
using UnityEngine;
using Random = UnityEngine.Random;


public class InfraActiveVictoryCondition : MapLevelVictoryConditionBase
{
    public string TargetId;

    public InfraActiveVictoryCondition()
    {
        FailIfNotMet = true;
    }

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
        InfrastructureInstance infrastructureInstance = GameManager.Instance.GetInfrastructureInstanceByID(TargetId);
        return $"Build {infrastructureInstance.GetWorldObjectType().DisplayName}";
    }
    public override void Render(UIVictoryConditionListPanel victoryConditionListPanel)
    {

        Color color = GetColor();
        UIPanelLine line = victoryConditionListPanel.AddLine<UIPanelLine>();
        UIPanelLineSectionText description = line.Add<UIPanelLineSectionText>();
        description.text.text = GetDescription();
        description.text.color = color;
        UIPanelLineSectionText stateText = line.Add<UIPanelLineSectionText>();
        stateText.text.text = $"{GetState()}"; // /{GetFinalState()}";
        stateText.text.color = color;
       
    }
}