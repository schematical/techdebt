using System;
using System.Collections.Generic;
using DefaultNamespace.Rewards;
using UI;
using UnityEngine.Events;
using Random = UnityEngine.Random;


public class SpecialReleaseVictoryCondition : MapLevelVictoryConditionBase
{
    public bool HasReleaseBeedDeployed { get; protected set; }= false;
    protected SpecialCallbackReward reward;
    public UnityAction onComplete;
    public SpecialReleaseVictoryCondition(string name, string description, string iconSpriteId, UnityAction? onComplete = null)
    {
        reward = new SpecialCallbackReward(OnSpecialReleaseComplete)
        {
            Name = name,
            Description = description,
            IconSpriteId = iconSpriteId
        };
        if (onComplete != null)
        {
            this.onComplete = onComplete;
        }
        FailIfNotMet = true;
        
    }

    public SpecialCallbackReward GetReward()
    {
        return reward;
    }
    private void OnSpecialReleaseComplete()
    {
        HasReleaseBeedDeployed = true;
        if (onComplete != null)
        {
            onComplete.Invoke();
        }
    }
    public override VictoryConditionState GetState()
    {
        if (HasReleaseBeedDeployed)
        {
            return VictoryConditionState.Succeeded;
        }

        return VictoryConditionState.NotMet;
    }
    public override string GetDescription()
    {
        return $"Deploy {reward.Name}";
    }
    public override void Render(UIVictoryConditionListPanel victoryConditionListPanel)
    {
        UIPanelLine line = victoryConditionListPanel.AddLine<UIPanelLine>();
        line.Add<UIPanelLineSectionText>().text.text =
            GetDescription();
    }
}