using System;
using Infrastructure;

namespace DefaultNamespace.Rewards
{
    public class WorldObjectTypeStartsOperationalReward: RewardBase
    {
        public string WorldObjectTypeId { get; set; }
        public override void Apply()
        {
            foreach (InfrastructureData infrastructureData in GameManager.Instance.AllInfrastructure)
            {
                WorldObjectType worldObjectType = GameManager.Instance.WorldObjectTypes[infrastructureData.worldObjectType];
                if (worldObjectType.GetTypeAsId() == WorldObjectTypeId)
                {
                    infrastructureData.InitialState = InfrastructureData.State.Operational;
                    infrastructureData.CurrentState = InfrastructureData.State.Operational;
                    // Debug.Log($"- Unlocking: {infrastructureData.Id} - {infrastructureData.CurrentState}");
                }
            }
        }
    }
}