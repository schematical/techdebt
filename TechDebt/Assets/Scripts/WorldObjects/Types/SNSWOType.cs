using Infrastructure;
using System.Collections.Generic;
using Tutorial;
using UnityEngine;

public class SNSWOType : WorldObjectType
{
    public SNSWOType()
    {
        type = WorldObjectType.Type.SNS;
        DisplayName = "Mobile Notifications";
        PrefabId = "SNS";
        BuildTime = 30;
        DailyCost = 30;
        CanBeUpsized = false;
        ShowInGlobalDisplay = true;
        TutorialStepId = TutorialStepId.Infra_SNS_Tip;
        UnlockConditions = new List<UnlockCondition>()
        {
            new UnlockCondition()
            {
                Type = UnlockCondition.ConditionType.Technology,
                TechnologyID = "sns"
            }
        };
    }
}
