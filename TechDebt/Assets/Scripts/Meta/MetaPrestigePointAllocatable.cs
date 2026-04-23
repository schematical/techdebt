using System.Collections.Generic;

namespace MetaChallenges
{
    [System.Serializable]
    public class MetaPrestigePointAllocation
    {

        public string Id;
        public int level = 1;

    }
    public class MetaPrestigePointAllocatable
    {
        public string Id;
        public List<MetaPrestigePointAllocatableLevel> levels = new List<MetaPrestigePointAllocatableLevel>();
        public RewardBase reward;

        public MetaPrestigePointAllocation GetAllocation()
        {
            MetaPrestigePointAllocation allocation = MetaGameManager.GetProgress().prestigePointAllocations.Find((allocation => allocation.Id == Id));
            return allocation;
        }
    
    }

    public class MetaPrestigePointAllocatableLevel
    {
        public int cost;
    }
}