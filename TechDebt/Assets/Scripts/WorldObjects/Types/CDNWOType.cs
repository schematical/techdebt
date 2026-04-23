using Infrastructure;
using System.Collections.Generic;
using Tutorial;
using UnityEngine;

public class CDNWOType : WorldObjectType
{
    public CDNWOType()
    {
        type = WorldObjectType.Type.CDN;
        DisplayName = "CDN";
        PrefabId = "CloudFront";
        BuildTime = 30;
        DailyCost = 30;
        CanBeUpsized = true;
        LoadRecoveryRate = 10;
        ShowInGlobalDisplay = true;
        TutorialStepId = TutorialStepId.Infra_CDN_Tip;
        UnlockConditions = new List<UnlockCondition>()
        {
            new UnlockCondition()
            {
                Type = UnlockCondition.ConditionType.Technology,
                TargetId = "cdn"
            }
        };
        NetworkConnections = new List<NetworkConnection>()
        {
            new NetworkConnection()
            {
                worldObjectType = WorldObjectType.Type.BinaryStorage,
                networkPacketType = NetworkPacketData.PType.Image,
            },
        };
    }
}
