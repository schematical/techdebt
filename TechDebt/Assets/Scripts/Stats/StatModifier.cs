using System;

namespace Stats
{
    [Serializable]
    public class StatModifier
    {
        public StatModifier(ModifierType type, float value, object source = null)
        {
            Type = type;
            Value = value;
            Source = source;
        }

        public enum ModifierType { Multiply }
        
        public ModifierType Type { get; private set; }
        public float Value { get; private set; }
        public object Source { get; private set; }

        public float Apply(StatData statData, float value)
        {
            switch (Type)
            {
                case ModifierType.Multiply:
                default:
                    return value * Value;
            }
        }
    }
}