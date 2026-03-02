using Infrastructure;
using System.Collections.Generic;
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
