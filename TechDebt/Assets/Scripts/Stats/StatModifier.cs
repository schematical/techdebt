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
        public StatModifier(string id, float value, ModifierType type = ModifierType.Multiply)
        {
            Id = id;
            Type = type;
            Value = value;
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