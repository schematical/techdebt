using System;
using UnityEngine;

namespace DefaultNamespace.NetworkPackets
{
    public class CoinNetworkPacket: NetworkPacket
    {
        
        /*public override void Initialize(NetworkPacketData npData, string fileName, int size,
            InfrastructureInstance origin = null)
        {
            base.Initialize(npData, fileName, size, origin);
           
        }*/
        public override void StartReturn()
        {
            if (IsReturning())
            {
                return;
            }
            float probTotal = 0f;
            foreach (NetworkPacketData _npData in GameManager.Instance.NetworkPacketDatas)
            {
                // Debug.Log($"{npData.Type} - Prob: {npData.probilitly} Total Before: {probTotal}");
                probTotal += _npData.probilitly;
            }

            float percentageOfTotalTraffic = data.probilitly / probTotal;
            float estimatedPacketsSentToday = percentageOfTotalTraffic * GameManager.Instance.GetStat(StatType.Traffic);
            int saleValue = (int)Math.Round(GameManager.Instance.GetStat(StatType.DailyIncome) / estimatedPacketsSentToday);
            GameManager.Instance.FloatingTextFactory.ShowText($"+${saleValue}",
                    transform.position, Color.green);
            GameManager.Instance.IncrStat(StatType.Money, saleValue);
            GameManager.Instance.DestroyPacket(this);
        }
    }
}