using Infrastructure;
using System.Collections.Generic;
using UnityEngine;

public class DedicatedDBWOType : WorldObjectType
{
    public DedicatedDBWOType()
    {
        type = WorldObjectType.Type.DedicadedDB;
        DisplayName = "Database";
        PrefabId = "DedicatedDB";
        BuildTime = 30;
        DailyCost = 30;
        CanBeUpsized = true;
        LoadRecoveryRate = 10;
        UnlockConditions = new List<UnlockCondition>()
        {
            new UnlockCondition()
            {
                Type = UnlockCondition.ConditionType.Technology,
                TechnologyID = "dedicated-db"
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
                PacketType =  NetworkPacketData.PType.Image,
                loadPerPacket = 100
            },
            new InfrastructureDataNetworkPacket()
            {
                PacketType =  NetworkPacketData.PType.PII,
                loadPerPacket = 10
            },
        };
    }
}
