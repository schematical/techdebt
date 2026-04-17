using System.Collections.Generic;
using System.Linq;
using DefaultNamespace.Rewards;
using UnityEngine;

namespace UI
{
    public class UIReleaseHistoryPanel: UIPanel
    {
        UIPanelButton planReleaseButton;
        UIPanelLineProgressBar planReleaseProgressBar;
       
        public override void Show()
        {
            base.Show();
            GameManager.OnReleaseChanged += HandleReleaseChanged;
            planReleaseButton = AddLine<UIPanelButton>();
            planReleaseButton.text.text = "Plan Next Release";
            planReleaseButton.button.onClick.AddListener(() => OnPlanReleaseClick());
            
            planReleaseProgressBar = AddLine<UIPanelLineProgressBar>();
            
            
            List<ReleaseBase> releases = GameManager.Instance.Releases.ToList();
            releases.Reverse();
            foreach (ReleaseBase release in releases)
            {
                UIPanelLine line = AddLine<UIPanelLine>();
                line.SetId(release.GetVersionString());
                UIPanelLineSectionText textSection = line.Add<UIPanelLineSectionText>();
                textSection.SetId("main");
                textSection.h2(release.GetVersionString());
                line.SetExpandable((line =>
                {
                   release.Render(line);
                }));  
            }

            Refresh();
        }

        public override void Close(bool forceClose = false)
        {
            base.Close(forceClose);
            GameManager.OnReleaseChanged -= HandleReleaseChanged;
        }

        public void OnPlanReleaseClick()
        {
            List<ReleaseBase> releases = GameManager.Instance.GetOpenReleases();
            if (releases.Count > 0)
            {
                ReleaseBase releaseBase = releases.Find((release =>
                    release.State == ReleaseBase.ReleaseState.DeploymentRewardReady));
                if (releaseBase != null)
                {
                    // GameManager.Instance.UIManager.rewardPanel.Show(releaseBase);
                    List<ApplicationServer> targets = releaseBase.GetAllReleaseTargets();
                    targets[0].ZoomTo();
                    GameManager.Instance.UIManager.rewardPanel.OnOpenClick();
                    return;
                }

               Debug.LogError("UIReleaseHistoryPanel::OnPlanReleaseClick This should never get hit");
                return;
            }

            
            GameManager.Instance.UIManager.multiSelectPanel.Close();


            GameManager.Instance.UIManager.multiSelectPanel.Display(
                "Select a release focus.",
                "Your team will start working towards a feature that will reward you once deployed."
            );
            GameManager.Instance.UIManager.multiSelectPanel.OnReRoll(() =>
            {
                GameManager.Instance.IncrStat(StatType.Global_ReRolls, -1);
                OnPlanReleaseClick();
            });
            
            int saftyCheck = 0;
            List<RewardBase> modifiers = new List<RewardBase>();
            int optionCount = 3;
            List<RewardBase> specialOptions = GameManager.Instance.Map.GetCurrentLevel().GetSpecialReleaseRewards();
            while (
                saftyCheck < 20 &&
                modifiers.Count < optionCount
            )
            {
                saftyCheck++;
                RewardBase modifierBase = null;
                if (specialOptions.Count > 0)
                {
                    modifierBase = specialOptions[0];
                    specialOptions.RemoveAt(0);
                }
                else
                {
                    modifierBase = MetaGameManager.GetRandomModifier(RewardBase.RewardGroup.Release);
                }

                if (modifiers.Find((t) => t.Id == modifierBase.Id) != null)
                {
                    continue;
                }
                Sprite sprite = modifierBase.GetSprite();
                RewardBase existingRewardBase = GameManager.Instance.Rewards.Rewards.Find((t) => t.Id == modifierBase.Id);
                

                if (
                    existingRewardBase == null
                )
                {
                    modifiers.Add(modifierBase);
                    UIMultiSelectOption option = GameManager.Instance.UIManager.multiSelectPanel.Add(
                        modifierBase.Id,
                        sprite,
                        modifierBase.GetTitle(),
                        modifierBase.GetDescription()
                    );
                    option.MarkBanisable();
                    option.OnInteract((type, id) =>
                    {
                        Debug.Log($"OnInteract: {type}: {id}");
                        if (type == UIMultiSelectOption.InteractionType.Select)
                        {
                            ReleaseBase releaseBase = new ReleaseBase(ReleaseBase.IncrGlobalVersion(), modifierBase);
                            GameManager.Instance.Releases.Add(releaseBase);
                            CodeTask codeTask = new CodeTask(releaseBase);
                            GameManager.Instance.AddTask(codeTask);
                            GameManager.Instance.UIManager.multiSelectPanel.Close();
                            GameManager.Instance.UIManager.CloseSideBars();
                            GameManager.Instance.GetInfrastructureInstanceByID("whiteboard").HideAttentionIcon();
                        }
                        else if (type == UIMultiSelectOption.InteractionType.Banish)
                        {
                            GameManager.Instance.IncrStat(StatType.Global_Banish, -1);
                            GameManager.Instance.Map.BanishedRewardIds.Add(id);
                            OnPlanReleaseClick();
                        }
                    });
                }
                else
                {
                    modifiers.Add(existingRewardBase);
                    GameManager.Instance.UIManager.multiSelectPanel.Add(
                            existingRewardBase.Id,
                            sprite,
                            existingRewardBase.GetTitle(),
                            existingRewardBase.GetDescription()
                        )
                        .MarkBanisable()
                        .OnInteract((type, id) =>
                        {
                            if (type == UIMultiSelectOption.InteractionType.Select)
                            {
                                ReleaseBase releaseBase =
                                    new ReleaseBase(ReleaseBase.IncrGlobalVersion(), existingRewardBase);
                                GameManager.Instance.Releases.Add(releaseBase);
                                CodeTask codeTask = new CodeTask(releaseBase);
                                GameManager.Instance.AddTask(codeTask);
                                GameManager.Instance.UIManager.multiSelectPanel.Close();
                                GameManager.Instance.GetInfrastructureInstanceByID("whiteboard").HideAttentionIcon();
                            }
                            else if (type == UIMultiSelectOption.InteractionType.Banish)
                            {
                                GameManager.Instance.IncrStat(StatType.Global_Banish, -1);
                                GameManager.Instance.Map.BanishedRewardIds.Add(id);
                                OnPlanReleaseClick();
                            }
                        });
                }
            }
        }

        private void HandleReleaseChanged(ReleaseBase release, ReleaseBase.ReleaseState prevState)
        {
            Refresh();
        }

        private void Refresh()
        {
            List<ReleaseBase> openReleases = GameManager.Instance.GetOpenReleases();
            bool hasOpenReleases = openReleases.Count > 0;
            planReleaseButton.gameObject.SetActive(!hasOpenReleases);
            planReleaseProgressBar.gameObject.SetActive(hasOpenReleases);
            if (hasOpenReleases)
            {
                planReleaseProgressBar.SetProgress(openReleases[0].GetProgress());
            }

            List<ReleaseBase> releases = GameManager.Instance.Releases.ToList();
            releases.Reverse();

            foreach (ReleaseBase release in releases)
            {
                UIPanelLine line = GetLineById(release.GetVersionString());
            }
        }
    }
}