using Infrastructure;
using System.Collections.Generic;
using Tutorial;
using UnityEngine;

public class RedisWOType : WorldObjectType
{
    public RedisWOType()
    {
        type = WorldObjectType.Type.Redis;
        DisplayName = "Key Value Store";
        PrefabId = "Redis";
        BuildTime = 30;
        DailyCost = 30;
        CanBeUpsized = true;
        ShowInGlobalDisplay = true;
        TutorialStepId = TutorialStepId.Infra_Redis_Tip;
        UnlockConditions = new List<UnlockCondition>()
        {
            new UnlockCondition()
            {
                Type = UnlockCondition.ConditionType.Technology,
                TechnologyID = "redis"
            }
        };
    }
}
