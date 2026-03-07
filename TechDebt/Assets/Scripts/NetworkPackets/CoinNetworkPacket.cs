using System;
using UnityEngine;
using Random = UnityEngine.Random;


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
        protected override void FixedUpdate()
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
            base.FixedUpdate();
        }

        public override void StartReturn()
        {
            if (ending)
            {
                return;
            }
            int saleValue = (int) Random.Range(
                data.Stats.GetStatValue(StatType.NetworkPacket_ValueMin),
                data.Stats.GetStatValue(StatType.NetworkPacket_ValueMax)
            );
            GameManager.Instance.FloatingTextFactory.ShowText($"+${saleValue}",
                    transform.position, Color.green);
            GameManager.Instance.IncrStat(StatType.Money, saleValue);
            GameManager.Instance.GameLoopManager.dailyPacketIncome += saleValue;
            Debug.Log($"{data.Type} - setValue: {saleValue}");
            ending = true;
   
        }
    }
}