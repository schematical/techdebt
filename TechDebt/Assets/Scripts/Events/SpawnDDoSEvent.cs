using System;
using NPCs;
using UnityEngine;

namespace Tutorial
{
    public class SpawnDDoSEvent: EventBase
    {
        //TODO: Severity Affects Duration + Traffic Increase Speed.
        //TODO: Big Tech Bot Crawl
        public override void Apply()
        {
            InternetPipe internetPipe = GameManager.Instance.GetRandomInfrastructureInstanceByClass<InternetPipe>();
            int duration = (int)Math.Round(10 * GameManager.Instance.GetStatValue(StatType.Difficulty));
            internetPipe.MarkDDoS(duration);
        }
        public override float GetProbability()
        {
            // return 100;
            GameManager gameManager = GameManager.Instance;
            if (gameManager.Stats.GetStatValue(StatType.Traffic) < 200)
            {
                return 0;
            }
          
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