using Infrastructure;
using System.Collections.Generic;
using Tutorial;
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
        TutorialStepId = TutorialStepId.Infra_SQS_Tip;
        UnlockConditions = new List<UnlockCondition>()
        {
            new UnlockCondition()
            {
                Type = UnlockCondition.ConditionType.Technology,
                TargetId = "sqs"
            }
        };
    }
}
