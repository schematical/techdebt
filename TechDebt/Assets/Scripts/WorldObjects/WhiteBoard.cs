using System.Collections.Generic;
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
            List<ModifierBase> modifiers = new List<ModifierBase>();
            int optionCount = 3;
            /*if (GameManager.Instance.Modifiers.Modifiers.Count >= GameManager.Stats.GetStatValue(StatType.NPC_ModifierSlots))
            {
                optionCount = (int)Stats.GetStatValue(StatType.NPC_ModifierSlots);
            }*/
            while (
                saftyCheck < 20 &&
                modifiers.Count < optionCount
            )
            {
                saftyCheck++;
                ModifierBase modifierBase = MetaGameManager.GetRandomModifier(ModifierBase.ModifierGroup.Release);
                if (modifiers.Find((t) => t.Id == modifierBase.Id) != null)
                {
                    continue;
                }

                ModifierBase existingModifierBase =
                    GameManager.Instance.Modifiers.Modifiers.Find((t) => t.Id == modifierBase.Id);
                Sprite sprite = GameManager.Instance.SpriteManager.GetSprite(modifierBase.IconSpriteId);
                if (
                    existingModifierBase == null
                )
                {
                    /*if (GameManager.Instance.Modifiers.Modifiers.Count < Stats.GetStatValue(StatType.NPC_ModifierSlots))
                    {*/
                    modifiers.Add(modifierBase);
                    UIMultiSelectOption option = GameManager.Instance.UIManager.multiSelectPanel.Add(
                        modifierBase.Id,
                        sprite,
                        modifierBase.GetTitle(),
                        modifierBase.GetNextLevelUpDisplayText(Rarity.Common)
                    );
                    option.OnSelect((string id) =>
                    {
                        ReleaseBase releaseBase = new ReleaseBase(ReleaseBase.IncrGlobalVersion(), modifierBase);
                        GameManager.Instance.Releases.Add(releaseBase);
                        CodeTask codeTask = new CodeTask(releaseBase);
                        GameManager.Instance.AddTask(codeTask);
                        GameManager.Instance.UIManager.multiSelectPanel.Close();
                        HideAttentionIcon();
                    });
                    option.OnPreview((string id) =>
                    {
                        modifierBase.ShowPreviewUI();
                    });
                }
                else
                {
                    modifiers.Add(existingModifierBase);
                    GameManager.Instance.UIManager.multiSelectPanel.Add(
                            existingModifierBase.Id,
                            sprite,
                            existingModifierBase.GetTitle(),
                            existingModifierBase.GetNextLevelUpDisplayText(Rarity.Common)
                        )
                        .OnSelect((string id) =>
                        {
                            ReleaseBase releaseBase =
                                new ReleaseBase(ReleaseBase.IncrGlobalVersion(), existingModifierBase);
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