using System;
using UnityEngine;

namespace DefaultNamespace.NetworkPackets
{
    public class CoinNetworkPacket: NetworkPacket
    {
        public bool ending = false;
        public override void Initialize(NetworkPacketData npData, string fileName, int size,
            InfrastructureInstance origin = null)
        {
            base.Initialize(npData, fileName, size, origin);
            ending = false;
        }
        protected override void Update()
        {
            
            if (ending)
            {
                transform.position = new Vector3(
                    transform.position.x, 
                    transform.position.y + (40 * Time.unscaledDeltaTime), 
                    transform.position.z
                );
                Vector3 bottomCornerScreenPos = Camera.main.WorldToScreenPoint(transform.position);

                if (bottomCornerScreenPos.y > Screen.height + 100)
                {
                    GameManager.Instance.UIManager.moneyPanel.AddCoin();
                    GameManager.Instance.DestroyPacket(this);
                }
          
            }
            base.Update();
        }

        public override void StartReturn()
        {
            if (ending)
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
            ending = true;
   
        }
    }
}