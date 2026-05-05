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
            if (data.claimedMetaRewardIds.Contains(Id))
            {
                Debug.LogWarning($"MetaStatBaseValueReward.Id `{Id}` already claimed");
                return;
            }

            data.prestigePoints += BaseValue;
            data.claimedMetaRewardIds.Add(Id);
            MetaGameManager.SaveProgress(data);
            
            //Applied and saved progresss.
        }
    }
}