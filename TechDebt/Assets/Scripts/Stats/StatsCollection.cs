using System;
using System.Collections.Generic;
using System.Data;
using Unity.VisualScripting;

namespace Stats
{
    [Serializable]
    public class StatsCollection
    {
        public Dictionary<StatType, StatData> Stats { get; private set; }

        public StatsCollection()
        {
            Stats = new Dictionary<StatType, StatData>();
        }
        
        public void Add(StatData statData)
        {
            if (Stats.ContainsKey(statData.Type))
            {
                throw new SyntaxErrorException($"StatsCollection: StatType already exists - {statData.Type}");
            }
            Stats.Add(statData.Type, statData);
        }

        public float GetStatValue(StatType statType)
        {
            if (Stats.TryGetValue(statType, out var statData))
            {
                return statData.Value;
            }
            return 0f;
        }

        public void AddModifier(StatType statType, StatModifier modifier)
        {
            if (Stats.TryGetValue(statType, out var statData))
            {
                statData.Modifiers.Add(modifier);
                statData.UpdateValue();
            }
            else
            {
                throw new SystemException($"StatsCollection: StatType does not exist {statType}");
            }
        }

        public bool RemoveModifier(StatType statType, StatModifier modifier)
        {
            if (Stats.TryGetValue(statType, out var statData))
            {
                if (statData.Modifiers.Remove(modifier))
                {
                    statData.UpdateValue();
                    return true;
                }
            }
            return false;
        }

        public void RemoveModifiers(StatType statType, object source)
        {
            if (Stats.TryGetValue(statType, out var statData))
            {
                statData.Modifiers.RemoveAll(mod => mod.Source == source);
                statData.UpdateValue();
            }
        }
    }
}