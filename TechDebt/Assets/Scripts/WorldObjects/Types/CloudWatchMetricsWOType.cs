using Infrastructure;
using System.Collections.Generic;
using Tutorial;
using UnityEngine;

public class CloudWatchMetricsWOType : WorldObjectType
{
    public CloudWatchMetricsWOType()
    {
        type = WorldObjectType.Type.CloudWatchMetrics;
        DisplayName = "Cloud Watch Metrics";
        PrefabId = "CloudWatchMetrics";
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
                TechnologyID = "cloud-watch-metrics"
            }
        };
    }
}
