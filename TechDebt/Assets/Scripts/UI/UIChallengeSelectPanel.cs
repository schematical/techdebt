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
            GetComponent<Image>().color = new Color(
                GetComponent<Image>().color.r,
                GetComponent<Image>().color.g,
                GetComponent<Image>().color.b,
                isCompleted ? .5f : 1f
            );
            
            AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = challenge.DisplayName;
            AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = challenge.Description;
            AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = $"{currentProgress}/{challenge.RequiredValue}";
            UIPanelLine rewardHolderLine = AddLine<UIPanelLine>();
            rewardHolderLine.Add<UIPanelLineSectionText>().text.text = $"Reward";

            foreach (RewardBase reward in challenge.Rewards)
            {
                UIPanelLine rewardLine = rewardHolderLine.AddLine<UIPanelLine>();
                rewardLine.AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = reward.Type.ToString();
                rewardLine.AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = reward.RewardId;
                rewardLine.AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = reward.RewardValue.ToString();
            }
        }
    }
}