using Infrastructure;
using System.Collections.Generic;
using Tutorial;
using UnityEngine;

public class WAFWOType : WorldObjectType
{
    public WAFWOType()
    {
        type = WorldObjectType.Type.WAF;
        DisplayName = "Firewall";
        PrefabId = "WAF";
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
                TechnologyID = "waf"
            }
        };
    }
}
