using System;
using Infrastructure;

namespace DefaultNamespace.Rewards
{
    public class WorldObjectTypeStartsOperationalReward: RewardBase
    {
        public string WorldObjectTypeId { get; set; }
        public override void Apply()
        {
            foreach (WorldObjectBase infrastructureData in GameManager.Instance.AllWorldObjects)
            {
                WorldObjectType worldObjectType = GameManager.Instance.WorldObjectTypes[infrastructureData.Type];
                if (worldObjectType.GetTypeAsId() == WorldObjectTypeId)
                {
                    infrastructureData.InitialState = WorldObjectBase.State.Operational;
                    infrastructureData.CurrentState = WorldObjectBase.State.Operational;
                    // Debug.Log($"- Unlocking: {infrastructureData.Id} - {infrastructureData.CurrentState}");
                }
            }
        }
    }
}