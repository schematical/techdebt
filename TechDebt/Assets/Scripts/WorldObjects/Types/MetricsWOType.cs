using Infrastructure;
using System.Collections.Generic;
using Tutorial;
using UnityEngine;

public class MetricsWOType : WorldObjectType
{
    public MetricsWOType()
    {
        type = WorldObjectType.Type.Metrics;
        DisplayName = "Metrics";
        PrefabId = "Metrics";
        BuildTime = 30;
        DailyCost = 1;
        CanBeUpsized = false;
        ShowInGlobalDisplay = true;
        TutorialStepId = TutorialStepId.Infra_CloudWatchMetrics_Tip;
        UnlockConditions = new List<UnlockCondition>()
        {
            new UnlockCondition()
            {
                Type = UnlockCondition.ConditionType.Technology,
                TargetId = "metrics"
            }
        };
    }
}
