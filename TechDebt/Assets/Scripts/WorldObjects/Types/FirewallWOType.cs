using Infrastructure;
using System.Collections.Generic;
using Tutorial;
using UnityEngine;

public class FirewallWOType : WorldObjectType
{
    public FirewallWOType()
    {
        type = WorldObjectType.Type.Firewall;
        DisplayName = "Firewall";
        PrefabId = "Firewall";
        BuildTime = 30;
        DailyCost = 30;
        CanBeUpsized = true;
        ShowInGlobalDisplay = true;
        TutorialStepId = TutorialStepId.Infra_WAF_Tip;
        UnlockConditions = new List<UnlockCondition>()
        {
            new UnlockCondition()
            {
                Type = UnlockCondition.ConditionType.Technology,
                TargetId = "firewall"
            }
        };
    }
}
