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

        public void Add(StatData statData)
        {
            if (Stats.ContainsKey(statData.Type))
            {
                throw new SyntaxErrorException($"StatsCollection: StatType already exists - {statData.Type}");
            }
            Stats.Add(statData.Type, statData);
        }
    }
}