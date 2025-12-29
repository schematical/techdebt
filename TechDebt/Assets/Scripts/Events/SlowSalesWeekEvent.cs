using Stats;
using UnityEngine;

namespace Events
{
    public class SlowSalesWeekEvent: EventBase
    {
        public SlowSalesWeekEvent()
        {
            EventStartText = "Slow Sales Day";
            EventEndText = "Slow Sales Over";
        }
        public override void Apply()
        {
            base.Apply();
            Debug.Log("Applying SlowSalesWeekEvent");
            StatModifier statModifier = new StatModifier(StatModifier.ModifierType.Multiply, 0.1f);
            GameManager.Instance.Stats.AddModifier(StatType.Traffic, statModifier);
        }

        public override bool IsPossible()
        {
            if (GameManager.Instance.CurrentEvents.Contains(this))
            {
                return false;
            }
            return true;
        }
    }
}