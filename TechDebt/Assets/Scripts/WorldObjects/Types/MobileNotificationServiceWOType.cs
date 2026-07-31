using Infrastructure;
using System.Collections.Generic;
using Tutorial;
using UnityEngine;

public class MobileNotificationServiceWOType : WorldObjectType
{
    public MobileNotificationServiceWOType()
    {
        type = WorldObjectType.Type.MobileNotificationService;
        DisplayName = "Mobile Notifications";
        PrefabId = "MobileNotificationService";
        BuildTime = 30;
        DailyCost = 30;
        CanBeUpsized = false;
        ShowInGlobalDisplay = true;
        TutorialStepId = TutorialStepId.Infra_MobileNotificationService_Tip;
        UnlockConditions = new List<UnlockCondition>()
        {
            new UnlockCondition()
            {
                Type = UnlockCondition.ConditionType.Technology,
                TargetId = "mobile-notifications"
            }
        };
    }
}
