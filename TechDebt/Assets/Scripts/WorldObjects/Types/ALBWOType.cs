using Infrastructure;
using System.Collections.Generic;
using UnityEngine;

public class ALBWOType : WorldObjectType
{
    public ALBWOType()
    {
        type = WorldObjectType.Type.ALB;
        DisplayName = "Load Balancer";
        PrefabId = "ServerALB";
        BuildTime = 30;
        DailyCost = 30;
        CanBeUpsized = false;
        ShowInGlobalDisplay = true;
        UnlockConditions = new List<UnlockCondition>()
        {
            new UnlockCondition()
            {
                Type = UnlockCondition.ConditionType.Technology,
                TechnologyID = "load-balancer"
            }
        };
        NetworkConnections = new List<NetworkConnection>()
        {
            new NetworkConnection()
            {
                worldObjectType = WorldObjectType.Type.ApplicationServer,
                networkPacketType = NetworkPacketData.PType.Text
            },
            new NetworkConnection()
            {
                worldObjectType = WorldObjectType.Type.ApplicationServer,
                networkPacketType = NetworkPacketData.PType.PII
            },
            new NetworkConnection()
            {
                worldObjectType = WorldObjectType.Type.ApplicationServer,
                networkPacketType = NetworkPacketData.PType.MaliciousText
            },
            new NetworkConnection()
            {
                worldObjectType = WorldObjectType.Type.ApplicationServer,
                networkPacketType = NetworkPacketData.PType.Purchase
            },
        };
    }
}
