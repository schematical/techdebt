using System;
using Infrastructure;

namespace DefaultNamespace.Rewards
{
    public class WorldObjectStartsOperationalReward: RewardBase
    {
        public string WorldObjectId { get; set; }
        public override void Apply()
        {
            foreach (WorldObjectBase infrastructureData in GameManager.Instance.AllWorldObjects)
            {
                           
                if (infrastructureData.Id == WorldObjectId)
                {
                    infrastructureData.InitialState = WorldObjectBase.State.Operational;
                    infrastructureData.CurrentState = WorldObjectBase.State.Operational;
                    // Debug.Log($"- Unlocking: {infrastructureData.Id} - {infrastructureData.CurrentState}");
                }
            }
        }
    }
}