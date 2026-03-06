using Infrastructure;
using System.Collections.Generic;
using UnityEngine;

public class WorkerServerWOType : WorldObjectType
{
    public WorkerServerWOType()
    {
        type = WorldObjectType.Type.WorkerServer;
        DisplayName = "Background Worker";
        PrefabId = "ServerWorker";
        BuildTime = 30;
        DailyCost = 30;
        CanBeUpsized = true;
        ShowInGlobalDisplay = true;
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
