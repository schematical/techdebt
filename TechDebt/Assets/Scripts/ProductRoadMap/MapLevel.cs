using System;
using System.Collections.Generic;
using System.Linq;
using DefaultNamespace.Rewards;
using NPCs;
using Stats;
using UI;
using UnityEngine;

public class MapLevel : iUIMapNode, iUnlockable
{
    public enum MapLevelState
    {
        Incomplete,
        Completed,
    }

    public MapLevelState State { get; protected set; } = MapLevelState.Incomplete;
    public string Name { get; set; }
    protected string SpriteId { get; set; } = "IconFlag";
    public int SprintDuration { get; set; } = 5;
    public List<MapLevelVictoryConditionBase> VictoryConditions = new List<MapLevelVictoryConditionBase>();

    public List<MapLevelModifier> LevelModifiers = new List<MapLevelModifier>();

    public List<MapLevelReward> LevelRewards = new List<MapLevelReward>();
   
    public string RequiredStakeholderId { get; set; }
    public List<UnlockCondition> UnlockConditions;
    // iUIMapNode Implementation
    public string Id => GetType().Name;
    public string DisplayName => Name;
    public string Description => GetDescription();
    public MapLevel()
    {
        
    }
    public MapNodeState CurrentState
    {
        get
        {
            if (State == MapLevelState.Completed) return MapNodeState.Unlocked;
            if (GameManager.Instance.Map.GetCurrentLevel() == this) return MapNodeState.Active;
            
            // If it's not completed or active, check if it's reachable
            if (DependencyIds == null || DependencyIds.Count == 0) return MapNodeState.Locked; // Root nodes are always playable if not completed/active
            
            // If ANY dependency is met, it's Locked (ready to play), else MetaLocked
            // Wait, should it be ANY or ALL? The user's mermaid diagram implies a tree/graph.
            // Usually in these games, ANY dependency being met unlocks the node.
            bool allDependencyMet = DependencyIds.All(depId => {
                MapLevel dep = GameManager.Instance.Map.LevelPool.FirstOrDefault(l => l.Id == depId);
                return  dep.State == MapLevelState.Completed;
            });
            
            return allDependencyMet ? MapNodeState.Locked : MapNodeState.MetaLocked;
        }
    }
    
    public MapNodeDirection Direction { get; set; } = MapNodeDirection.Right;
    

    public List<string> DependencyIds { get; protected set; } = new List<string>();

    public float GetProgress()
    {
        if (SprintDuration <= 0) return 0f;
        return (float)GameManager.Instance.GameLoopManager.GetCurrentDay() / SprintDuration;
    }

    public UnityEngine.Tilemaps.TileBase GetTile()
    {
        string tileId = "TechTreeLockedTile";
        switch (CurrentState)
        {
            case MapNodeState.MetaLocked:
                tileId = "TechTreeLockedTile";
                break;
            case MapNodeState.Locked:                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                      
                tileId = "TechTreeUnlockedTile";
                break;
            case MapNodeState.Active:
                tileId = "TechTreeResearching";
                break;
            case MapNodeState.Unlocked:
                tileId = "TechTreeResearched";
                break;
        }
        return GameManager.Instance.prefabManager.GetTile(tileId);
    }

    public void OnSelected(UI.UIMapPanel panel)
    {
        panel.UpdateDetailsArea();
    }
    


    public virtual List<RewardBase> GetSpecialReleaseRewards()
    {
        List<RewardBase> rewards = new List<RewardBase>();
        foreach (MapLevelVictoryConditionBase victoryCondition in VictoryConditions)
        {
            if (victoryCondition is SpecialReleaseVictoryCondition)
            {
                SpecialReleaseVictoryCondition specialReleaseVictoryCondition =
                    victoryCondition as SpecialReleaseVictoryCondition;
                if (!specialReleaseVictoryCondition.HasReleaseBeedDeployed)
                {
                    rewards.Add(specialReleaseVictoryCondition.GetReward());
                }
            }
        }
        return rewards;
    }



    public string GetSpriteId()
    {
        return SpriteId;
    }

    public List<MapLevelVictoryConditionBase> GetCombinedVictoryConditions()
    {
        List<MapLevelVictoryConditionBase>
            victoryConditions = new List<MapLevelVictoryConditionBase>(VictoryConditions);
        victoryConditions.AddRange(GameManager.Instance.Map.GlobalVictoryConditions);
        return victoryConditions;
    }

