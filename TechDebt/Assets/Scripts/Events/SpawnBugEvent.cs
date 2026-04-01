using System;
using UI;
using UnityEngine;

namespace Tutorial
{
    public class SpawnBugEvent: EventBase
    {
        public override void Apply()
        {
           GameManager.Instance.SpawnNPCBug();
           GameManager.Instance.SetStat(StatType.AttackPossibility, 0);
           if(GameManager.Instance.TutorialManager != null)
           {
               GameManager.Instance.TutorialManager.Trigger(TutorialStepId.NPC_Bug_Spawn);
           }
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
            float attackPossibility = gameManager.GetStatValue(StatType.AttackPossibility);
            return techDebt * releaseQuality  * attackPossibility;
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
            float attackPossibility = gameManager.GetStatValue(StatType.AttackPossibility);

            line.AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = $"techDebt: {techDebt} -  releaseQuality: {releaseQuality} - {attackPossibility} - Total: {techDebt * releaseQuality * attackPossibility} ";
        }

    }
}