using System;
using System.Collections.Generic;
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
    /*public static InfraActiveVictoryCondition GetRandomCondition(int stage)
    {
        InfraActiveVictoryCondition condition = null;
        int saftyCheck = 0;
        while (condition == null && saftyCheck < 10)
        {
            saftyCheck += 1;

            List<string> infraIds = new List<string>()
            {
                "email-service",
                "sns"
            };
            condition = new InfraActiveVictoryCondition();
            int i = Random.Range(0, infraIds.Count);
            condition.TargetId = infraIds[i];
            if (condition.GetState() == VictoryConditionState.NotMet)
            {
                return condition;
            }
        }

        throw new SystemException("Could not find a victory condition that was not met");

    }*/
}