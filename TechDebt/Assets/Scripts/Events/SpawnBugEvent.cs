using System;
using UI;
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
            float releaseLevel = currentRelease.RewardModifier.GetLevel();
            float attackPossibility = gameManager.GetStatValue(StatType.AttackPossibility);
            return techDebt * releaseQuality * releaseLevel  * attackPossibility;
        }
        public override void Render(UIPanelLine line)
        {
            GameManager gameManager = GameManager.Instance;
            ReleaseBase currentRelease = gameManager.GetCurrentRelease();
            if (currentRelease == null)
            {
                line.AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = $"{GetType().Name.Replace("Event", "")} - Prob: 0";
            }

            float techDebt = gameManager.GetStatValue(StatType.TechDebt);
            
            float releaseQuality = 1 - currentRelease.GetQuality();
            float releaseLevel = currentRelease.RewardModifier.GetLevel();
            float attackPossibility = gameManager.GetStatValue(StatType.AttackPossibility);

            line.AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = $"techDebt: {techDebt} -  releaseQuality: {releaseQuality} - releaseLevel: {releaseLevel} - {attackPossibility} - Total: {techDebt * releaseQuality * releaseLevel * attackPossibility} ";
        }

    }
}