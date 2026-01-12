using Stats;
using UnityEngine;

namespace Events
{
    public class AttackStartEvent : EventBase
    {

        public AttackStartEvent()
        {
            EventStartText = "We seem to be getting some malicious traffic";
            // EventEndText = "Calm seas"; // Or perhaps an actual result
            Probability = 1;
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

            networkPacketData.probilitly = 2;
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

            return (GameManager.Instance.GameLoopManager.currentDay > 3);
        }
    }
}