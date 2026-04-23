using System;
using Stats;
using UnityEngine;

namespace DefaultNamespace.Rewards
{
    public class MetaStatBaseValueReward: RewardBase
    {
      
        public int BaseValue = 1;


        public override string GetTitle()
        {
            return $"Vested Shares: {BaseValue}";
        }

        


        public override void Apply()
        {
            //TODO: Possibly move this to the end of the run. So you can chose if they vest or not.
            MetaProgressData data = MetaGameManager.GetProgress();
            data.prestigePoints += BaseValue;
            MetaGameManager.SaveProgress(data);
            
            //Applied and saved progresss.
        }
    }
}