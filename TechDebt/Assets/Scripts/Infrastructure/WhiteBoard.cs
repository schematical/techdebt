using System.Collections.Generic;
using NPCs;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Infrastructure
{
    public class WhiteBoard: InfrastructureInstance
    {
        public override void OnPointerClick(PointerEventData eventData)
        {
            List<ReleaseBase> releases = GameManager.Instance.GetOpenReleases();
            if (releases.Count > 0)
            {
                GameManager.Instance.UIManager.ShowAlert("You already have open releases that need to be finished first.");
                return;
            }
            GameManager.Instance.UIManager.MultiSelectPanel.Clear();
         
             Sprite sprite = GameManager.Instance.prefabManager.GetPrefab("Manual").GetComponent<SpriteRenderer>().sprite;
         
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
              
               if (
                   existingModifierBase == null
                   
               ) {
                   /*if (GameManager.Instance.Modifiers.Modifiers.Count < Stats.GetStatValue(StatType.NPC_ModifierSlots))
                   {*/
                       modifiers.Add(modifierBase);
                       GameManager.Instance.UIManager.MultiSelectPanel.Add(
                               modifierBase.Id,
                               sprite,
                               modifierBase.GetDisplayText()
                           )
                           .OnClick((string id) =>
                           {
                               ReleaseBase releaseBase = new ReleaseBase()  
                               {
                                   ServiceId = "monolith",
                                   Version = ReleaseBase.IncrGlobalVersion(),
                               };
                               releaseBase.RewardModifier = modifierBase;
                               GameManager.Instance.Releases.Add(releaseBase);
                               CodeTask codeTask = new CodeTask(releaseBase);
                               GameManager.Instance.AddTask(codeTask);
                               GameManager.Instance.UIManager.MultiSelectPanel.Clear();
                           });
                   //}
               }
               else
               {
                   modifiers.Add(existingModifierBase);
                   GameManager.Instance.UIManager.MultiSelectPanel.Add(
                           existingModifierBase.Id, 
                           sprite, 
                           existingModifierBase.GetDisplayText(1) 
                       )
                       .OnClick((string id) =>
                       {
                           
                           ReleaseBase releaseBase = new ReleaseBase()  
                           {
                               ServiceId = "monolith",
                               Version = ReleaseBase.IncrGlobalVersion(),
                           };;
                           releaseBase.RewardModifier = modifierBase;
                           GameManager.Instance.Releases.Add(releaseBase);
                           CodeTask codeTask = new CodeTask(releaseBase);
                           GameManager.Instance.AddTask(codeTask);
                           GameManager.Instance.UIManager.MultiSelectPanel.Clear();
                       });
               }
              
           }
            
        }
    }
}