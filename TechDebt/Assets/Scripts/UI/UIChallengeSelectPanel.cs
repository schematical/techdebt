using MetaChallenges;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class UIChallengeSelectPanel: UIPanel
    {
        public void Initialize(MetaChallengeBase challenge, int currentProgress)
        {
            CleanUp();
            Debug.Log($"Initialize {challenge.DisplayName}");
            bool isCompleted = MetaGameManager.IsChallengeCompleted(MetaGameManager.ProgressData, challenge);
            GetComponent<Image>().color = isCompleted ? Color.green : Color.white;
            
            AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = challenge.DisplayName;
            AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = challenge.Description;
            AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = $"{currentProgress}/{challenge.RequiredValue}";
            if (!isCompleted)
            {
                return;
            }
            UIPanelLine rewardHolderLine = AddLine<UIPanelLine>();
            rewardHolderLine.Add<UIPanelLineSectionText>().text.text = $"Reward";

            foreach (RewardBase reward in challenge.Rewards)
            {
                UIPanelLine rewardLine = rewardHolderLine.AddLine<UIPanelLine>();
                // TODO: Make a panel that renders this
                rewardLine.AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = reward.GetType().ToString();
                // rewardLine.AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = reward.RewardId;
                rewardLine.AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = reward.RewardValue.ToString();
            }
        }
    }
}