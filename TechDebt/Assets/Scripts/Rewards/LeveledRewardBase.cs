using System;
using System.Collections.Generic;

namespace DefaultNamespace.Rewards
{
    public abstract class LeveledRewardBase: RewardBase
    {
        public List<Rarity> Levels { get; set; } = new List<Rarity>();
        public float BaseValue { get; set; } = 1f;
        
        public ScaleDirection ScaleDirection = ScaleDirection.Up;
        public float GetScaledValue()
        {
            float percent = BaseValue;
            foreach (Rarity rarity in Levels)
            {
                percent *= GetScaledAdjustmentValue(rarity);
            }
            return percent;
        }

        public float GetScaledAdjustmentValue(Rarity rarity)
        {
            float scaleValue =  RarityHelper.GetDefaultScaleValue(rarity);
            switch (ScaleDirection)
            {
                case ScaleDirection.Up:
                    break;
                case ScaleDirection.Down:
                    scaleValue = (1 - scaleValue) + 1;
                    break;
                default:
                    throw new SystemException("Unknown scale direction: " + ScaleDirection);
            }

            return scaleValue;
        }
        
        public int LevelUp(Rarity rarity)
        {

            Levels.Add(rarity);
            OnLevelUp(rarity);
            return Levels.Count;
        }

        public virtual void OnLevelUp(Rarity rarity)
        {
        }

        public int GetLevel()
        {
            return Levels.Count;
        }
    }
}