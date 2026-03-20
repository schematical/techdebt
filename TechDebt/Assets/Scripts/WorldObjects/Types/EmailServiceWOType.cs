using Infrastructure;
using System.Collections.Generic;
using Tutorial;
using UnityEngine;

public class EmailServiceWOType : WorldObjectType
{
    public EmailServiceWOType()
    {
        type = WorldObjectType.Type.EmailService;
        DisplayName = "Email Service";
        PrefabId = "EmailService";
        BuildTime = 30;
        DailyCost = 30;
        CanBeUpsized = false;
        ShowInGlobalDisplay = true;
        TutorialStepId = TutorialStepId.Infra_EmailService_Tip;
        UnlockConditions = new List<UnlockCondition>()
        {
            new UnlockCondition()
            {
                Type = UnlockCondition.ConditionType.Technology,
                TechnologyID = "email-service"
            }
        };
    }
}
