using System;

namespace DefaultNamespace.Rewards
{
    public class WorldObjectStartsOperationalReward: RewardBase
    {
        public string WorldObjectId { get; set; }
        public override void Apply()
        {
            foreach (InfrastructureData infrastructureData in GameManager.Instance.AllInfrastructure)
            {
                           
                if (infrastructureData.Id == WorldObjectId)
                {
                    infrastructureData.InitialState = InfrastructureData.State.Operational;
                    infrastructureData.CurrentState = InfrastructureData.State.Operational;
                    // Debug.Log($"- Unlocking: {infrastructureData.Id} - {infrastructureData.CurrentState}");
                }
            }
        }
    }
}