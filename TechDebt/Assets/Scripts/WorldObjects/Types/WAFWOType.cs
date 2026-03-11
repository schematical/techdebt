using Infrastructure;
using System.Collections.Generic;
using UnityEngine;

public class WAFWOType : WorldObjectType
{
    public WAFWOType()
    {
        type = WorldObjectType.Type.Queue;
        DisplayName = "Firewall";
        PrefabId = "WAF";
        BuildTime = 30;
        DailyCost = 30;
        CanBeUpsized = true;
        ShowInGlobalDisplay = true;
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
