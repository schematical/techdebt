using NPCs;
using Stats;

namespace DefaultNamespace.Rewards
{
    public class NPCStatModifierReward: StatModifierReward
    {

        public NPCBase npc;

        public void SetTarget(NPCBase npc)
        {
            this.npc = npc;
        }

        public override iModifiable GetTarget()
        {
            return npc;
        }

    }
}