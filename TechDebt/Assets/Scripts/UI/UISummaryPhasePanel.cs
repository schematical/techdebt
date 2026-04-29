using System.Collections.Generic;
using MetaChallenges;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{

    public class UISummaryPhasePanel: UIPanel
    {
     

       

        public void ShowSummary(List<MapLevelVictoryConditionBase> victoryConditions, List<MetaChallengeBase> newlyUnlockedMetaChallenges)
        {
            CleanUp();
            GameManager.Instance.UIManager.ForcePause();
            GameManager.Instance.UIManager.Block();
            base.Show();
            AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().h1("Game Over");
            AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().h2("Failed Victory Conditions");
            

            foreach (MapLevelVictoryConditionBase condition in victoryConditions)
            {
                if (condition.GetFinalState() == VictoryConditionState.Failed)
                {
                    AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = $" - {condition.GetDescription()}";
               
                }
            }

            if (newlyUnlockedMetaChallenges.Count > 0)
            {
                AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().h2("Unlocked");
                foreach (MetaChallengeBase metaChallenge in newlyUnlockedMetaChallenges)
                {
                        AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text =
                            $" - {metaChallenge.DisplayName}";
                }
            }
            if (GameManager.Instance.Map.GetMetaRewards().Count > 0)
            {
                AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = "You have earned new `Vested Shares`. Spend them to unlock bonuses for future runs.";
                AddButton("Allocate Vested Shares", () =>
                {
                    GameManager.Instance.ShowMainMenu();
                    GameManager.Instance.UIManager.mainMenu.Close(true);
                    GameManager.Instance.UIManager.metaUnlockMapPanel.Show();
                });
            }
            else
            {
                AddButton("Start Over", () => { GameManager.Instance.StartNewGame(); });
                AddButton("Main Menu", () => { GameManager.Instance.ShowSaveSlotDetailPanel(); });
            }
            AddButton("Wishlist now!", () => Application.OpenURL("https://store.steampowered.com/app/4567430/Tech_Debt/")); 
            AddButton("Give Feedback", () => Application.OpenURL("https://forms.gle/NRRbLNhtoaJQrRzp9"));  
        }
        
    }
}
