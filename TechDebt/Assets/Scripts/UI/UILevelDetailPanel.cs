using System.Linq;

namespace UI
{
    public class UILevelDetailPanel: UIPanel
    {
        private UIPanelLine rewardsLine;
        public override void Show()
        {
            base.Show();
            UIPanelLineSectionText difficultyText = AddLine<UIPanelLine>().Add<UIPanelLineSectionText>();
            difficultyText.text.text =
                $"Difficulty: {GameManager.Instance.GetStatValue(StatType.Difficulty)}";
            difficultyText.SetToolTip("BANANAS");
            AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text =
                $"Rarity: {GameManager.Instance.GetStatValue(StatType.Global_LevelUpRarityModifier)}";
            rewardsLine = AddLine<UIPanelLine>();
            rewardsLine.Add<UIPanelLineSectionText>().text.text = $"Rewards";
            rewardsLine.SetExpandable(OnExpandRewards);
        }
        public void OnExpandRewards(UIPanelLine rewardsLine) {

            MapLevel level = GameManager.Instance.Map.GetCurrentLevel();
            MetaProgressData metaData = MetaGameManager.GetProgress();
            foreach(MapLevelReward mapLevelReward in level.LevelRewards)
            {
                UIPanelLine line = rewardsLine.AddLine<UIPanelLine>();
                bool locked = !mapLevelReward.DependencyIds.All(depId => metaData.claimedMetaRewardIds.Contains(depId));
               

                bool claimed = metaData.claimedMetaRewardIds.Contains(mapLevelReward.Id);
                string prefix = "Open";
                string icon = "3";
                if (locked)
                {
                    prefix = "Locked";
                    icon = "7";
                } else if (claimed)
                {
                    icon = "13";
                    prefix = "Claimed";
                }

                
                line.Add<UIPanelLineSectionImage>().image.sprite = GameManager.Instance.SpriteManager.GetSprite("TechTreeTiles",icon);
                line.Add<UIPanelLineSectionText>().text.text =
                    $"({prefix}) {mapLevelReward.Description}";
                line.SetExpandable((line2) =>
                {
                    line2.AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = $"Reward Name: {mapLevelReward.Reward.Name} ";
                    line2.AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = $"Reward Description: {mapLevelReward.Reward.Description}";
                    line2.AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = $"Reward Title: {mapLevelReward.Reward.GetTitle()}";
                    line2.AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = $"Description: {mapLevelReward.Description}";
                    line2.AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = $"Game Stage: {mapLevelReward.GameStage}";
                    line2.AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = $"Applied At: {mapLevelReward.AppliedAt}";
                    line2.AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = $"Type: {mapLevelReward.Type}";
                    if (mapLevelReward.VictoryConditions.Count > 0)
                    {
                        line2.AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = $"Victory Conditions:";
                        foreach (MapLevelVictoryConditionBase victoryConditionBase in mapLevelReward.VictoryConditions)
                        {
                            UIPanelLine victoryConditionLine = line2.AddLine<UIPanelLine>();
                            victoryConditionLine.Add<UIPanelLineSectionText>().text.text =
                                $" - {victoryConditionBase.GetDescription()}";
                        }
                    }

                    if (locked)
                    {
                        line2.AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = $"Unlock Requirements:";
                        foreach (string dependencyId in mapLevelReward.DependencyIds)
                        {
                            bool claimed2 = metaData.claimedMetaRewardIds.Contains(dependencyId);
                            UIPanelLine victoryConditionLine = line2.AddLine<UIPanelLine>();
                            victoryConditionLine.Add<UIPanelLineSectionText>().text.text =
                                $" - {dependencyId} - {(claimed2?"Claimed":"Unclaimed")}";
                        }
                    }
                });
            }
        }
    }
    
}