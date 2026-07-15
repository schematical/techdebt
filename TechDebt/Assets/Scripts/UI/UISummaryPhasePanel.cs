using System.Collections.Generic;
using DefaultNamespace;
using MetaChallenges;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
  
    public class UISummaryPhasePanel: UIPanel
    {
        private List<MapLevelVictoryConditionBase> _victoryConditions;
        private MetaProgressUpdateContext _context;
        private List<string> _pendingUnlocks;
        private List<string> _claimedUnlocks = new();
        private bool _pendingDifficultyUnlock;
        private bool _difficultyClaimed;
        private UIPanelButton _lastClaimButton;

        public void ShowSummary(List<MapLevelVictoryConditionBase> victoryConditions, MetaProgressUpdateContext context)
        {
            _victoryConditions = victoryConditions;
            _context = context;
            _pendingUnlocks = new List<string>();
            if (context.newlyPassedChallenges != null)
            {
                foreach (MetaChallengeBase challenge in context.newlyPassedChallenges)
                {
                    _pendingUnlocks.Add(challenge.DisplayName);
                }
            }
            foreach (MapLevelReward reward in GameManager.Instance.Map.GetMetaRewards())
            {
                _pendingUnlocks.Add($"{reward.Description} - {reward.Reward.Name}");
            }
            _pendingDifficultyUnlock = context.HasUnlockedNewStage();
            _claimedUnlocks.Clear();
            _difficultyClaimed = false;

            GameManager.Instance.UIManager.ForcePause();
            GameManager.Instance.UIManager.Block();
            
            RefreshUI();
        }

        private void RefreshUI()
        {
            CleanUp();
            base.Show();
            
            AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().h1("Game Over");
            List<MapLevelVictoryConditionBase> failedVictoryConditions = _victoryConditions.FindAll(condition => condition.GetFinalState() == VictoryConditionState.Failed);
            if (failedVictoryConditions.Count > 0)
            {
                AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().h2("Failed Victory Conditions");
                foreach (MapLevelVictoryConditionBase condition in failedVictoryConditions)
                {
                    AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text =
                        $" - {condition.GetDescription()}";
                }
            }

            bool hasPending = _pendingDifficultyUnlock || _pendingUnlocks.Count > 0;

            if (_difficultyClaimed)
            {
                string label = "New Difficulty Unlocked: " + _context.currentStage;
                AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().h2(label);
            }
            
            if (_claimedUnlocks.Count > 0)
            {
                AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().h2("Unlocked");
                foreach (string unlock in _claimedUnlocks)
                {
                    AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text =
                        $" - {unlock}";
                }
            }

            if (hasPending)
            {
                string buttonText = "Claim Reward";
                if (_pendingDifficultyUnlock)
                {
                    buttonText = $"Difficulty Unlocked: {_context.currentStage}";
                }
                else if (_pendingUnlocks.Count > 0)
                {
                    buttonText = $"Claim Reward: {_pendingUnlocks[0]}";
                }

                _lastClaimButton = AddButton(buttonText, () =>
                {
                    bool claimed = false;
                    if (_pendingDifficultyUnlock)
                    {
                        _pendingDifficultyUnlock = false;
                        _difficultyClaimed = true;
                        claimed = true;
                    }
                    else if (_pendingUnlocks.Count > 0)
                    {
                        string unlocked = _pendingUnlocks[0];
                        _pendingUnlocks.RemoveAt(0);
                        _claimedUnlocks.Add(unlocked);
                        claimed = true;
                    }

                    if (claimed)
                    {
                        TriggerRewardExplosion();
                        RefreshUI();
                    }
                });
            }
            else
            {
                if (GameManager.Instance.Map.GetMetaRewards().Count > 0) // _context.newlyPassedChallenges.Count > 0)}
                {
                    AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text =
                        "You have earned new `Vested Shares`. Spend them to unlock bonuses for future runs.";
                    AddButton("Allocate Vested Shares", () =>
                    {
                        GameManager.Instance.ShowMainMenu();
                        GameManager.Instance.UIManager.mainMenu.Close(true);
                        GameManager.Instance.UIManager.metaUnlockMapPanel.Show();
                    });
                }
                else {
                    AddButton("Start Over", () => { GameManager.Instance.StartNewGame(GameManager.Instance.Map.difficulty); });
                    AddButton("Main Menu", () => { GameManager.Instance.ShowSaveSlotDetailPanel(); });
                }
                AddButton("Wishlist now!", () => Application.OpenURL("https://store.steampowered.com/app/4567430/Tech_Debt/")); 
                AddButton("Give Feedback", () => Application.OpenURL("https://forms.gle/NRRbLNhtoaJQrRzp9"));  
            }
        }

        private void TriggerRewardExplosion()
        {
            List<string> particleSprites = new List<string>()
            {
                "ParticleMicroChip",
                "ParticlePowerSource",
                "ParticleChip",
                "ParticleHardDrive",
                "ParticleRam1"
            };

            if (_lastClaimButton == null || _lastClaimButton.button == null) return;
            Vector3 worldSpawnPos = _lastClaimButton.button.transform.position;

            // Parent to UIManager so they are siblings to the panels
            for (int i = 0; i < 20; i++)
            {
                string spriteString = particleSprites[Random.Range(0, particleSprites.Count)];
                Sprite sprite = GameManager.Instance.SpriteManager.GetSprite(spriteString);
                
                GameObject particleGO = GameManager.Instance.prefabManager.Create("UIScreenParticle", worldSpawnPos, GameManager.Instance.UIManager.transform);
                UIScreenParticle particle = particleGO.GetComponent<UIScreenParticle>();
                
                // Position at the button's world position
                particle.transform.position = worldSpawnPos;

                // Render in front of panel: Set index to panel index + 1
                int panelIndex = transform.GetSiblingIndex();
                particle.transform.SetSiblingIndex(panelIndex + 1);
                
                float angle = Random.Range(45f, 135f) * Mathf.Deg2Rad;
                float force = Random.Range(600f, 1200f);
                Vector2 velocity = new Vector2(Mathf.Cos(angle) * force, Mathf.Sin(angle) * force);
                
                particle.InitParabolic(sprite, velocity);
            }
        }
    }
}
