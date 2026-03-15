using NPCs;
using Stats;

namespace DefaultNamespace.Rewards
{
    public class GlobalStatBaseValueReward: StatBaseValueReward
    {

        public override iModifiable GetTarget()
        {
            return GameManager.Instance;
        }
       
    }
}