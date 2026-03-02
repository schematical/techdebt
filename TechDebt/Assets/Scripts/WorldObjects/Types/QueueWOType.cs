using Infrastructure;
using System.Collections.Generic;
using UnityEngine;

public class QueueWOType : WorldObjectType
{
    public QueueWOType()
    {
        type = WorldObjectType.Type.Queue;
        DisplayName = "Queue";
        PrefabId = "SQS";
        BuildTime = 30;
        DailyCost = 30;
        CanBeUpsized = true;
        UnlockConditions = new List<UnlockCondition>()
        {
            new UnlockCondition()
            {
                Type = UnlockCondition.ConditionType.Technology,
                TechnologyID = "sqs"
            }
        };
    }
}
