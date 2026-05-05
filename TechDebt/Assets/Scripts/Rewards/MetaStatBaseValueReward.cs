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
         
            MetaProgressData data = MetaGameManager.GetProgress();
            data.prestigePoints += BaseValue;
            MetaGameManager.SaveProgress(data);
            
        }
    }
}