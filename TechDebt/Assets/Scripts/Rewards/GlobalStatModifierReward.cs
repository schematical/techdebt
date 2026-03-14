using NPCs;
using Stats;

namespace DefaultNamespace.Rewards
{
    public class GlobalStatModifierReward: StatModifierReward
    {

        public override iModifiable GetTarget()
        {
            return GameManager.Instance;
        }
       
    }
}