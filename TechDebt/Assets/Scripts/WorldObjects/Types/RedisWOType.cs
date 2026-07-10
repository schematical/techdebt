using Infrastructure;
using System.Collections.Generic;
using Tutorial;
using UnityEngine;

public class RedisWOType : WorldObjectType
{
    public RedisWOType()
    {
        type = WorldObjectType.Type.Redis;
        DisplayName = "Key Value Store";
        PrefabId = "Redis";
        BuildTime = 30;
        DailyCost = 30;
        CanBeUpsized = true;
        ShowInGlobalDisplay = true;
        LoadRecoveryRate = 40;
        TutorialStepId = TutorialStepId.Infra_Redis_Tip;
        sizeTechnologyPrefix = "redis";
        UnlockConditions = new List<UnlockCondition>()
        {
            new UnlockCondition()
            {
                Type = UnlockCondition.ConditionType.Technology,
                TargetId = "redis"
            }
        };
        networkPackets = new List<InfrastructureDataNetworkPacket>()
        {
            new InfrastructureDataNetworkPacket()
            {
                PacketType =  NetworkPacketData.PType.Text,
                loadPerPacket = 10
            },
            new InfrastructureDataNetworkPacket()
            {
                PacketType =  NetworkPacketData.PType.MaliciousText,
                loadPerPacket = 20
            },
            new InfrastructureDataNetworkPacket()
            {
                PacketType =  NetworkPacketData.PType.Image,
                loadPerPacket = 100
            },
            new InfrastructureDataNetworkPacket()
            {
                PacketType =  NetworkPacketData.PType.PII,
                loadPerPacket = 10
            },
            new InfrastructureDataNetworkPacket()
            {
                PacketType =  NetworkPacketData.PType.Purchase,
                loadPerPacket = 5
            }
        };
    }
}
