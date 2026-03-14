using Infrastructure;
using NPCs;
using Stats;

namespace DefaultNamespace.Rewards
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