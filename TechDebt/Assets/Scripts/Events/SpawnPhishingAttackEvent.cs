using System;
using NPCs;
using UnityEngine;

namespace Tutorial
{
    public class SpawnPhishingAttackEvent: EventBase
    {
        public override void Apply()
        {
           
          
            InternetPipe internetPipe = GameManager.Instance.GetRandomInfrastructureInstanceByClass<InternetPipe>();

            GameObject npcGO = GameManager.Instance.prefabManager.Create("NPCPhishingAttack", internetPipe.transform.position);

            NPCPhishingAttack npc = npcGO.GetComponent<NPCPhishingAttack>();
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
            if (gameManager.Stats.GetStatValue(StatType.Traffic) < 100)
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