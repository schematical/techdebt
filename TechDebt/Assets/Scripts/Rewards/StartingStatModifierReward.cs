using System;
using Stats;

namespace DefaultNamespace.Rewards
{
    public class StartingStatModifierReward: RewardBase
    {
        public StatType StatType;
        public override void Apply()
        {

            GameManager.Instance.Stats.AddModifier(StatType, new StatModifier(
                $"metaChallenge_{StatType}",
                RewardValue
            ));
        }
    }
}