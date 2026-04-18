// NPCDevOps.cs

using System;
using System.Collections.Generic;
using System.Linq;
using DefaultNamespace;
using DefaultNamespace.Rewards;
using Tutorial;
using NPCs;
using Stats;
using UI;
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
        SetExpression(FacialExpression.Default);
        Stats.Add(new StatData(StatType.NPC_ModifierSlots, 5));
        Stats.Add(new StatData(StatType.NPC_XPSpeed, 1));
        Stats.Add(new StatData(StatType.NPC_InfoSec, 0.75f));
        Stats.Add(new StatData(StatType.NPC_CodeSpeed, 0.5f));
        Stats.Add(new StatData(StatType.NPC_CodeQuality, .1f));
        Stats.Add(new StatData(StatType.NPC_DevOpsQuality, 1f));
        Stats.Add(new StatData(StatType.NPC_DevOpsSpeed, 1f));
        Stats.Add(new StatData(StatType.NPC_ResearchSpeed, 1f));
        Stats.Add(new StatData(StatType.NPC_FixSpeed, 1f));
        Stats.Add(new StatData(StatType.NPC_Release_TechDebt, 1f));
        currentXP = 0;
        lastDisplayXP = 0;
        level = 1;
        leveledUpTo = 1;
    }

    public override void OnPlanPhaseStart()
    {
        base.OnPlanPhaseStart();
        transform.position = GameManager.Instance.GetInfrastructureInstanceByID("door").transform.position + new Vector3(-0.1f,-0.1f, -0.1f);
        gameObject.SetActive(false);
    }

    public override void AddXP(float amount = 1)
    {
        float adjustedAmount = Stats.GetStatValue(StatType.NPC_XPSpeed) * amount;
        currentXP += adjustedAmount;
        if (Math.Floor(currentXP) != lastDisplayXP)
        {
            GameManager.Instance.FloatingTextFactory.ShowText($"{Math.Round(currentXP - lastDisplayXP)} XP",
                transform.position); //  + new Vector3(0, 1, 3));
            lastDisplayXP = (int)Math.Floor(currentXP);
        }

        int nextLevelXP = (int)Math.Round(30 * Math.Pow(1.5f, level));
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
        if (GameManager.Instance.TutorialManager != null)
        {
            GameManager.Instance.TutorialManager.Trigger(TutorialStepId.NPC_LevelUp_Pending);
        }
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
            "Choose a bonus to be applied to your engineer"
        );
        GameManager.Instance.UIManager.multiSelectPanel.OnReRoll(() =>
        {
            GameManager.Instance.IncrStat(StatType.Global_ReRolls, -1);
            LevelUp();
        });

        int saftyCheck = 0;
        List<RewardBase> traits = new List<RewardBase>();
        int optionCount = 3;
        if (Modifiers.Rewards.Count >= Stats.GetStatValue(StatType.NPC_ModifierSlots))
        {
            optionCount = (int)Stats.GetStatValue(StatType.NPC_ModifierSlots);
        }

        System.Action<UIMultiSelectOption, RewardBase, Rarity> setupOption = null;
        setupOption = (opt, mod, rar) =>
        {
            Sprite sprite = mod.GetSprite();
            Sprite spriteOut = RarityHelper.PaintIcon(rar, sprite);
            RewardBase existing = Modifiers.Rewards.Find((t) => t.Id == mod.Id);

            opt.Initialize(GameManager.Instance.UIManager.multiSelectPanel, mod.Id, spriteOut, mod.Name, mod.GetDescription());
            opt.MarkBanisable();
            opt.OnInteract((type, currentId) =>
            {
                if (type == UIMultiSelectOption.InteractionType.Select)
                {
                
                    if (existing == null)
                    {
                        if (mod is NPCStatModifierReward statMod) statMod.SetTarget(this);
                        AddModifier(mod);
                        mod.Apply();
                    }
                    else if (existing is LeveledRewardBase leveled)
                    {
                        leveled.LevelUp(rar);
                    }

                    if (GameManager.Instance.TutorialManager != null)
                    {
                        GameManager.Instance.TutorialManager.Trigger(TutorialStepId.NPC_LevelUp_Completed);
                    }
                    GameManager.Instance.UIManager.multiSelectPanel.Close();
                
                }
                else if (type == UIMultiSelectOption.InteractionType.Banish)
                {
                    GameManager.Instance.IncrStat(StatType.Global_Banish, -1);
                    GameManager.Instance.Map.BanishedRewardIds.Add(currentId);
                    traits.Remove(mod);

                    RewardBase replacement = null;
                    int safety = 0;
                    while (safety < 50)
                    {
                        safety++;
                        replacement = MetaGameManager.GetRandomModifier(RewardBase.RewardGroup.NPC);
                        if (!traits.Any(t => t.Id == replacement.Id)) break;
                    }

                    traits.Add(replacement);
                    setupOption(opt, replacement, RarityHelper.GetRandomRarity());
                    GameManager.Instance.UIManager.multiSelectPanel.RefreshBanishButtons();
                }
            });
        };

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

            traits.Add(modifierBase);
            UIMultiSelectOption option = GameManager.Instance.UIManager.multiSelectPanel.Add(
                modifierBase.Id,
                modifierBase.GetSprite(),
                modifierBase.Name,
                modifierBase.GetDescription()
            );
            setupOption(option, modifierBase, RarityHelper.GetRandomRarity());
        }
    }

    public void AddModifier(RewardBase modifierBase)
    {
        Modifiers.Rewards.Add(modifierBase);
        /*if (GameManager.Instance.TutorialManager != null)
        {
            GameManager.Instance.TutorialManager.Check(TutorialEvent.TutorialCheck.NPC_AddTrait);
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

    public void FaceTarget(Vector3 position)
    {
        if (transform.position.x < position.x)
        {
            FaceRight();
        }
        else
        {
            FaceRight();
        }
        if (transform.position.y >= position.y)
        {
            FaceDown();
        }
        else
        {
            FaceUp();
        }
    }

    public override void ReceiveAttack(NPCBase npcBase)
    {
        base.ReceiveAttack(npcBase);
        // animator.SetBool("isHolding", true);
        SetExpression(FacialExpression.Panic);
    }
}