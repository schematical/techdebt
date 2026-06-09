using System.Collections.Generic;
using DefaultNamespace;

namespace MetaChallenges
{
    public class MetaProgressUpdateContext
    {
        public GameStage startingStage;
        public GameStage currentStage;
        public List<MetaChallengeBase> newlyPassedChallenges { get; set; }
        
        public bool HasUnlockedNewStage()
        {
            return startingStage != currentStage;
        }
    }
}