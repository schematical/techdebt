using System.Collections.Generic;

namespace MetaChallenges
{
    public class MetaStatCollection
    {
        public Dictionary<MetaStat, int> Stats = new Dictionary<MetaStat, int>();

        public int Incr(MetaStat stat, int value = 1)
        {
            if (!Stats.ContainsKey(stat))
            {
                Stats[stat] = 0;
            }
            Stats[stat] += value;
            return Stats[stat];
        }

        public int Get(MetaStat stat)
        {
            if (!Stats.ContainsKey(stat))
            {
                return 0;
            }
            return Stats[stat];
        }

        public void Set(MetaStat stat, int val)
        {
            Stats[stat] = val;
        }
    }
}