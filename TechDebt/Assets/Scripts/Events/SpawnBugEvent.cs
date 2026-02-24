using System;
using UnityEngine;

namespace Events
{
    public class SpawnBugEvent: EventBase
    {
        public override void Apply()
        {
           GameManager.Instance.GetCurrentRelease().SpawnBug();
        }
        public override int GetProbability()
        {
            GameManager gameManager = GameManager.Instance;
            ReleaseBase currentRelease = gameManager.GetCurrentRelease();
            if (currentRelease == null)
            {
                return 0;
            }

            float techDebt = gameManager.GetStat(StatType.TechDebt);
            
            float releaseQuality = 1 - currentRelease.GetQuality();
            float releaseLevel = currentRelease.RewardModifier.GetLevel();
            
            return (int)Math.Round(techDebt * releaseQuality * releaseLevel);
        }
        public override string GetDescription()
        {
            GameManager gameManager = GameManager.Instance;
            ReleaseBase currentRelease = gameManager.GetCurrentRelease();
            if (currentRelease == null)
            {
                return $"{GetType().Name.Replace("Event", "")} - Prob: 0";
            }

            float techDebt = gameManager.GetStat(StatType.TechDebt);
            
            float releaseQuality = 1 - currentRelease.GetQuality();
            float releaseLevel = currentRelease.RewardModifier.GetLevel();

            return $"{GetType().Name.Replace("Event", "")} - Prob: {GetProbability():F2} - techDebt: {techDebt} -  releaseQuality: {releaseQuality} - releaseLevel: {releaseLevel} - Total: {techDebt * releaseQuality * releaseLevel} ";
        }

    }
}