
using System;
using System.Collections.Generic;
using System.Linq;
using MetaChallenges;
using NPCs;
using Stats;
using UI;
using UnityEngine;
using Random = UnityEngine.Random;


public class Map
{
    public List<MapStage> Stages { get; set; }
    public List<MapLevel> LevelPool { get; set; } = new List<MapLevel>()
    {
        new MobileMapLevel(),
        new EmailMapLevel(),
        new SocketChatMapLevel(),
        new GeoLocationMapLevel(),
        new SecurityAuditMapLevel()
    };
    public int CurrentStageIndex { get; protected set; } = 0;
    public List<MapLevelVictoryConditionBase> GlobalVictoryConditions { get; set; }  = new List<MapLevelVictoryConditionBase>();
    public void Randomize()
    {
        Stages = new List<MapStage>();
       // Level 1 is just launch
        MapStage Stage = new MapStage(Stages.Count);
        MapLevel launchLevel = new LaunchMapLevel();
        launchLevel.Init(Stage);
        Stage.Levels.Add(launchLevel);
        Stages.Add(Stage);
        /*
       Stage = new MapStage(Stages.Count);


       Stage.Levels.Add(new MobileMapLevel(Stage));
       Stage.Levels.Add(new EmailMapLevel(Stage));
       Stages.Add(Stage);

       Stage = new MapStage(Stages.Count);
       Stage.Levels.Add(new SecurityAuditMapLevel(Stage));
       Stages.Add(Stage);

       Stage = new MapStage(Stages.Count);
       Stage.Levels.Add(new SocketChatMapLevel(Stage));
       Stage.Levels.Add(new GeoLocationMapLevel(Stage));
       Stages.Add(Stage);
       foreach (MapStage nextStage in Stages)
       {
           nextStage.Randomize();
       }*/
        
        
    }

    public MapLevel GetCurrentLevel()
    {
        return Stages[CurrentStageIndex].GetSelectedLevel();
    }


    public void IncrStage()
    {
    
        Stages[CurrentStageIndex].GetSelectedLevel().MarkCompleted();
        CurrentStageIndex += 1;
        GameManager.Instance.MetaStats.Incr(MetaStat.Sprint);
        
        GameManager.Instance.UIManager.multiSelectPanel.Display(
            "What should we work on next?",
            "Choose a feature to focus on."
        );

        List<MapLevel> levels = LevelPool.FindAll((level => level.State == MapLevel.MapLevelState.Incomplete));
        int endAt = levels.Count - 1;
        if (endAt > 3)
        {
            endAt = 3;
        }
        for (int i = 0; i < endAt; i++)
        {
            MapLevel level = levels[i]; 
            GameManager.Instance.UIManager.multiSelectPanel.Add(
                    level.Name,
                    GameManager.Instance.SpriteManager.GetSprite(level.GetSpriteId()),
                    level.Name,
                    level.GetDescription()
                )
                .OnSelect((string id) =>
                {
                    Stages[CurrentStageIndex].SetLevel(level);
                    GameManager.Instance.UIManager.Close();
                    GameManager.Instance.GameLoopManager.BeginPlanPhase();
                });
        }
    }

    public MapStage GetCurrentStage()
    {
        return Stages[CurrentStageIndex];
    }

    public void AddGlobalVictoryCondition(MapLevelVictoryConditionBase condition)
    {
        condition.SetGlobal();
        GlobalVictoryConditions.Add(condition);
    }
}
public class MapStage
{
    public int SelectedLevel { get; protected set; } = -1;
    public List<MapLevel>  Levels { get; set; } = new List<MapLevel>();
    public int StageNumber { get; protected set; }
    public MapStage(int stageNumber)
    {
        StageNumber = stageNumber;
    }

    public void Randomize()
    {
        foreach (MapLevel level in Levels)
        {
            level.Randomize();
        }
        /*for (int i = 0; i < 3; i++)
        {
            MapLevel level = new MapLevel();
            level.Randomize(StageNumber);
        }*/


    }
    public MapLevel SetSelectedLevel(int level)
    {
        SelectedLevel = level;
        Levels[SelectedLevel].OnSprintStart();
        return Levels[SelectedLevel];
    }
    public MapLevel SetLevel(MapLevel mapLevel)
    {
        mapLevel.Init(this);
        SelectedLevel = 0;
        Levels = new List<MapLevel>()
        {
            mapLevel
        };
        Levels[SelectedLevel].OnSprintStart();
        return Levels[SelectedLevel];
    }

