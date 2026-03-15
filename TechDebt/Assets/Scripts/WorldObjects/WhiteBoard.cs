using System.Collections.Generic;
using DefaultNamespace.Rewards;
using NPCs;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Infrastructure
{
    public class WhiteBoard : InfrastructureInstance
    {
        public override void Initialize()
        {
            base.Initialize();
            attentionIconColor = Color.green;
            if (IsActive())
            {
                ShowAttentionIcon();
            }
        }

        public override void OnLeftClick(PointerEventData eventData)
        {
            if (!IsActive())
            {
                base.OnLeftClick(eventData);
                return;
            }

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

                GameManager.Instance.UIManager.ShowAlert(
                    "You already have open releases that need to be finished first.");
                return;
            }

            /*if (GameManager.Instance.HasOpenBugs())
            {
                GameManager.Instance.UIManager.ShowAlert("You must debug the bugs introduced in the last release first.");
                return;
            }*/
            GameManager.Instance.UIManager.multiSelectPanel.Close();


            GameManager.Instance.UIManager.multiSelectPanel.Display(
                "Select a release focus.",
                "Your team will start working towards a feature that will reward you once deployed."
            );
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
                    option.OnSelect((string id) =>
                    {
                        ReleaseBase releaseBase = new ReleaseBase(ReleaseBase.IncrGlobalVersion(), modifierBase);
                        GameManager.Instance.Releases.Add(releaseBase);
                        CodeTask codeTask = new CodeTask(releaseBase);
                        GameManager.Instance.AddTask(codeTask);
                        GameManager.Instance.UIManager.multiSelectPanel.Close();
                        GameManager.Instance.UIManager.CloseSideBars();
                        HideAttentionIcon();
                    });
                    option.OnPreview((string id) =>
                    {
                        // modifierBase.ShowPreviewUI();
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
                        .OnSelect((string id) =>
                        {
                            ReleaseBase releaseBase =
                                new ReleaseBase(ReleaseBase.IncrGlobalVersion(), existingRewardBase);
                            GameManager.Instance.Releases.Add(releaseBase);
                            CodeTask codeTask = new CodeTask(releaseBase);
                            GameManager.Instance.AddTask(codeTask);
                            GameManager.Instance.UIManager.multiSelectPanel.Close();
                            HideAttentionIcon();
                        });
                }
            }
        }
    }
}