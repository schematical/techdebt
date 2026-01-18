using System;
using NPCs;

namespace Stats
{
    [Serializable]
    public class StatModifier
    {
        public StatModifier(ModifierType type, float value, iModifierSource source = null)
        {
            Type = type;
            Value = value;
            Source = source;
        }

        public enum ModifierType { Flat, Multiply }
        
        public ModifierType Type { get; private set; }
        public float Value { get; private set; }
        public object Source { get; private set; }

        public float Apply(StatData statData, float value)
        {
            switch (Type)
            {
                case ModifierType.Flat:
                    return value + Value;
                case ModifierType.Multiply:
                    return value * Value;
                default:
                    throw new NotImplementedException();
            }
        }
        public string GetDisplayText()
        {
            switch (Type)
            {
                case ModifierType.Flat:
                    return $"+{Value}";
                case ModifierType.Multiply:
                    return $"x{Value}";
                default:
                    throw new NotImplementedException();
            }
     
        }
    }
}