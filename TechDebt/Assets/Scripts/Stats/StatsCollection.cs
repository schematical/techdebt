using System;
using System.Collections.Generic;
using System.Data;
using Unity.VisualScripting;
using UnityEngine;

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
        
        public void Clear()
        {
            Stats.Clear();
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
            if(!Stats.ContainsKey(statType)) {
                throw new SystemException($"StatsCollection Missing StatType `{statType}`");
            }
            return Stats[statType].Value;
        }

        public float AddModifier(StatType statType, StatModifier modifier)
        {
            if (!Stats.ContainsKey(statType))
            {
                throw new SystemException($"StatsCollection: StatType `{statType}` does not exist. Debug: Count: {Stats.Count} ");
            }
            StatModifier existingStatModifier = Stats[statType].Modifiers.Find(mod => mod.Id == modifier.Id);
            if (existingStatModifier != null)
            {
                throw new SystemException($"A StatModifier with ID {modifier.Id} already exists - {statType}");
            }
            modifier.Initialize(this, statType);
            if (!Stats[statType].IsModifiable)
            {
                throw new SystemException($"{statType} is not a modifiable and we are trying to AddModifier with ID {modifier.Id}");
            }
            Stats[statType].Modifiers.Add(modifier);
            return Stats[statType].RefreshValue();
            
        }

        public bool RemoveModifier(StatType statType, StatModifier modifier)
        {
            if (Stats.TryGetValue(statType, out var statData))
            {
                if (statData.Modifiers.Remove(modifier))
                {
                    statData.RefreshValue();
                    return true;
                }
            }
            return false;
        }

        public void RemoveModifier(StatType statType, string id)
        {
            if (Stats.TryGetValue(statType, out var statData))
            {
                statData.Modifiers.RemoveAll(mod => mod.Id == id);
                statData.RefreshValue();
            }
        }

        public StatData Get(StatType _type)
        {
            if (!Stats.ContainsKey(_type))
            {
                return null;
            }
            return Stats[_type];
        }

        public void RefreshStatValue(StatType statType)
        {
            Stats[statType].RefreshValue();
        }

        public StatModifier GetModifierByTypeAndId(StatType statType, string id)
        {
            return Stats[statType].Modifiers.Find(mod => mod.Id == id);
        }
    }
}