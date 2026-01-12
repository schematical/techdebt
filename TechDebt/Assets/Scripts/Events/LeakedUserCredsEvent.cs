using System;
using Stats;
using UnityEngine;

namespace Events
{
    public class LeakedUserCredsEvent : EventBase
    {
        public LeakedUserCredsEvent()
        {
            EventStartText = "The User table was leaked with personally identifiable information. We will have to pay";
            // EventEndText = "TODO: Make a cycle credentials task you can trigger."; // Or perhaps an actual result
            Probability = 1;
        }
         public override bool IsPossible()
         {
             if (GameManager.Instance.GameLoopManager.currentDay < 4)
             {
                 return false;
             }
            InfrastructureInstance infrastructureInstance = GameManager.Instance.GetInfrastructureInstanceByID("cognito");
            if (infrastructureInstance == null)
            {
                Debug.LogError("No infrastructureInstance found for the specified 'cognito'.");
                return false;
            }

            return !infrastructureInstance.IsActive();
        }

         public override void Apply()
         {
             base.Apply();
             float money = GameManager.Instance.GetStat(StatType.Money);
             GameManager.Instance.SetStat(StatType.Money, money * 0.5f);
         }
         
    }
}
