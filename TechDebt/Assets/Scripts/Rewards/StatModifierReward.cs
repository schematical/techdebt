using System;
using Stats;

namespace DefaultNamespace.Rewards
{
    public abstract class StatModifierReward: LeveledRewardBase
    {
        public StatType StatType;
        protected StatModifier StatModifier { get; private set; }
        protected iModifiable AttachedModifier { get; private set; }
        public abstract iModifiable GetTarget();
        public override void OnLevelUp(Rarity rarity)
        {
            if (StatModifier == null)
            {
                throw new SystemException($"StatModifier for {GetType()} is null");
            }
            StatModifier.SetValue(GetScaledValue());

        }

      
        public StatModifier BuildStatModifier()
        {
            StatModifier = new StatModifier(
                Id,
                GetScaledValue()
            );
            return StatModifier;
        }
       

        public override void Apply()
        {
            if (AttachedModifier == null)
            {
                AttachedModifier = GetTarget();
                StatModifier = BuildStatModifier();
                AttachedModifier.Stats.AddModifier(StatType, StatModifier);
            }
  
        }
    }
}