using System;
using Stats;

namespace DefaultNamespace.Rewards
{
    public abstract class StatBaseValueReward: RewardBase
    {
        public StatType StatType;
        protected iModifiable AttachedModifier { get; private set; }
        public abstract iModifiable GetTarget();
        public float BaseValue;


        public override string GetTitle()
        {
            return $"{base.GetTitle()}: {StatType} {BaseValue}";
        }

        


        public override void Apply()
        {
            AttachedModifier = GetTarget();
            AttachedModifier.Stats.Get(StatType).IncrStat(BaseValue);
  
        }
    }
}