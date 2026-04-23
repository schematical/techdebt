using Infrastructure;
using System.Collections.Generic;
using Tutorial;
using UnityEngine;

public class SecretManagerWOType : WorldObjectType
{
    public SecretManagerWOType()
    {
        type = WorldObjectType.Type.SecretManager;
        DisplayName = "Secret Manager";
        PrefabId = "SecretManager";
        BuildTime = 30;
        DailyCost = 30;
        CanBeUpsized = false;
        ShowInGlobalDisplay = true;
        TutorialStepId = TutorialStepId.Infra_SecretManager_Tip;
        UnlockConditions = new List<UnlockCondition>()
        {
            new UnlockCondition()
            {
                Type = UnlockCondition.ConditionType.Technology,
                TargetId = "secret-manager"
            }
        };
    }
}
