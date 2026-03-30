using System;
using NPCs;
using UnityEngine;

namespace Tutorial
{
    public class SpawnDDoSEvent: EventBase
    {
        public override void Apply()
        {
            InternetPipe internetPipe = GameManager.Instance.GetRandomInfrastructureInstanceByClass<InternetPipe>();
            internetPipe.MarkDDoS();
        }
        public override float GetProbability()
        {
            return 100;
            GameManager gameManager = GameManager.Instance;
            ReleaseBase currentRelease = gameManager.GetCurrentRelease();
            if (currentRelease == null)
            {
                return 0;
            }

            float techDebt = gameManager.GetStatValue(StatType.TechDebt);
            
            float releaseQuality = 1 - currentRelease.GetQuality();
            float inputValidation = 1 - GameManager.Instance.Stats.GetStatValue(StatType.Infra_InputValidation);
            
            float attackPossibility = gameManager.GetStatValue(StatType.AttackPossibility);
            return (techDebt * releaseQuality * inputValidation * attackPossibility);
        }
        

    }
}