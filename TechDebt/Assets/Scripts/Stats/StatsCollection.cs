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
            if (Stats.TryGetValue(statType, out var statData))
            {
                return statData.Value;
            }
            return 0f;
        }

        public void AddModifier(StatType statType, StatModifier modifier)
        {
            foreach (StatType key in Stats.Keys)
            {
                Debug.Log($"networkPacketData.Stats  ???? {key} - {Stats[key].Value}");
            }
            if (!Stats.ContainsKey(statType))
            {
                foreach (StatType key in Stats.Keys)
                {
                    Debug.LogError($"networkPacketData.Stats  ???? {key} - {Stats[key].Value}");
                }
                throw new SystemException($"StatsCollection: StatType `{statType}` does not exist ");
            }
          
            Stats[statType].Modifiers.Add(modifier);
            Stats[statType].UpdateValue();
            
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

        public StatData Get(StatType _type)
        {
            if (!Stats.ContainsKey(_type))
            {
                return null;
            }
            return Stats[_type];
        }
    }
}