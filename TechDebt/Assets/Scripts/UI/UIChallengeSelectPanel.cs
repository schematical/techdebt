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
            
            bool isCompleted = MetaGameManager.IsChallengeCompleted(MetaGameManager.GetProgress(), challenge);
            GetComponent<Image>().color = isCompleted ? Color.green : Color.white;
            
            AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = challenge.DisplayName;
            UIPanelLine descriptionLine = AddLine<UIPanelLine>();
            descriptionLine.Add<UIPanelLineSectionText>().text.text = challenge.Description;
            descriptionLine.GetComponent<LayoutElement>().minHeight = 60;
            AddLine<UIPanelLine>();
            UIPanelLine progressLine = AddLine<UIPanelLine>();
            progressLine.GetComponent<LayoutElement>().minHeight = 50;
            progressLine.Add<UIPanelLineSectionText>().text.text = $"{currentProgress}/{challenge.RequiredValue}";
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
                rewardLine.AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = reward.GetDescription();
                // rewardLine.AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = reward.AllocationId;
                // rewardLine.AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = reward.RewardValue.ToString();
            }
            Refresh();
        }
    }
}