    public virtual VictoryConditionState GetVictoryConditionState(bool isFinal = false)
    {
        VictoryConditionState state = VictoryConditionState.Succeeded;
        List<MapLevelVictoryConditionBase> victoryConditions = GetCombinedVictoryConditions();
        foreach (MapLevelVictoryConditionBase condition in victoryConditions)
        {
            VictoryConditionState testState = condition.GetState();
            if (isFinal)
            {
                testState = condition.GetFinalState();
            }
            
            switch (testState)
            {
                case (VictoryConditionState.Failed):
                    return VictoryConditionState.Failed;
                    break;
                case (VictoryConditionState.NotMet):
                    state = VictoryConditionState.NotMet;
                    break;
                case (VictoryConditionState.Succeeded):
                    // Do nothing because it should be succeeded already
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        return state;
    }

    public virtual void OnSprintStart()
    {
        GameManager.Instance.GameLoopManager.Reset();
        // TODO: Probably move this to the map.
        ApplyRewards(MapLevelReward.MapLevelRewardApplied.Start);
 
        MapLevelVictoryConditionBase condition = GameManager.Instance.Map.GlobalVictoryConditions.Find((
            condition => { return condition is NetworkPacketLatencyVictoryCondition; }));
        if (condition == null)
        {
            GameManager.Instance.Map.AddGlobalVictoryCondition(new NetworkPacketLatencyVictoryCondition());
        }
        else
        {
            (condition as NetworkPacketLatencyVictoryCondition).Stats.AddModifier(
                StatType.VictoryCondition_NetworkPacketLatency,
                new StatModifier(
                    $"level_{Id}",
                    0.75f
                )
            );
        }
        
    }

    private void ApplyRewards(MapLevelReward.MapLevelRewardApplied appliedAt)
    {
        MetaProgressData metaData = MetaGameManager.LoadProgress();
        bool progressChanged = false;

        foreach (MapLevelReward reward in LevelRewards)
        {
            if (reward.AppliedAt != appliedAt) continue;

            if (reward.DependencyIds.Count > 0 && !reward.DependencyIds.All(depId => metaData.claimedMetaRewardIds.Contains(depId)))
            {
                continue;
            }

            if (reward.Type == MapLevelReward.MapLevelRewardType.Meta && metaData.claimedMetaRewardIds.Contains(reward.Id))
            {
                continue;
            }

            if (reward.VictoryConditions.All((condition) => condition.GetFinalState() == VictoryConditionState.Succeeded))
            {
                reward.Reward.Apply();
                
                if (reward.Type == MapLevelReward.MapLevelRewardType.Meta)
                {
                    metaData.claimedMetaRewardIds.Add(reward.Id);
                    progressChanged = true;
                }
            }
        }

        if (progressChanged)
        {
            MetaGameManager.SaveProgress(metaData);
        }
    }

    public virtual void Randomize(int modifierCount)
    {
        Debug.Log($"{Name} Randomize, Clearing Level Modifiers - Before {LevelModifiers.Count}");
        LevelModifiers.Clear();
        // First, get a victory condition
        if (VictoryConditions.Count == 0)
        {
            // throw new NotImplementedException();
            /*InfraActiveVictoryCondition
                condition = InfraActiveVictoryCondition.GetRandomCondition(stage);
            VictoryConditions.Add(condition);*/
        }

        // Chose random modifiers based on stage

        for (int i = 0; i < modifierCount; i++)
        {
            MapLevelModifier modifier = null;
            int saftyCheck = 0;
            while (modifier == null && saftyCheck < 10)
            {
                saftyCheck += 1;
                modifier = MapLevelModifier.GetRandom();
                foreach (MapLevelModifier checkModifier in LevelModifiers)
                {
                    if (checkModifier.Equals(modifier))
                    {
                        modifier = null;
                        break;
                    }
                }
            }

            if (modifier == null)
            {
                Debug.LogError($"SaftyCheck hit randomizing level");
                continue;
            }

            LevelModifiers.Add(modifier);
        }

        // int levelDifficultyAdjustment = CalculateLevelDifficulty();
    }

    public int CalculateLevelDifficulty()
    {
        int levelDifficultyAdjustment = 0; // Use to figure out bigger reward
        foreach (MapLevelModifier modifier in LevelModifiers)
        {
            switch (modifier.Direction)
            {
                case (MapLevelModifier.ModifierDirection.Negative):
                    // Get a negative
                    levelDifficultyAdjustment -= 1;

                    break;
                case (MapLevelModifier.ModifierDirection.Positive):
                    // Get a positive
                    levelDifficultyAdjustment += 1;
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        //TODO: Use the `levelDifficultyAdjustment` to determine the reward.
        return levelDifficultyAdjustment;
    }

    public string GetLevelDifficultyDesc()
    {
        int levelDifficultyAdjustment = CalculateLevelDifficulty();
        if (levelDifficultyAdjustment == 0)
        {
            return "Medium";
        }

        string res = "Easy";
        if (levelDifficultyAdjustment < 0)
        {
            res = "Hard";
        }

        for (int i = 1; i <= levelDifficultyAdjustment; i++)
        {
            res = $"Very {res}";
        }

        return res;
    }

    /*public void CleanUpModifiers(ModifierType modifierType)
    {
        if (!StatModifiers.ContainsKey(modifierType))
        {
            throw new SystemException($"Cannot find {modifierType}");
        }

        foreach (StatModifier statModifier in StatModifiers[modifierType])
        {
            statModifier.Remove();
        }
        StatModifiers[modifierType].Clear();
    }*/

    public virtual bool IsLaunchDay()
    {
        return SprintDuration == GameManager.Instance.GameLoopManager.GetCurrentDay();
    }

    public virtual void PlanPhaseCheck()
    {
        if (IsLaunchDay())
        {
            OnLaunchDayPlan();
        }
        else if (GameManager.Instance.GameLoopManager.GetCurrentDay() == 1)
        {
            OnStartDayPlan();
        }
    }

    public virtual void OnStartDayPlan()
    {
        foreach (MapLevelModifier modifier in LevelModifiers)
        {
            switch (modifier.Duration)
            {
                case (MapLevelModifier.ModifierDuration.Sprint):
                    modifier.Apply(this);
                    break;
            }

            GameManager.Instance.InfrastructureUpdateNetworkTargets();
        }
    }

    public virtual void OnLaunchDayPlan()
    {
        foreach (MapLevelModifier modifier in LevelModifiers)
        {
            switch (modifier.Duration)
            {
                case (MapLevelModifier.ModifierDuration.LaunchDay):
                    modifier.Apply(this);
                    break;
            }

            GameManager.Instance.InfrastructureUpdateNetworkTargets();
        }
    }

    public virtual void OnLaunchDaySummary()
    {
        MarkCompleted();
        NPCBase npc =
            GameManager.Instance.AllNpcs.Find((npc) => npc.GetComponent<NPCSchematicalBot>() != null);
        npc.ShowDialogBubble().SimpleDisplay(
            "Great work. Lets get working on our next sprint.",
            new List<DialogButtonOption>()
            {
                new DialogButtonOption()
                {
                    Text = "Plan Next Sprint", OnClick = () =>
                    {
                        if (!GameManager.Instance.GetInfrastructureInstanceByID("product-road-map").IsActive())
                        {
                            npc.ShowDialogBubble().SimpleDisplay(
                                "Research `Product Road Map` to progress"
                            );
                        }
                        else
                        {
                            GameManager.Instance.UIManager.productRoadMap.Show(UIProductRoadMap.State.Select);
                        }
                    }
                } /*,
                new DialogButtonOption() { Text = "Main Menu", OnSelect = () =>
                    {
 // TODO: Prestige?
                    }
                },*/
            }
        );
    }


    public virtual string GetDescription()
    {
        string res = $"{Name}\n";
        res += GetLevelDifficultyDesc() + "\n";
        if (LevelModifiers.Count > 0)
        {
            res += "LevelModifiers:\n";
            foreach (MapLevelModifier modifier in LevelModifiers)
            {
                res += modifier.GetDescription(this) + "\n";
            }
        }
        res += "\n";
        res += $"Victory Conditions:\n";
        foreach (MapLevelVictoryConditionBase condition in GetCombinedVictoryConditions())
        {
            if (condition.IsGlobal())
            {
                res += "(Global): ";
            }

            res += condition.GetDescription() + "\n";
        }

        res += "\n";
        MetaProgressData metaData = MetaGameManager.LoadProgress();
        List<MapLevelReward> rewardsWithoutConditions = LevelRewards.FindAll(r => {
            if (r.VictoryConditions.Count > 0) return false;
            if (r.DependencyIds.Count > 0 && !r.DependencyIds.All(depId => metaData.claimedMetaRewardIds.Contains(depId))) return false;
            return true;
        });
        if (rewardsWithoutConditions.Count > 0)
        {
            res += $"Rewards:\n";
            foreach (MapLevelReward reward in rewardsWithoutConditions)
            {
                res += $"{reward.Reward.GetTitle()}\n";
            }
        }
        res += "\n";
        return res;
    }

    public virtual void CleanUp()
    {
        foreach (MapLevelModifier modifier in LevelModifiers)
        {
            modifier.CleanUp(this);
        }

        GameManager.Instance.InfrastructureUpdateNetworkTargets();
    }

    public virtual void EndGame(string dialog = null, bool isVictory = false)
    {
        GameManager.Instance.UIManager.ForcePause();

        GameManager.Instance.UpdateMetaProgress(isVictory);

        NPCBase npc =
            GameManager.Instance.AllNpcs.Find((npc) => npc.GetComponent<NPCSchematicalBot>() != null);
        npc.ZoomToAndFollow();
        if (dialog == null)
        {
            dialog = "Unfortunately we failed the following goals: \n\n";
            foreach (MapLevelVictoryConditionBase condition in GetCombinedVictoryConditions())
            {
                if (condition.GetFinalState() == VictoryConditionState.Failed)
                {
                    dialog += $" - {condition.GetDescription()}\n";
                }
            }
            
        }
        npc.ShowDialogBubble().SimpleDisplay(
            dialog,
            new List<DialogButtonOption>()
            {
                new DialogButtonOption()
                {
                    Text = "Start Over", OnClick = () => { GameManager.Instance.StartNewGame(); }
                },
                new DialogButtonOption()
                {
                    Text = "Main Menu", OnClick = () => { GameManager.Instance.ShowMainMenu(); }
                },
            }
        );
    }

  

    public List<MapLevelVictoryConditionBase> GetVictoryConditions()
    {
        return VictoryConditions;
    }

    public void MarkCompleted()
    {
        if (State != MapLevelState.Incomplete)
        {
            throw new SystemException("MapLevel.State is not incomplete");
        }

        MetaProgressData metaData = MetaGameManager.LoadProgress();
        if (metaData.mapLevelData == null) metaData.mapLevelData = new List<MetaMapLevelData>();
        
        MetaMapLevelData levelMeta = metaData.mapLevelData.Find(l => l.levelId == Id);
        if (levelMeta == null)
        {
            levelMeta = new MetaMapLevelData { levelId = Id, completedCount = 0 };
            metaData.mapLevelData.Add(levelMeta);
        }
        levelMeta.completedCount++;
        MetaGameManager.SaveProgress(metaData);

        CleanUp();
        State = MapLevelState.Completed;
        ApplyRewards(MapLevelReward.MapLevelRewardApplied.End);
    }

    public void PostSummaryCheck()
    {
        VictoryConditionState state = GetVictoryConditionState(IsLaunchDay());
        switch (state)
        {
            case (VictoryConditionState.Failed):
                EndGame();
                return;
            case (VictoryConditionState.NotMet):
                if (IsLaunchDay())
                {
                    EndGame();//TODO: Make this show a real error message.
                    throw new SystemException("This shouldn't happen any more");
                }

                break;
            case (VictoryConditionState.Succeeded):
                break;
            default:
                throw new NotImplementedException();
        }

        // TODO: Possibly make the amount of packets served up increase or decrease the amount of PURCHASE packets served up
        if (IsLaunchDay())
        {
            OnLaunchDaySummary();
        }
        else
        {
            // GameManager.Instance.GameLoopManager.BeginPlanPhase();
        }
    }

    public void AddCashReward(float start = -1, float end = -1)
    {

        if (start != -1)
        {
            LevelRewards.Add(new MapLevelReward()
            {
                AppliedAt =   MapLevelReward.MapLevelRewardApplied.Start,
                Reward = new GlobalStatBaseValueReward()
                {
                    // Group = RewardBase.RewardGroup.Release,
                    Id = "sprint_start_money",
                    Name = "Sprint Start Budget Bonus",
                    Description = "Your budget will be increased by this much at the start of the sprint",
                    StatType = StatType.Money,
                    BaseValue = start,
                    // ScaleDirection = ScaleDirection.Down,
                    IconSpriteId = "IconDollar"
                },
            });
        }
        if (end != -1)
        {
            LevelRewards.Add(new MapLevelReward()
            {
                AppliedAt =   MapLevelReward.MapLevelRewardApplied.End,
                Reward = new GlobalStatBaseValueReward()
                {
                    // Group = RewardBase.RewardGroup.Release,
                    Id = "sprint_end_money",
                    Name = "Sprint Completed Budget Bonus",
                    Description = "Your budget will be increased by this much when you complete the sprint",
                    StatType = StatType.Money,
                    BaseValue = end,
                    // ScaleDirection = ScaleDirection.Down,
                    IconSpriteId = "IconDollar"
                },
            });
        }

    }

    public void AddPrestigePointsReward(int value = 1)
    {


        MapLevelReward levelCompleted = new MapLevelReward()
        {
            Id = $"{Id}_completed",
            Description = $"Level Completed",
            Type = MapLevelReward.MapLevelRewardType.Meta,
            AppliedAt = MapLevelReward.MapLevelRewardApplied.End,
            Reward = new MetaStatBaseValueReward()
            {
                Id = "prestige_points",
                Name = "Vested Shares",
                Description = "Vested Shares allow you to unlock bonuses on future runs.",
                BaseValue = value,
                IconSpriteId = "IconDollar"
            },
        };
        LevelRewards.Add(levelCompleted);
        MapLevelReward uptime75 = new MapLevelReward()
        {
            Id = $"{Id}_uptime_75",
            Description = $"Uptime Greater Than 75%",
            Type = MapLevelReward.MapLevelRewardType.Meta,
            AppliedAt = MapLevelReward.MapLevelRewardApplied.End,
            DependencyIds = new List<string>()
            {
                levelCompleted.Id
            },
            VictoryConditions = new List<MapLevelVictoryConditionBase>()
            {
                new UpTimeVictoryCondition(0.75f)
            },
            Reward = new MetaStatBaseValueReward()
            {
                Id = "prestige_points",
                Name = "Vested Shares",
                Description = "Vested Shares allow you to unlock bonuses on future runs.",
                BaseValue = value,
                IconSpriteId = "IconDollar"
            },
        };
        LevelRewards.Add(uptime75);
        MapLevelReward uptime90 = new MapLevelReward()
        {
            Id = $"{Id}_uptime_90",
            Description = $"Uptime Greater Than 90%",
            Type = MapLevelReward.MapLevelRewardType.Meta,
            AppliedAt = MapLevelReward.MapLevelRewardApplied.End,
            DependencyIds = new List<string>()
            {
                uptime75.Id
            },
            VictoryConditions = new List<MapLevelVictoryConditionBase>()
            {
                new UpTimeVictoryCondition(0.90f)
            },
            Reward = new MetaStatBaseValueReward()
            {
                Id = "prestige_points",
                Name = "Vested Shares",
                Description = "Vested Shares allow you to unlock bonuses on future runs.",
                BaseValue = value,
                IconSpriteId = "IconDollar"
            },
        };
        LevelRewards.Add(uptime90);
        LevelRewards.Add(new MapLevelReward()
        {
            Id = $"{Id}_uptime_99",
            Description = $"Uptime Greater Than 99%",
            Type = MapLevelReward.MapLevelRewardType.Meta,
            AppliedAt =   MapLevelReward.MapLevelRewardApplied.End,
            DependencyIds = new List<string>()
            {
                uptime90.Id
            },
            VictoryConditions = new List<MapLevelVictoryConditionBase>()
            {
                new UpTimeVictoryCondition(0.99f)
            },
            Reward = new MetaStatBaseValueReward()
            {
                Id = "prestige_points",
                Name = "Vested Shares",
                Description = "Vested Shares allow you to unlock bonuses on future runs.",
                BaseValue = value,
                IconSpriteId = "IconDollar"
            },
        });
    }

    public bool IsUnlocked()
    {
        return UnlockConditions.All(condition => condition.IsUnlocked());
    }
}