using System;
using DefaultNamespace.Rewards;
using Stats;
using UI;
using UnityEngine;

namespace Rewards
{
    public abstract class StatModifierReward: LeveledRewardBase
    {
        public StatType StatType;
        protected StatModifier StatModifier { get; private set; }
        protected iModifiable AttachedModifiable { get; private set; }
        public abstract iModifiable GetTarget();
        public override void OnLevelUp(Rarity rarity)
        {
            if (StatModifier == null)
            {
                throw new SystemException($"StatModifier for Name: {Name} - Type: {GetType()} is null");
            }
            StatModifier.SetValue(GetScaledValue());

        }
        public StatModifier BuildPreviewStatModifier()
        {
            StatModifier statModifier = new StatModifier(
                Id,
                GetScaledValue()
            );
            return statModifier;
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
            // Debug.Log($"Applying StatModifier for Name: {Name}");
            AttachedModifiable = GetTarget();
            StatModifier = BuildStatModifier();
            AttachedModifiable.Stats.AddModifier(StatType, StatModifier);
        }
        public override UIPanelLine Render(UIPanelLine line)
        {
            UIPanelLine rewardLine = base.Render(line);
            rewardLine.AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = $"Stat: {Util.GetDisplayable(StatType.ToString())}";
            return rewardLine;
        }
    }
}