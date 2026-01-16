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
    }
}