using Infrastructure;
using System.Collections.Generic;
using Tutorial;
using UnityEngine;

public class BinaryStorageWOType : WorldObjectType
{
    public BinaryStorageWOType()
    {
        type = WorldObjectType.Type.BinaryStorage;
        DisplayName = "Binary Storage";
        PrefabId = "S3Bucket";
        BuildTime = 30;
        DailyCost = 30;
        CanBeUpsized = true;
        ShowInGlobalDisplay = true;
        TutorialStepId = TutorialStepId.Infra_BinaryStorage_Tip;
        UnlockConditions = new List<UnlockCondition>()
        {
            new UnlockCondition()
            {
                Type = UnlockCondition.ConditionType.Technology,
                TechnologyID = "binary-storage"
            }
        };
    }
}
