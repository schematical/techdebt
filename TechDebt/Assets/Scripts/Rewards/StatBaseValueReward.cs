using System;
using Stats;
using UnityEngine;

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
            Debug.Log($"Applying {GetTitle()} - {StatType} {BaseValue}");
            AttachedModifier = GetTarget();
            AttachedModifier.Stats.Get(StatType).IncrStat(BaseValue);
  
        }
    }
}