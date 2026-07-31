using Infrastructure;
using System.Collections.Generic;
using Tutorial;
using UnityEngine;

public class AuthServiceWOType : WorldObjectType
{
    public AuthServiceWOType()
    {
        type = WorldObjectType.Type.AuthService;
        DisplayName = "Auth Service";
        PrefabId = "AuthService";
        BuildTime = 30;
        DailyCost = 30;
        CanBeUpsized = false;
        ShowInGlobalDisplay = true;
        TutorialStepId = TutorialStepId.Infra_AuthService_Tip;
        UnlockConditions = new List<UnlockCondition>()
        {
            new UnlockCondition()
            {
                Type = UnlockCondition.ConditionType.Technology,
                TargetId = "auth-service"
            }
        };
    }
}
