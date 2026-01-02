using Stats;
using UnityEngine;

namespace Events
{
    public class DDoSEvent : EventBase
    {
        int originalProbability;
        public DDoSEvent()
        {
            EventStartText = "Someone from customer service is about slow service";
            EventEndText = "DDoS Attack seems to have ended"; // Or perhaps an actual result
            Probility = 1;
        }
        
        public override void Apply()
        {
            base.Apply();
            NetworkPacketData networkPacketData = GameManager.Instance.NetworkPacketDatas.Find(data =>
            {
                return data.Type == NetworkPacketData.PType.MaliciousText;
            });
            if (networkPacketData == null)
            {
                Debug.LogError("No NetworkPacketData found for `NetworkPacketData.PType.MaliciousText`");
                return;
            }

            originalProbability = (int)networkPacketData.probilitly;
            networkPacketData.probilitly = 50;
        }

        public override void End()
        {
            base.End();
            NetworkPacketData networkPacketData = GameManager.Instance.NetworkPacketDatas.Find(data =>
            {
                return data.Type == NetworkPacketData.PType.MaliciousText;
            });
            if (networkPacketData == null)
            {
                Debug.LogError("No NetworkPacketData found for `NetworkPacketData.PType.MaliciousText`");
                return;
            }

            networkPacketData.probilitly = originalProbability;
        }

        public override bool IsPossible()
        {
            NetworkPacketData networkPacketDatas = GameManager.Instance.NetworkPacketDatas.Find(data =>
            {
                return data.Type == NetworkPacketData.PType.MaliciousText;
            });
            if (networkPacketDatas == null)
            {
                Debug.LogError("No NetworkPacketData found for `NetworkPacketData.PType.MaliciousText`");
                return false;
            }

            return (GameManager.Instance.GameLoopManager.currentDay > 6);
        }
    }
}