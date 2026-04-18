using System;
using System.Collections.Generic;
using UI;

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
        public override string GetDescription()
        {
            string desc = base.GetDescription();
            if (GetLevel() == 0)
            {
                desc += "\nNew!";
            }
            else
            {

                desc += $"\nLevel: {GetLevel()}";
            }

            return desc;
        }

        public override UIPanelLine Render(UIPanelLine line)
        {
            UIPanelLine rewardLine = base.Render(line);
            rewardLine.AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = $"Scaled Value: {Math.Round(GetScaledValue()  *100)}%";
            UIPanelLine levelsLine = rewardLine.AddLine<UIPanelLine>();
            levelsLine.Add<UIPanelLineSectionText>().text.text = $"Level: {Levels.Count + 1}";
            levelsLine.SetExpandable((levelsLine =>
            {
                levelsLine.AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = $" - Level 1";
                for (int i = 0; i < Levels.Count; i++)
                {
                    UIPanelLine levelLine = levelsLine.AddLine<UIPanelLine>();
                    levelLine.Add<UIPanelLineSectionText>().text.text = $" - Level {i + 2}: {Levels[i]} - {GetScaledAdjustmentValue(Levels[i])}";
                }
            }));
            

            return rewardLine;
        }
    }
}