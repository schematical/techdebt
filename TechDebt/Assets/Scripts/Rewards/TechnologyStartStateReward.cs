using System;

namespace DefaultNamespace.Rewards
{
    public class TechnologyStartStateReward: RewardBase
    {
        public string TechnologyId { get; set; }
        public Technology.State StartState = Technology.State.Locked;
        public override void Apply()
        {
           Technology technology = GameManager.Instance.GetAllTechnologies().Find((t => t.TechnologyID == TechnologyId));
            if (technology == null)
            {
                throw new SystemException($"Technology_Locked '{TechnologyId}' is null.");
            }

            if (technology.CurrentState == Technology.State.MetaLocked || technology.CurrentState == Technology.State.Locked)
            {
                technology.CurrentState = StartState;
            }
        }
    }
}