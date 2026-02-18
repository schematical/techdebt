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
            RefreshValue();
        }
        public float IncrStat(float value = 1)
        {
            BaseValue += value;
            return RefreshValue();
        }
        public event Action<float> OnStatChanged;

        public float RefreshValue()
        {
            float value = BaseValue;
            // Apply modifier
            foreach (StatModifier modifier in Modifiers)
            {
                value = modifier.Apply(this, value);
            }

            /*if (
                GameManager.Instance  != null &&
                GameManager.Instance.GlobalStats.Stats.ContainsKey(Type)
                )
            {
                List<StatModifier> globalModifiers = GameManager.Instance.GlobalStats.Stats[Type].Modifiers;

                foreach (var globalModifier in globalModifiers)
                {
                    value = globalModifier.Apply(this, value);
                    
                }
            }*/

            /*if (Type == StatType.Traffic)
            {
               //  Debug.Log($"End Value: {Value}");
            }*/
            Value = value;
            if (_broadcastByDefault && Math.Abs(Value - value) > 0.00001f)
            {
                Broadcast();
            }

            if (Type == StatType.Infra_LoadPerPacket)
            {
                Debug.Log($"RefreshValue - {Type} BaseValue: {BaseValue} - NewValue: {Value}");
            }

            return Value;
        }

        public void Broadcast()
        {
            OnStatChanged?.Invoke(Value);
            foreach (var listener in Listeners)
            {
                listener.Invoke(this);
            }
        }

        public void SetBaseValue(float value)
        {
            BaseValue = value;
            RefreshValue();
        }

        public void ReplaceOrAdd(StatModifier statModifier)
        {
            int index = Modifiers.FindIndex(x => x.Id.Equals(statModifier.Id));
            if (index != -1)
            {
                Modifiers.RemoveAt(index);
            }
            Modifiers.Add(statModifier);
            RefreshValue();
        }
    }
}