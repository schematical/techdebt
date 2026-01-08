using Stats;
using UnityEngine;

namespace Events
{
    public class SlowSalesWeekEvent: EventBase
    {
        private StatModifier statModifier;
        public SlowSalesWeekEvent()
        {
            EventStartText = "Slow Sales Day";
            EventEndText = "Slow Sales Over";
        }
        public override void Apply()
        {
            base.Apply();
            statModifier = new StatModifier(StatModifier.ModifierType.Multiply, 0.1f);
            GameManager.Instance.Stats.AddModifier(StatType.DailyIncome, statModifier);
       
        }

        public override void End()
        {
            GameManager.Instance.Stats.RemoveModifier(StatType.DailyIncome, statModifier);
            base.End();
        

        }

        public override bool IsPossible()
        {
            if (GameManager.Instance.CurrentEvents.Contains(this))
            {
                return false;
            }

            if (GameManager.Instance.GameLoopManager.currentDay < 4)
            {
                return false;
            }
            return true;
        }
        public override bool IsOver()
        {
            return true;
        }
    }
}