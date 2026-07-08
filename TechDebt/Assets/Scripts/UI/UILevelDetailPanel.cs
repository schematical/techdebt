namespace UI
{
    public class UILevelDetailPanel: UIPanel
    {
        public override void Show()
        {
            base.Show();
            MapLevel level = GameManager.Instance.Map.GetCurrentLevel();
            MetaProgressData metaData = MetaGameManager.GetProgress();
            foreach(MapLevelReward mapLevelReward in level.LevelRewards)
            {
                UIPanelLine line = AddLine<UIPanelLine>();


                bool claimed = metaData.claimedMetaRewardIds.Contains(mapLevelReward.Id);
                line.Add<UIPanelLineSectionText>().text.text = $"{(claimed? "CLAIMED: ":"")}{mapLevelReward.Id} - {mapLevelReward.GameStage} - {mapLevelReward.GameStage} - {mapLevelReward.AppliedAt} - {mapLevelReward.Type}";
                /*line.Add<UIPanelLineSectionText>().text.text = $"{mapLevelReward.Description}";
                line.Add<UIPanelLineSectionText>().text.text = $"{mapLevelReward.GameStage}";
                line.Add<UIPanelLineSectionText>().text.text = $"{mapLevelReward.AppliedAt}";
                line.Add<UIPanelLineSectionText>().text.text = $"{mapLevelReward.Type}";*/
                UIPanelLine line2 = AddLine<UIPanelLine>();
                line2.Add<UIPanelLineSectionText>().text.text = $"Reward: {mapLevelReward.Reward.Name} -  {mapLevelReward.Reward.Description}";
                // line.Add<UIPanelLineSectionText>().text.text = $"Reward: {mapLevelReward.Reward.Description}";
                foreach (MapLevelVictoryConditionBase victoryConditionBase in mapLevelReward.VictoryConditions)
                {
                    UIPanelLine victoryConditionLine = AddLine<UIPanelLine>();
                    victoryConditionLine.Add<UIPanelLineSectionText>().text.text =
                        $"{victoryConditionBase.GetDescription()}";
                }
            }
            /*GameManager.Instance.UIManager.Block();

            AddButton("Play", () => { Close(); GameManager.Instance.UIManager.saveSlotListPanel.Show(); });
            AddButton("Discord", OpenDiscord);
            AddButton("Feedback", () => Application.OpenURL("https://forms.gle/NRRbLNhtoaJQrRzp9"));
            AddButton("Wishlist now!", () => Application.OpenURL("https://store.steampowered.com/app/4567430/Tech_Debt/"));
            AddButton("Quit", () => { Application.Quit(); });
            AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = $"Play Test - v{Application.version}";*/
        }
    }
}