    public MapLevel GetSelectedLevel()
    {
        if (SelectedLevel == -1)
        {
            throw new SystemException($"Selected level ({SelectedLevel}) is invalid.");
        }
        return Levels[SelectedLevel];
    }
}

public class MapLevel
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
   public List<MapLevelVictoryConditionBase> VictoryConditions =  new List<MapLevelVictoryConditionBase>();

   public List<MapLevelModifier> LevelModifiers = new List<MapLevelModifier>();
   public List<MapLevelModifier> LevelRewards = new List<MapLevelModifier>();
   // TODO Stake holder? Sales, PR, etc?
   //TODO Add in rewards
   // protected Dictionary<ModifierType, List<StatModifier>> StatModifiers { get; set; } = new Dictionary<ModifierType, List<StatModifier>>();
   protected MapStage Stage;
   public MapLevel()
   {
       
       VictoryConditions.Add(new HasMoneyVictoryCondition());
   }

   public void Init(MapStage stage)
   {
       Stage =  stage;
   }

   public string GetSpriteId()
   {
       return SpriteId;
   }

   public List<MapLevelVictoryConditionBase> GetCombinedVictoryConditions()
   {
       List<MapLevelVictoryConditionBase> victoryConditions = new List<MapLevelVictoryConditionBase>(VictoryConditions);
       victoryConditions.AddRange(GameManager.Instance.Map.GlobalVictoryConditions);
       return victoryConditions;
   }
   public virtual VictoryConditionState GetVictoryConditionState()
   {
       VictoryConditionState state = VictoryConditionState.Succeeded;
       List<MapLevelVictoryConditionBase> victoryConditions = GetCombinedVictoryConditions();
       foreach (MapLevelVictoryConditionBase condition in victoryConditions)
       {
           switch (condition.GetState())
           {
               case(VictoryConditionState.Failed):
                   return  VictoryConditionState.Failed;
                   break;
               case(VictoryConditionState.NotMet):
                   state =  VictoryConditionState.NotMet;
                   break;
                case(VictoryConditionState.Succeeded):
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
       if (Stage.StageNumber > 0) 
       {
           MapLevelVictoryConditionBase condition = GameManager.Instance.Map.GlobalVictoryConditions.Find((
               condition =>
               {
                   return condition is NetworkPacketLatencyVictoryCondition;
               }));
           if (condition == null)
           {
               GameManager.Instance.Map.AddGlobalVictoryCondition(new NetworkPacketLatencyVictoryCondition());
           }
           else
           {
               (condition as NetworkPacketLatencyVictoryCondition).Stats.AddModifier(
                   StatType.VictoryCondition_NetworkPacketLatency,
                   new StatModifier(
                       $"stage_{Stage.StageNumber}",
                       0.75f
                   )
               );
           }
       }
   }

   public virtual void Randomize()
   {
       // First, get a victory condition
       if (VictoryConditions.Count == 0)
       {
           // throw new NotImplementedException();
           /*InfraActiveVictoryCondition
               condition = InfraActiveVictoryCondition.GetRandomCondition(stage);
           VictoryConditions.Add(condition);*/
       }
      
       // Chose random modifiers based on stage

       for (int i = 0; i < Stage.StageNumber; i++)
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
       } else if (GameManager.Instance.GameLoopManager.GetCurrentDay() == 1)
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
   
       GameManager.Instance.UIManager.ShowNPCDialog(
           GameManager.Instance.SpriteManager.GetSprite("Suit1NPC"),
           "Great work. Lets get working on our next sprint.",
           new List<DialogButtonOption>()
           {
               new DialogButtonOption() { Text = "Plan Next Sprint", OnClick = () =>
                   {
                       
                       GameManager.Instance.Map.IncrStage();
                       GameManager.Instance.UIManager.productRoadMap.Show(UIProductRoadMap.State.Select);
                   }
               }/*,
               new DialogButtonOption() { Text = "Main Menu", OnSelect = () =>
                   {

                   }
               },*/
           }
       );
   }
   

   public virtual string GetDescription()
   {
       string res = $"{Name}\n";
       res += GetLevelDifficultyDesc() + "\n";
       res += "LevelModifiers:\n";
       foreach (MapLevelModifier modifier in LevelModifiers)
       {
           res += modifier.GetDescription(this) + "\n";
       }
       res += $"Victory Conditions:\n";
       foreach (MapLevelVictoryConditionBase condition in GetCombinedVictoryConditions())
       {
           if (condition.IsGlobal())
           {
               res += "(Global): ";
           }

           res += condition.GetDescription() + "\n";
       }
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
   public virtual void EndGame()
   {
    
       GameManager.Instance.UIManager.SetTimeScalePause();
       NPCBase bossNPC = GameManager.Instance.AllNpcs.Find((npc) => npc.GetComponent<BossNPC>() != null);
       GameManager.Instance.cameraController.ZoomToAndFollow(bossNPC.transform);
       GameManager.Instance.UpdateMetaProgress();
       GameManager.Instance.UIManager.ShowNPCDialog(
           GameManager.Instance.SpriteManager.GetSprite("Suit1NPC"),
           "You have failed to keep our infrastructure up and running with in our budget. You are fired!",
           new List<DialogButtonOption>()
           {
               new DialogButtonOption() { Text = "Start Over", OnClick = () =>
                   {
                       GameManager.Instance.StartNewGame();
                   }
               },
               new DialogButtonOption() { Text = "Main Menu", OnClick = () =>
                   {
                         GameManager.Instance.ShowMainMenu();
                   }
               },
           }
       );
   }

   public MapStage GetStage()
   {
       return Stage;
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

       CleanUp();
       State = MapLevelState.Completed;
   }

   public void PostSummaryCheck()
   {
       VictoryConditionState state = GetVictoryConditionState();
       switch (state)
       {
           case(VictoryConditionState.Failed):
               EndGame();
               return;
           case(VictoryConditionState.NotMet):
               if (IsLaunchDay())
               {
                   EndGame();
                   return;
               }
               break;
           case(VictoryConditionState.Succeeded):
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
           GameManager.Instance.GameLoopManager.BeginPlanPhase();
       }
   }
}

public class MapLevelModifier
{
    public enum ModifierDirection
    {
        Positive,
        Negative,
        // Neutral
    }

    public enum ModifierDuration
    {
        None,
        Sprint,
        LaunchDay
    }
    public enum ModifierType
    {
        SprintDuration,
        Stat
    }
    protected static List<MapLevelModifier> LevelModifiers = null;
    public ModifierDirection Direction;
    public ModifierType Type;
    public ModifierDuration Duration;
    public StatType? statType = null;
    public string Name;
    protected float OverrideValue;
    protected bool UseOverrideValue = false;
    protected StatModifier _statModifier;

    public static List<ModifierDuration> GetDurations()
    {
        return Enum.GetValues(typeof(ModifierDuration))
            .Cast<ModifierDuration>()
            .ToList();
    }
    public static ModifierDuration GetRandomDuration()
    {
        List<ModifierDuration> durations = GetDurations();
        int i = Random.Range(0, durations.Count);
        return durations[i];
    }


    private static List<ModifierDirection> GetDirections()
    {
        return Enum.GetValues(typeof( ModifierDirection))
            .Cast<ModifierDirection>()
            .ToList();
    }



    private static List<ModifierType> GetTypes()
    {
        return Enum.GetValues(typeof( ModifierType))
            .Cast<ModifierType>()
            .ToList();
    }

  

    private static List<StatType>  GetStats()
    {
        return new List<StatType>()
        {
            StatType.NetworkPacket_Probibility,
            StatType.Traffic,
            StatType.TechDebt_AccumulationRate
        };
    }

    public static List<MapLevelModifier> BuildMapLevelModifiers()
    {
        
        List<MapLevelModifier> modifiers = new List<MapLevelModifier>();
        List<ModifierType> types = GetTypes();
        foreach (ModifierType type in types)
        {
            switch (type)
            {
                case(ModifierType.SprintDuration):
                    foreach (ModifierDirection direction in GetDirections())
                    {
                        MapLevelModifier modifier = new MapLevelModifier();
                        modifier.Direction = direction;
                        modifier.Type = type;
                        
                        modifiers.Add(modifier);
                    }

                    break;
                case(ModifierType.Stat):
                    foreach (ModifierDirection direction in GetDirections())
                    {
                        foreach (ModifierDuration duration in GetDurations())
                        {
                            switch (duration)
                            {
                                case(ModifierDuration.Sprint):
                                case(ModifierDuration.LaunchDay):
                                    foreach (StatType statType in GetStats())
                                    {
                                        MapLevelModifier modifier = new MapLevelModifier();
                                        modifier.Direction = direction;
                                        modifier.Duration = duration;
                                        modifier.Type = type;
                                        modifier.statType = statType;
                                        modifiers.Add(modifier);
                                    }
                                    break;
                                case(ModifierDuration.None):
                                    continue;
                                default:
                                    throw new NotImplementedException($"Duration {duration} not implemented");
                                    break;
                                
                            }
                            
                        }
                    }

                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        return modifiers;
    }

    public static MapLevelModifier GetRandom()
    {
        if (LevelModifiers == null)
        {
            LevelModifiers = BuildMapLevelModifiers();
        }

        int i = Random.Range(0, LevelModifiers.Count);
        return LevelModifiers[i];
    }

    public void Apply(MapLevel level)
    {
        switch (Type)
        {
            case(ModifierType.SprintDuration):
                level.SprintDuration = (int)CalcValue(level);
                break;
            case(ModifierType.Stat):
                
                switch (statType)
                {
                    case(StatType.NetworkPacket_Probibility):
                        // Find and apply this to 
                        NetworkPacketData networkPacketData =
                            GameManager.Instance.GetNetworkPacketDataByType(NetworkPacketData.PType.Purchase);
                        _statModifier = new StatModifier("level_modifier_temp", CalcValue(level));
                        networkPacketData.Stats.AddModifier(statType.Value, _statModifier);
                        break;
                    case(StatType.Traffic):
                    case(StatType.TechDebt_AccumulationRate):
                        _statModifier = new StatModifier("level_modifier_temp", CalcValue(level));
                        GameManager.Instance.Stats.AddModifier(statType.Value, _statModifier);
                        break;
                    default:
                        throw new NotImplementedException();
                }
                break;
            default:
                throw new NotImplementedException();
        }
    }

    public void SetOverrideValue(float value)
    {
        UseOverrideValue = true;
        OverrideValue = value;
    }
    public bool Equals(MapLevelModifier other)
    {
        return (
            Type == other.Type &&
            Direction == other.Direction &&
            Duration == other.Duration && 
            statType == other.statType
        );
    }

    public string GetDescription(MapLevel level)
    {
        return $"{Type} {Direction} {Duration} {statType} x {CalcValue(level)}";
    }

    private float CalcValue(MapLevel level)
    {
        if (UseOverrideValue)
        {
            return OverrideValue;
        }
        switch (Type)
        {
            case(ModifierType.SprintDuration):
                switch (Direction)
                {
                    case(ModifierDirection.Positive):
                        return 5 + level.GetStage().StageNumber; // TODO Make this dynamic
                        break;
                    case(ModifierDirection.Negative):
                        return 5 - level.GetStage().StageNumber; // TODO Make this dynamic
                        break;
                    default:
                        throw new NotFiniteNumberException();
                }
            case(ModifierType.Stat):
                ModifierDirection direction = Direction;
                switch (statType)
                {
                    case(StatType.TechDebt_AccumulationRate):
                        // Invert the direction for calculation purpuses
                        switch (direction)
                        {
                            case(ModifierDirection.Positive):
                                direction = ModifierDirection.Negative;
                                break;
                            case(ModifierDirection.Negative):
                                direction = ModifierDirection.Positive;
                                break;
                            default:
                                throw new NotImplementedException();
                                
                        }
                        break;
                }

                float baseValue = 0;
                switch (Duration)
                {
                    case(ModifierDuration.LaunchDay):
                        baseValue = .25f;
                        break;
                    case(ModifierDuration.Sprint):
                        baseValue = .1f;
                        break;
                    default:
                        throw new NotImplementedException();
                }
                switch (direction)
                {
                    case(ModifierDirection.Positive):
                        return (float)Math.Pow(1 + baseValue, level.GetStage().StageNumber);
                    
                    case(ModifierDirection.Negative):
                        return (float)Math.Pow(1 - baseValue, level.GetStage().StageNumber);
                    default:
                        throw new NotImplementedException();
                }
            default:
                throw new NotImplementedException();
        }
    }

    public void CleanUp(MapLevel mapLevel)
    {
        switch (Type)
        {
            case(ModifierType.Stat):
                if (_statModifier != null)
                {
                    _statModifier.Remove();
                }

                break;
        }
    }
}

