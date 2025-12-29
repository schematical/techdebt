using System;
using System.Collections.Generic;
using UnityEngine;

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
        public event Action OnStatChanged;

        public float UpdateValue()
        {
            float value = BaseValue;
            if (Type == StatType.Traffic)
            {
                Debug.Log($"BaseValue: {BaseValue}");
            }
            // Apply modifier
            foreach (var modifier in Modifiers)
            {
                value = modifier.Apply(this, value);
                if (Type == StatType.Traffic)
                {
                    Debug.Log($"Modifying: {modifier.Type} - {modifier.Value} = {value}");
                }
            }

            if (
                GameManager.Instance  != null &&
                GameManager.Instance.GlobalStats.Stats.ContainsKey(Type)
                )
            {
                List<StatModifier> globalModifiers = GameManager.Instance.GlobalStats.Stats[Type].Modifiers;

                foreach (var globalModifier in globalModifiers)
                {
                    value = globalModifier.Apply(this, value);
                    
                }
            }

            if (Type == StatType.Traffic)
            {
                Debug.Log($"End Value: {Value}");
            }
            Value = value;
            if (Math.Abs(Value - value) > 0.00001f)
            {
                Broadcast();
            }
            return Value;
        }

        public void Broadcast()
        {
            OnStatChanged?.Invoke();
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