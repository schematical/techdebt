using System;
using UnityEngine;

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

        public enum ModifierType { Flat, Multiply }
        
        [SerializeField] public ModifierType Type;
        [SerializeField] public float Value;
        [NonSerialized] public object Source;

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
    }
}