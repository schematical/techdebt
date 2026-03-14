using System;
using System.Collections.Generic;

namespace MetaChallenges
{
    public class MetaChallengeBase
    {
        public enum MetaChallengeType
        {
            Active, // They have to opt into this challenge
            Passive, // Is always listening and checking in the background
        }
        public enum MetaChallengeRequirementType
        {
            Cumulative, 
            Highest
        }
      
        
        public string ChallengeID; // Unique identifier
        public string DisplayName; // For UI
        public string Description; // For UI
        public MetaChallengeRequirementType RequirementType = MetaChallengeRequirementType.Cumulative;
        public string WorldObjectTypeId { get; set; }
        public List<RewardBase> Rewards;

        public MetaStat metaStat;
        public int RequiredValue = -1;
        public MetaChallengeType Type = MetaChallengeType.Passive;
        

    }
}