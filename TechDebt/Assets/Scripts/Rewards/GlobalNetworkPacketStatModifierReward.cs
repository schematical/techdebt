using System;
using NPCs;
using Stats;

namespace DefaultNamespace.Rewards
{
    public class GlobalNetworkPacketStatModifierReward: StatModifierReward
    {
        public NetworkPacketData.PType NetworkPacketType;
        public override iModifiable GetTarget()
        {
            NetworkPacketData networkPacketData2 = GameManager.Instance.GetNetworkPacketDatas()
                .Find((data => data.Type == NetworkPacketType));
            if (networkPacketData2 == null)
            {
                throw new SystemException($"Could not find {NetworkPacketType}");
            }
            return networkPacketData2;
        }
    }
}