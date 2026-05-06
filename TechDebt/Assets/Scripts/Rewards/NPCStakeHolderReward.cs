

using System;
using UI;

namespace DefaultNamespace.Rewards
{
    public class NPCStakeHolderReward: RewardBase, iLevelable
    {
        public int Level {get; set;}
        public override void Apply()
        {
           // Go through and unlock any missing that fits that
           Stakeholder stakeholder = GameManager.Instance.Stakeholders.Find((stakeholder => stakeholder.Id == Id));
           if (stakeholder == null)
           {
               throw new SystemException($"Missing Stakeholder ID {Id}");
           }

           stakeholder.Level = Level;
           // UIMetaUnlockLevelData level = stakeholder.Levels.Find(level => level.Id == LevelId);

           // Add a desk

           // Add a NPC
        }
    }
}