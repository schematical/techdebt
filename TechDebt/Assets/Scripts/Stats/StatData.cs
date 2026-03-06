using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Stats
{
    [Serializable]
    public class StatData
    {
        public enum StatDataDisplayType
        {
            Default,
            Percentage,
            Dollar
        }

        public enum StatDataBelowZeroBehavior
        {
            AllowNegative,
            SetZero
        }
        public bool _broadcastByDefault = false;
        
        
        public StatType Type { get; private set; }
        public float BaseValue { get; private set; }
        public float Value { get; private set; }
        public StatDataBelowZeroBehavior BelowZeroBehavior = StatDataBelowZeroBehavior.AllowNegative;
        public bool IsModifiable = true;
        public StatDataDisplayType DisplayType = StatDataDisplayType.Default;
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

            float value = CalculateValue();
            Value = value;
            if (Value < 0)
            {
                switch (BelowZeroBehavior)
                {
                    case(StatDataBelowZeroBehavior.SetZero):
                        Value = 0;
                        BaseValue = 0;
                        break;
                    case(StatDataBelowZeroBehavior.AllowNegative):
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
            if (_broadcastByDefault && Math.Abs(Value - value) > 0.00001f)
            {
                Broadcast();
            }


            return Value;
        }

        protected float CalculateValue(List<StatModifier> modifiers = null)
        {
            if (modifiers == null)
            {
                modifiers = Modifiers;
            }
            float value = BaseValue;
            if (!IsModifiable && Modifiers.Count > 0)
            {
                Debug.LogError("Non Modifiable StatData has modifier");
            }
            // Apply modifier
            foreach (StatModifier modifier in modifiers)
            {
                value = modifier.Apply(this, value);
            }

            return value;

        }

        public float PreviewValue(StatModifier modifier)
        {
            List<StatModifier> modifiers = new List<StatModifier>(Modifiers);
            modifiers.Add(modifier);
            return CalculateValue(modifiers);
        }

        public string GetPreviewText(StatModifier modifier)
        {
            float value = PreviewValue(modifier);
            return $"{Type}: {FormatDisplayValue(Value)} {modifier.GetDisplayText()} => {FormatDisplayValue(value)}";
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

        public string GetDescription()
        {
           return FormatDescription(Value);
        }

        public string FormatDescription(float value)
        {
            return $"{Type}: {FormatDisplayValue(value)}";
        }

        public string GetDisplayValue()
        {
            return FormatDisplayValue(Value);
        }
        public string GetDisplayBaseValue()
        {
            return FormatDisplayValue(BaseValue);
        }

        public string FormatDisplayValue(float value)
        {
            if (value < 0)
            {
                switch (BelowZeroBehavior)
                {
                    case(StatDataBelowZeroBehavior.SetZero):
                        value = 0;
                        break;
                    case(StatDataBelowZeroBehavior.AllowNegative):
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
            switch (DisplayType)
            {
                case(StatDataDisplayType.Percentage):
                    return $"{Math.Round(value  * 100)}%";
                case(StatDataDisplayType.Dollar):
                    return $"${Math.Round(value  * 100)/100}";
                case(StatDataDisplayType.Default):
                    return $"{Math.Round(value)}";
                default:
                    throw new NotImplementedException();
            }
            
        }
    }
}