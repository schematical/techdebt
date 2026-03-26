using NPCs;
using Stats;

namespace Rewards
{
    public class GlobalStatModifierReward: StatModifierReward
    {

        public override iModifiable GetTarget()
        {
            return GameManager.Instance;
        }
       
    }
}