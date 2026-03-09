using System;
using NPCs;
using UnityEngine;

namespace Events
{
    public class SpawnXSSEvent: EventBase
    {
        public override void Apply()
        {
           
          
            InternetPipe internetPipe = GameManager.Instance.GetRandomInfrastructureInstanceByClass<InternetPipe>();

            GameObject npcGO = GameManager.Instance.prefabManager.Create("NPCXSS", internetPipe.transform.position);

            NPCXSS npc = npcGO.GetComponent<NPCXSS>();
            npc.Initialize();
            GameManager.Instance.SetStat(StatType.AttackPossibility, 0);
            

        }
        public override float GetProbability()
        {
            
            GameManager gameManager = GameManager.Instance;
            ReleaseBase currentRelease = gameManager.GetCurrentRelease();
            if (currentRelease == null)
            {
                return 0;
            }

            float techDebt = gameManager.GetStatValue(StatType.TechDebt);
            
            float releaseQuality = 1 - currentRelease.GetQuality();
            float inputValidation = 1 - GameManager.Instance.Stats.GetStatValue(StatType.Infra_InputValidation);
         
            float releaseLevel = currentRelease.RewardModifier.GetLevel();
            float attackPossibility = gameManager.GetStatValue(StatType.AttackPossibility);
            return (techDebt * releaseQuality * releaseLevel * inputValidation * attackPossibility);
        }
        

    }
}