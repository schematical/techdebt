using System;
using System.Collections.Generic;
using Stats;
using UnityEngine;

namespace DefaultNamespace.Rewards
{
    public abstract class StatBaseValueReward: RewardBase
    {
        public StatType StatType;
        protected iModifiable AttachedModifier { get; private set; }
        public abstract iModifiable GetTarget();
        public int Level = 0;
        public List<float> LevelValues = new(); 


        public override string GetTitle()
        {
            return $"{base.GetTitle()}: {StatType} {GetValue()}";
        }
        
        public float GetValue()
        {
            if (Level > LevelValues.Count)
            {
                throw new SystemException($"Level {Level} is greater than `LevelValues.Count`: {LevelValues.Count}");
            }
            return LevelValues[Level];
        }
        


        public override void Apply()
        {
            AttachedModifier = GetTarget();
            AttachedModifier.Stats.Get(StatType).IncrStat(GetValue());
  
        }
    }
}