using System;
using NPCs;
using UnityEngine;

namespace Tutorial
{
    public class SpawnFishingAttackEvent: EventBase
    {
        public override void Apply()
        {
           
          
            InternetPipe internetPipe = GameManager.Instance.GetRandomInfrastructureInstanceByClass<InternetPipe>();

            GameObject npcGO = GameManager.Instance.prefabManager.Create("NPCFishingAttack", internetPipe.transform.position);

            NPCFishingAttack npc = npcGO.GetComponent<NPCFishingAttack>();
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
            float inputValidation = 1; //  - GameManager.Instance.Stats.GetStatValue(StatType.NPC_Paranoia); TODO Create a Paranoa modifier?
            
            float attackPossibility = gameManager.GetStatValue(StatType.AttackPossibility);
            return (techDebt * releaseQuality * inputValidation * attackPossibility);
        }
        

    }
}