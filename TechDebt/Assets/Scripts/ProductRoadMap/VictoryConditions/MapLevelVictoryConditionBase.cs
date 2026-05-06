
using System;
using System.Collections.Generic;
using UI;
using UnityEngine;
using Random = UnityEngine.Random;


public abstract class MapLevelVictoryConditionBase
{
    
    public bool FailIfNotMet = false;
    public abstract VictoryConditionState GetState();

    public abstract string GetDescription();
    public bool isGlobal = false;

    public void SetGlobal()
    {
        isGlobal = true;
    }

    public bool IsGlobal()
    {
        return isGlobal;
    }

    public virtual VictoryConditionState GetFinalState()
    {
        VictoryConditionState state = GetState();
       
        switch (state)
        {
            case(VictoryConditionState.NotMet):
                if (FailIfNotMet)
                {
                    return VictoryConditionState.Failed;
                }
                return VictoryConditionState.Succeeded;
                break;
            default:
                return state;
        }
    }

    public Color GetColor()
    {
        Color color = Color.white;
        if (GetFinalState() == VictoryConditionState.Succeeded)
        {
            color = Color.green;
        }
        else
        {
            switch (GameManager.Instance.GameLoopManager.GetDaysLeftInSprint())
            {
                case 0:
                    color = Color.red;
                    break;
                case 1:
                    color = Color.darkOrange;
                    break;
            }
        }

        return color;
    }
    public abstract void Render(UIVictoryConditionListPanel victoryConditionListPanel);
}