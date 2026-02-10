using System.Collections.Generic;
using NPCs;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Infrastructure
{
    public class WhiteBoard: InfrastructureInstance
    {
        public override void Initialize()
        {
            base.Initialize();
            attentionIconColor = Color.green;
            ShowAttentionIcon();
        }

        public override void OnLeftClick(PointerEventData eventData)
        {
            
            List<ReleaseBase> releases = GameManager.Instance.GetOpenReleases();
            if (releases.Count > 0)
            {
                ReleaseBase releaseBase = releases.Find((release => release.State == ReleaseBase.ReleaseState.DeploymentRewardReady));
                if (releaseBase != null)
                {
                    // GameManager.Instance.UIManager.rewardPanel.Show(releaseBase);
                    List<ApplicationServer> targets = releaseBase.GetAllReleaseTargets();
                    targets[0].ZoomTo();
                    GameManager.Instance.UIManager.rewardPanel.OnOpenClick();
                    return;
                }
                GameManager.Instance.UIManager.ShowAlert("You already have open releases that need to be finished first.");
                return;
            }

            /*if (GameManager.Instance.HasOpenBugs())
            {
                GameManager.Instance.UIManager.ShowAlert("You must debug the bugs introduced in the last release first.");
                return;
            }*/
            GameManager.Instance.UIManager.MultiSelectPanel.Close();
         

         
           GameManager.Instance.UIManager.MultiSelectPanel.Display(
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
               ModifierBase existingModifierBase = GameManager.Instance.Modifiers.Modifiers.Find((t) => t.Id == modifierBase.Id);
               Sprite sprite = GameManager.Instance.prefabManager.GetPrefab(modifierBase.IconPrefab).GetComponent<SpriteRenderer>().sprite;
               if (
                   existingModifierBase == null
               ) {
                   /*if (GameManager.Instance.Modifiers.Modifiers.Count < Stats.GetStatValue(StatType.NPC_ModifierSlots))
                   {*/
                       modifiers.Add(modifierBase);
                       GameManager.Instance.UIManager.MultiSelectPanel.Add(
                               modifierBase.Id,
                               sprite,
                               modifierBase.GetTitle(),
                               modifierBase.GetNextLevelUpDisplayText(Rarity.Common)
                           )
                           .OnClick((string id) =>
                           {
                               ReleaseBase releaseBase = new ReleaseBase(ReleaseBase.IncrGlobalVersion(), modifierBase);
                               GameManager.Instance.Releases.Add(releaseBase);
                               CodeTask codeTask = new CodeTask(releaseBase);
                               GameManager.Instance.AddTask(codeTask);
                               GameManager.Instance.UIManager.MultiSelectPanel.Close();
                               HideAttentionIcon();
                           });
                   //}
               }
               else
               {
                   modifiers.Add(existingModifierBase);
                   GameManager.Instance.UIManager.MultiSelectPanel.Add(
                           existingModifierBase.Id, 
                           sprite, 
                           existingModifierBase.GetTitle(),
                           existingModifierBase.GetNextLevelUpDisplayText(Rarity.Common) 
                       )
                       .OnClick((string id) =>
                       {
                           
                           ReleaseBase releaseBase = new ReleaseBase(ReleaseBase.IncrGlobalVersion(), existingModifierBase);
                           GameManager.Instance.Releases.Add(releaseBase);
                           CodeTask codeTask = new CodeTask(releaseBase);
                           GameManager.Instance.AddTask(codeTask);
                           GameManager.Instance.UIManager.MultiSelectPanel.Close();
                           HideAttentionIcon();
                       });
               }
              
           }
            
        }
    }
}