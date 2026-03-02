using Infrastructure;
using System.Collections.Generic;
using UnityEngine;

public class ApplicationServerWOType : WorldObjectType
{
    public ApplicationServerWOType()
    {
        type = WorldObjectType.Type.ApplicationServer;
        DisplayName = "Application Server";
        PrefabId = "ServerPrefab";
        BuildTime = 30;
        DailyCost = 30;
        CanBeUpsized = true;
        LoadRecoveryRate = 20;
        UnlockConditions = new List<UnlockCondition>()
        {
            new UnlockCondition()
            {
                Type = UnlockCondition.ConditionType.Technology,
                TechnologyID = "application-server"
            }
        };
        networkPackets = new List<InfrastructureDataNetworkPacket>()
        {
            new InfrastructureDataNetworkPacket()
            {
                PacketType =  NetworkPacketData.PType.Text,
                loadPerPacket = 20
            },
            new InfrastructureDataNetworkPacket()
            {
                PacketType =  NetworkPacketData.PType.Image,
                loadPerPacket = 40
            },
            new InfrastructureDataNetworkPacket()
            {
                PacketType =  NetworkPacketData.PType.PII,
                loadPerPacket = 20
            },
            new InfrastructureDataNetworkPacket()
            {
                PacketType =  NetworkPacketData.PType.Purchase,
                loadPerPacket = 5
            }
        };
        NetworkConnections = new List<NetworkConnection>()
        {
            new NetworkConnection()
            {
                worldObjectType = WorldObjectType.Type.BinaryStorage,
                networkPacketType = NetworkPacketData.PType.Image,
                networkConnectionBonus = new List<NetworkConnectionBonus>()
                {
                    new NetworkConnectionBonus()
                    {
                        value = 0.75f
                    }
                }
            },
            new NetworkConnection()
            {
                worldObjectType = WorldObjectType.Type.DedicadedDB,
                networkPacketType = NetworkPacketData.PType.Text,
                networkConnectionBonus = new List<NetworkConnectionBonus>()
                {
                    new NetworkConnectionBonus()
                    {
                        value = 0.75f
                    }
                }
            },
            new NetworkConnection()
            {
                worldObjectType = WorldObjectType.Type.DedicadedDB,
                networkPacketType = NetworkPacketData.PType.PII,
                networkConnectionBonus = new List<NetworkConnectionBonus>()
                {
                    new NetworkConnectionBonus()
                    {
                        value = 0.75f
                    }
                }
                
            },
            new NetworkConnection()
            {
                worldObjectType = WorldObjectType.Type.DedicadedDB,
                networkPacketType = NetworkPacketData.PType.MaliciousText
            },
            new NetworkConnection()
            {
                worldObjectType = WorldObjectType.Type.Redis,
                networkPacketType = NetworkPacketData.PType.Text,
                networkConnectionBonus = new List<NetworkConnectionBonus>()
                {
                    new NetworkConnectionBonus()
                    {
                        value = 0.75f
                    }
                }
            },
            new NetworkConnection()
            {
                worldObjectType = WorldObjectType.Type.Queue,
                networkPacketType = NetworkPacketData.PType.Text,
                networkConnectionBonus = new List<NetworkConnectionBonus>()
                {
                    new NetworkConnectionBonus()
                    {
                        value = 0.5f
                    }
                }
            },

            
        };
    }
}
