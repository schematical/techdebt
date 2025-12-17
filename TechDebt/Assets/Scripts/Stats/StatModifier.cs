using System;

namespace Stats
{
    [Serializable]
    public class StatModifier
    {
        public enum ModifierType { Multiply }
        
        public ModifierType Type { get; private set; }
        public float Value { get; private set; }

        public float Apply(StatData statData, float value)
        {
            switch (Type)
            {
                case(ModifierType.Multiply):
                default:
                    return value * Value;
                            break;
            }
        }
    }
}