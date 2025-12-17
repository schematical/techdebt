using System;
using System.Collections.Generic;

namespace Stats
{
    [Serializable]
    public class StatData
    {
        public enum DisplayType
        {
            None,
            Basic
        }

        public bool _broadcastByDefault = false;
        
        public StatType Type { get; private set; }
        public float BaseValue { get; private set; }
        public float Value { get; private set; }
        public List<StatModifier> Modifiers { get; private set; }  = new List<StatModifier>();
        
        public List<System.Action<StatData>> Listeners { get; set; } = new List<System.Action<StatData>>();

        public StatData(StatType statType, float baseValue)
        {
            Type = statType;
            BaseValue = baseValue;
            UpdateValue();
        }
        public float IncrStat(float value = 1)
        {
            BaseValue += value;
            return UpdateValue();
        }
        public float UpdateValue()
        {
            float value = BaseValue;
            // Apply modifier
            foreach (var modifier in Modifiers)
            {
                value = modifier.Apply(this, value);
            }

            Value = value;
            if (_broadcastByDefault)
            {
                Broadcast();
            }
            return Value;
        }

        public void Broadcast()
        {
            foreach (var listener in Listeners)
            {
                listener.Invoke(this);
            }
        }

        public void SetBaseValue(float value)
        {
            BaseValue = value;
        }
    }
}