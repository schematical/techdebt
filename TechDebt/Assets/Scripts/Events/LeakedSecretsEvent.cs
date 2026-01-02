using System;
using Stats;
using UnityEngine;

namespace Events
{
    public class LeakedSecretEvent : EventBase
    {
        public LeakedSecretEvent()
        {
            EventStartText = "Some how we are sending hundreds on unauthorized email. This is going to cost us...";
            EventEndText = "TODO: Make a cycle credentials task you can trigger."; // Or perhaps an actual result
            Probility = 1;
        }
         public override bool IsPossible()
         {
             if (GameManager.Instance.GameLoopManager.currentDay < 4)
             {
                 return false;
             }
            InfrastructureInstance infrastructureInstance = GameManager.Instance.GetInfrastructureInstanceByID("secret-manager");
            if (infrastructureInstance == null)
            {
                Debug.LogError("No infrastructureInstance found for the specified 'secret-manager'.");
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
