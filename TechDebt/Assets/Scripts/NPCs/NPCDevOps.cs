// NPCDevOps.cs

using System;
using System.Collections.Generic;
using System.Linq;
using DefaultNamespace;
using DefaultNamespace.Rewards;
using Events;
using NPCs;
using Stats;
using UnityEngine;
using UnityEngine.EventSystems;

public class NPCDevOps : NPCAnimatedBiped
{
    public NPCDevOpsData Data { get; private set; }

    public ModifierCollection
        Modifiers =
            new ModifierCollection(); // List<ModifierBase> Traits { get; private set; } = new List<ModifierBase>();

    public int level = 1;
    public int leveledUpTo = 1;
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
        Stats.Add(new StatData(StatType.NPC_CodeSpeed, 0.5f));
        Stats.Add(new StatData(StatType.NPC_CodeQuality, .1f));
        Stats.Add(new StatData(StatType.NPC_DevOpsQuality, 1f));
        Stats.Add(new StatData(StatType.NPC_DevOpsSpeed, 1f));
        Stats.Add(new StatData(StatType.NPC_ResearchSpeed, 1f));
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
            GameManager.Instance.FloatingTextFactory.ShowText($"{Math.Round(currentXP - lastDisplayXP)} XP",
                transform.position); //  + new Vector3(0, 1, 3));
            lastDisplayXP = (int)Math.Floor(currentXP);
        }

        int nextLevelXP = (int)Math.Round(60 * Math.Pow(1.5f, level));
        if (currentXP >= nextLevelXP)
        {
       
            currentXP = currentXP - nextLevelXP;
            lastDisplayXP = 0;
            MarkReadyForLevelUp();
           
        }
    }

    protected void MarkReadyForLevelUp()
    {
        leveledUpTo++;
        ShowAttentionIcon(() =>
        {
            HideAttentionIcon();
            LevelUp();
               
               
        });
    }
    protected void LevelUp()
    {
        level++;

        GameManager.Instance.UIManager.multiSelectPanel.Display(
            "One of your team has leveled up!",
            "Choose a bonus to be applied to your DevOps Engineer"
        );
        int saftyCheck = 0;
        List<RewardBase> traits = new List<RewardBase>();
        int optionCount = 3;
        if (Modifiers.Rewards.Count >= Stats.GetStatValue(StatType.NPC_ModifierSlots))
        {
            optionCount = (int)Stats.GetStatValue(StatType.NPC_ModifierSlots);
        }

        while (
            saftyCheck < 20 &&
            traits.Count < optionCount
        )
        {
            saftyCheck++;
            RewardBase modifierBase = MetaGameManager.GetRandomModifier(RewardBase.RewardGroup.NPC);
            if (traits.Find((t) => t.Id == modifierBase.Id) != null)
            {
                continue;
            }

            Sprite sprite = modifierBase.GetSprite();
            

         
  
            Rarity rarity = RarityHelper.GetRandomRarity();
            Sprite spriteOut = RarityHelper.PaintIcon(rarity, sprite); // spriteRenderer.sprite;
            
            RewardBase existingModifierBase = Modifiers.Rewards.Find((t) => t.Id == modifierBase.Id);
            // Debug.Log($"TraitTest: {existingModifierBase != null} && {Modifiers.Modifiers.Count} < {Stats.GetStatValue(StatType.NPC_ModifierSlots)}");
 
            if (
                existingModifierBase == null
            )
            {
                if (Modifiers.Rewards.Count < Stats.GetStatValue(StatType.NPC_ModifierSlots))
                {
                    traits.Add(modifierBase);

                    GameManager.Instance.UIManager.multiSelectPanel.Add(
                        modifierBase.Id,
                        spriteOut,
                        modifierBase.Name,
                        $"New - {rarity}" //  - {modifierBase.GetNextLevelUpDisplayText(rarity)}"
                        )
                        .OnSelect((string id) =>
                        {
                            try
                            {
                                if (modifierBase is NPCStatModifierReward)
                                {
                                    (modifierBase as NPCStatModifierReward).SetTarget(this);
                                }
                                AddModifier(modifierBase);
                                modifierBase.Apply();
                              

                                GameManager.Instance.UIManager.multiSelectPanel.Close();
                            }
                            catch (Exception e)
                            {
                                Debug.LogError("NPC Level Up Exception Start");
                                Debug.LogException(e);
                                Debug.LogError("NPC Level Up Exception End");
                                throw e;
                            }
                        });
                }
            }
            else
            {
                traits.Add(modifierBase);
            
                GameManager.Instance.UIManager.multiSelectPanel.Add(
                        existingModifierBase.Id,
                        sprite,
                        existingModifierBase.Name,
                        $"{rarity}" // - {existingModifierBase.GetNextLevelUpDisplayText(rarity)}"
                    )
                    .OnSelect((string id) =>
                    {
                        try {
                            if (modifierBase is LeveledRewardBase)
                            {
                                (modifierBase as LeveledRewardBase).LevelUp(rarity);
                            }
                            GameManager.Instance.UIManager.multiSelectPanel.Close();
                        }catch (Exception e) {
                            Debug.LogError("NPC Level Up Exception2 Start");
                            Debug.LogException(e);
                            Debug.LogError("NPC Level Up Exception2 End");
                        }
                    });
            }
        }
    }

    public void AddModifier(RewardBase modifierBase)
    {
        Modifiers.Rewards.Add(modifierBase);
        /*if (GameManager.Instance.Tutorial != null)
        {
            GameManager.Instance.Tutorial.Check(TutorialEvent.TutorialCheck.NPC_AddTrait);
        }*/
      
    }

    public override bool CanAssignTask(NPCTask task)
    {
        return task.Role == NPCTask.TaskRole.DevOps;
    }

    public float GetResearchPointsPerSecond(Technology technology)
    {
        // This could be influenced by the NPC's skills or the technology type
        return Stats.GetStatValue(StatType.NPC_ResearchSpeed);
    }

  
    public override void OnPlayPhaseStart()
    {
        base.OnPlayPhaseStart();
        if (level < leveledUpTo)
        {
            ShowAttentionIcon(() =>
            {
                HideAttentionIcon();
                LevelUp();
               
               
            });
        }
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

    
   
    public override Vector3 GetHomePoint()
    {
        return GameManager.Instance.GetInfrastructureInstanceByID("desk").transform.position;
    }
    public override void OnLeftClick(PointerEventData eventData)
    {
        if (level < leveledUpTo)
        {
            HideAttentionIcon();
            LevelUp();
            return;
        }
        base.OnLeftClick(eventData);
    }
}