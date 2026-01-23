// NPCDevOps.cs

using System;
using System.Collections.Generic;
using Events;
using NPCs;
using Stats;
using UnityEngine;
using UnityEngine.EventSystems;

public class NPCDevOps : NPCBase, IPointerClickHandler
{
    public NPCDevOpsData Data { get; private set; }
    public ModifierCollection Modifiers = new ModifierCollection(); // List<ModifierBase> Traits { get; private set; } = new List<ModifierBase>();
    public int level = 1;
    public int lastDisplayXP = 0;
    public float currentXP = 0;

    public void Initialize(NPCDevOpsData data)
    {
        Data = data;
        gameObject.name = $"NPCDevOps_{Data.Name}";
        base.Initialize();
        Stats.Add(new StatData(StatType.NPC_ModifierSlots, 1));
        Stats.Add(new StatData(StatType.NPC_XPSpeed, 1));
        Stats.Add(new StatData(StatType.NPC_InfoSec, 0.75f));
        Stats.Add(new StatData(StatType.NPC_CodeSpeed, 1));
        Stats.Add(new StatData(StatType.NPC_CodeQuality, .75f));
        
    }
    

    public override void AddXP(float amount = 1)
    {
        if (
            GameManager.Instance.Tutorial != null &&
            !GameManager.Instance.Tutorial.NPCsCanGetXP
        )
        {
            return;
        }
        float adjustedAmount = Stats.GetStatValue(StatType.NPC_XPSpeed) * amount;
       currentXP += adjustedAmount;
       if (Math.Floor(currentXP) != lastDisplayXP)
       {
           GameManager.Instance.FloatingTextFactory.ShowText($"{Math.Ceiling(amount)} XP",
               transform.position); //  + new Vector3(0, 1, 3));
           lastDisplayXP = (int)Math.Floor(currentXP);
       }

       int nextLevel = (int) Math.Round(20 * Math.Pow(1.5f, level));
       if (currentXP >= nextLevel)
       {
           level ++;
           currentXP = 0;
           lastDisplayXP = 0;
       

         
           GameManager.Instance.UIManager.MultiSelectPanel.Display(
               "One of your team has leveled up!",
               "Choose a bonus to be applied to your DevOps Engineer"
           );
           int saftyCheck = 0;
           List<ModifierBase> traits = new List<ModifierBase>();
           int optionCount = 3;
           if (Modifiers.Modifiers.Count >= Stats.GetStatValue(StatType.NPC_ModifierSlots))
           {
               optionCount = (int)Stats.GetStatValue(StatType.NPC_ModifierSlots);
           }
           while (
               saftyCheck < 20 &&
               traits.Count < optionCount
           )
           {
               saftyCheck++;
               ModifierBase modifierBase = MetaGameManager.GetRandomModifier(ModifierBase.ModifierGroup.NPC);
               if (traits.Find((t) => t.Id == modifierBase.Id) != null)
               {
                   continue;
               }

               GameObject spriteGO = GameManager.Instance.prefabManager.GetPrefab(modifierBase.IconPrefab);
               if (spriteGO == null)
               {
                   throw new SystemException($"modifierBase.IconPrefab: {modifierBase.IconPrefab} not found");
               }
               Sprite sprite = spriteGO.GetComponent<SpriteRenderer>().sprite;
               ModifierBase existingModifierBase = Modifiers.Modifiers.Find((t) => t.Id == modifierBase.Id);
               // Debug.Log($"TraitTest: {existingTrait != null} && {Traits.Count} < {Stats.GetStatValue(StatType.NPC_TraitSlots)}");
               if (
                   existingModifierBase == null
                   
               ) {
                   if (Modifiers.Modifiers.Count < Stats.GetStatValue(StatType.NPC_ModifierSlots))
                   {
                       traits.Add(modifierBase);
                       
                       GameManager.Instance.UIManager.MultiSelectPanel.Add(
                               modifierBase.Id,
                               sprite,
                               modifierBase.GetDisplayText()
                           )
                           .OnClick((string id) =>
                           {
                               AddModifier(modifierBase);
                               GameManager.Instance.UIManager.MultiSelectPanel.Clear();
                           });
                   }
               }
               else
               {
                   traits.Add(existingModifierBase);
                   GameManager.Instance.UIManager.MultiSelectPanel.Add(
                           existingModifierBase.Id, 
                           sprite, 
                           existingModifierBase.GetDisplayText(1) 
                       )
                       .OnClick((string id) =>
                       {
                           existingModifierBase.LevelUp();
                           GameManager.Instance.UIManager.MultiSelectPanel.Clear();
                       });
               }

              
           }
       }
    }
    public void AddModifier(ModifierBase modifierBase)
    {
        Modifiers.Modifiers.Add(modifierBase);
        if (GameManager.Instance.Tutorial != null)
        {
            GameManager.Instance.Tutorial.Check(TutorialEvent.TutorialCheck.NPC_AddTrait);
        }   
    }
    
    public override bool CanAssignTask(NPCTask task)
    {
        return task.Role == NPCTask.TaskRole.DevOps;
    }

    public float GetResearchPointsPerSecond(Technology technology)
    {
        // This could be influenced by the NPC's skills or the technology type
        return 1f;
    }
    
    public void OnPlanPhaseStart()
    {
        if (CurrentTask != null)
        {
            CurrentTask.Unassign();
        }
        StopMovement();
        CurrentState = State.Idle;
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        GameManager.Instance.UIManager.ShowNPCDetail(this);
    }

    public float GetBuildSpeed()
    {
        float npcBuildSpeed = 1;
        if (Stats.Get(StatType.NPC_DevOpsSpeed) != null)
        {
            npcBuildSpeed = Stats.GetStatValue(StatType.NPC_DevOpsSpeed);
        }

        return npcBuildSpeed;
    }
}
