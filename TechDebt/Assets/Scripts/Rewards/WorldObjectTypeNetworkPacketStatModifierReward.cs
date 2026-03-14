using System;
using Infrastructure;
using NPCs;
using Stats;
using UnityEngine;

namespace DefaultNamespace.Rewards
{
    public class WorldObjectTypeNetworkPacketStatModifierReward: StatModifierReward
    {
        public WorldObjectType.Type WorldObjectType;
        public NetworkPacketData.PType NetworkPacketType;
        public override iModifiable GetTarget()
        {
            int hitCheck = 0;
            foreach (InfrastructureDataNetworkPacket networkPacketData in  GameManager.Instance.WorldObjectTypes[WorldObjectType].networkPackets)
            {
                if (networkPacketData.PacketType == NetworkPacketType)
                {
                    return networkPacketData;
                    /*hitCheck += 1;
                    if (hitCheck > 1)
                    {
                        Debug.LogError($"Hit more than once. Something is wrong. {this.WorldObjectType} + {NetworkPacketType} - {StatType}");
                    }*/
                }
            }
            Debug.LogError($"Could not find a network packet for Something is wrong. {this.WorldObjectType} + {NetworkPacketType} - {StatType}");
            /*if (hitCheck == 0)
            {
                Debug.LogError($"Could not find a network packet for Something is wrong. {this.WorldObjectType} + {NetworkPacketType} - {StatType}");
            }*/
            return null;
        }

       
    }
}