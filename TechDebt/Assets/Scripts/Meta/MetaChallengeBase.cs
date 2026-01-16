using System;

namespace MetaChallenges
{
    public class MetaChallengeBase
    {
        public enum MetaChallengeType
        {
            Active, // They have to opt into this challenge
            Passive, // Is always listening and checking in the background
        }
        public enum MetaChallengeRewardType
        {
            Technology
        }
        
        public string ChallengeID; // Unique identifier
        public string DisplayName; // For UI
        public string Description; // For UI
        
        public string InfrastructureId { get; set; }
        public MetaStat metaStat;
        public int RequiredValue = -1;
        public MetaChallengeType Type = MetaChallengeType.Passive;
        
        public MetaChallengeRewardType RewardType= MetaChallengeRewardType.Technology;
        public string RewardId;

  
        public bool IsCompleted;
    }
}