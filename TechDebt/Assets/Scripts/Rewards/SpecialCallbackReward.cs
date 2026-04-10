using UnityEngine.Events;

namespace DefaultNamespace.Rewards
{
    public class SpecialCallbackReward: RewardBase
    {
        protected UnityAction onComplete;

        public SpecialCallbackReward(UnityAction onComplete)
        {
            this.onComplete = onComplete;
        }
        public override void Apply()
        {
            
            onComplete.Invoke();
            
        }
    }
}