using Infrastructure;
using System.Collections.Generic;
using Tutorial;
using UnityEngine;

public class KeyValueStoreWOType : WorldObjectType
{
    public KeyValueStoreWOType()
    {
        type = WorldObjectType.Type.KeyValueStore;
        DisplayName = "Key Value Store";
        PrefabId = "KeyValueStore";
        BuildTime = 30;
        DailyCost = 30;
        CanBeUpsized = true;
        ShowInGlobalDisplay = true;
        LoadRecoveryRate = 40;
        TutorialStepId = TutorialStepId.Infra_KeyValueStore_Tip;
        sizeTechnologyPrefix = "key-value-store";
        UnlockConditions = new List<UnlockCondition>()
        {
            new UnlockCondition()
            {
                Type = UnlockCondition.ConditionType.Technology,
                TargetId = "key-value-store"
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
