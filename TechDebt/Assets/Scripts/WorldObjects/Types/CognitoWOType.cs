using Infrastructure;
using System.Collections.Generic;
using Tutorial;
using UnityEngine;

public class CognitoWOType : WorldObjectType
{
    public CognitoWOType()
    {
        type = WorldObjectType.Type.Cognito;
        DisplayName = "Cognito User Pools";
        PrefabId = "Cognito";
        BuildTime = 30;
        DailyCost = 30;
        CanBeUpsized = false;
        ShowInGlobalDisplay = true;
        TutorialStepId = TutorialStepId.Infra_Cognito_Tip;
        UnlockConditions = new List<UnlockCondition>()
        {
            new UnlockCondition()
            {
                Type = UnlockCondition.ConditionType.Technology,
                TechnologyID = "cognito"
            }
        };
    }
}
