using System;
using UnityEngine;

namespace Events
{
    public class SpawnBugEvent: EventBase
    {
        public override void Apply()
        {
           GameManager.Instance.GetCurrentRelease().SpawnBug();
           GameManager.Instance.SetStat(StatType.AttackPossibility, 0);
        }
        public override int GetProbability()
        {
            GameManager gameManager = GameManager.Instance;
            ReleaseBase currentRelease = gameManager.GetCurrentRelease();
            if (currentRelease == null)
            {
                return 0;
            }

            float techDebt = gameManager.GetStatValue(StatType.TechDebt);
            
            float releaseQuality = 1 - currentRelease.GetQuality();
            float releaseLevel = currentRelease.RewardModifier.GetLevel();
            float attackPossibility = gameManager.GetStatValue(StatType.AttackPossibility);
            return (int)Math.Round(techDebt * releaseQuality * releaseLevel  * attackPossibility);
        }
        public override string GetDescription()
        {
            GameManager gameManager = GameManager.Instance;
            ReleaseBase currentRelease = gameManager.GetCurrentRelease();
            if (currentRelease == null)
            {
                return $"{GetType().Name.Replace("Event", "")} - Prob: 0";
            }

            float techDebt = gameManager.GetStatValue(StatType.TechDebt);
            
            float releaseQuality = 1 - currentRelease.GetQuality();
            float releaseLevel = currentRelease.RewardModifier.GetLevel();
            float attackPossibility = gameManager.GetStatValue(StatType.AttackPossibility);

            return $"{GetType().Name.Replace("Event", "")} - Prob: {GetProbability():F2} - techDebt: {techDebt} -  releaseQuality: {releaseQuality} - releaseLevel: {releaseLevel} - {attackPossibility} - Total: {techDebt * releaseQuality * releaseLevel * attackPossibility} ";
        }

    }
}