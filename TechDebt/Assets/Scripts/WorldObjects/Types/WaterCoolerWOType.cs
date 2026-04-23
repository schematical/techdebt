using Infrastructure;
using System.Collections.Generic;
using Tutorial;
using UnityEngine;

public class WaterCoolerWOType : WorldObjectType
{
    public WaterCoolerWOType()
    {
        type = WorldObjectType.Type.WaterCooler;
        DisplayName = "Water Cooler";
        PrefabId = "WaterCooler";
        BuildTime = 30;
        DailyCost = 10;
        CanBeUpsized = false;
        ShowInGlobalDisplay = true;
        TutorialStepId = TutorialStepId.Infra_WaterCooler_Tip;
        UnlockConditions = new List<UnlockCondition>()
        {
            new UnlockCondition()
            {
                Type = UnlockCondition.ConditionType.Technology,
                TargetId = "water-cooler"
            }
        };
    }
}
