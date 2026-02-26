using System;
using NPCs;

namespace Stats
{
    [Serializable]
    public class StatModifier
    {
    
        public string Id { get; protected set; }
        public enum ModifierType {  Multiply /*Flat,*/ }
        
        public ModifierType Type { get; private set; }
        public float Value { get; private set; }
        protected StatType statType;
        protected StatsCollection statsCollection;
        public StatModifier(string id, float value, ModifierType type = ModifierType.Multiply)
        {
            Id = id;
            Type = type;
            Value = value;
        }

        public void Initialize(StatsCollection _statsCollection, StatType _statType)
        {
            statType = _statType;
            statsCollection = _statsCollection;
        }
        public float Apply(StatData statData, float value)
        {
            switch (Type)
            {
                /*case ModifierType.Flat:
                    return value + Value;*/
                case ModifierType.Multiply:
                    return value * Value;
                default:
                    throw new NotImplementedException();
            }
        }

        public void Remove()
        {
            statsCollection.RemoveModifier(statType, Id);
        }

        public void SetValue(float value)
        {
            Value = value;
            statsCollection.RefreshStatValue(statType);
        }
        public string GetDisplayText()
        {
            switch (Type)
            {
                /*case ModifierType.Flat:
                    return $"+{Value}";*/
                case ModifierType.Multiply:
                    return $"x{Value}";
                default:
                    throw new NotImplementedException();
            }
     
        }
    }
}