using System;
using System.Collections.Generic;
using UI;
using Random = UnityEngine.Random;


public class TechnologyResearchVictoryCondition : MapLevelVictoryConditionBase
{
    public string TechnologyId;

    public TechnologyResearchVictoryCondition()
    {
        FailIfNotMet = true;
    }

    public override VictoryConditionState GetState()
    {
        Technology technology = GameManager.Instance.GetTechnologyByID(TechnologyId);
        if (technology.IsUnlocked())
        {
            return VictoryConditionState.Succeeded;
        }

        return VictoryConditionState.NotMet;
    }
    public override string GetDescription()
    {
        return $"Research {TechnologyId}";
    }
    public override void Render(UIVictoryConditionListPanel victoryConditionListPanel)
    {
        UIPanelLine line = victoryConditionListPanel.AddLine<UIPanelLine>();
        line.Add<UIPanelLineSectionText>().text.text =
            GetDescription();
    }
}