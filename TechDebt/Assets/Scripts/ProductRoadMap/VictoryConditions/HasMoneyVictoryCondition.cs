
using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;


public class HasMoneyVictoryCondition: MapLevelVictoryConditionBase
{
    public int Requirement = 0;
    public bool FailIfNotMet = true;


    public override VictoryConditionState GetState()
    {


        if (GameManager.Instance.GetStat(StatType.Money) > Requirement)
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

   
}