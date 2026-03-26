using Infrastructure;
using NPCs;
using Stats;

namespace Rewards
{
    public class WorldObjectTypeStatModifierReward: StatModifierReward
    {
        public WorldObjectType.Type WorldObjectType;
        public override iModifiable GetTarget()
        {
            return GameManager.Instance.WorldObjectTypes[WorldObjectType];
        }
    }
}