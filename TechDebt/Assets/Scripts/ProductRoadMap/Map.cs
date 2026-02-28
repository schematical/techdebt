
using System;
using System.Collections.Generic;
using System.Linq;
using MetaChallenges;
using NPCs;
using Stats;
using UnityEngine;
using Random = UnityEngine.Random;


public class Map
{
    public List<ProductRoadMapStage> Stages { get; set; }
    public int CurrentStage { get; protected set; } = 0;

    public void Randomize()
    {
        Stages = new List<ProductRoadMapStage>();
        // Level 1 is just launch
        ProductRoadMapStage Stage = new ProductRoadMapStage(Stages.Count);
        Stage.Levels.Add(new LaunchMapLevel());
        Stages.Add(Stage);
        
        Stage = new ProductRoadMapStage(Stages.Count);

        
        Stage.Levels.Add(new MobileMapLevel());
        Stage.Levels.Add(new EmailMapLevel());
        Stages.Add(Stage);
        
        Stage = new ProductRoadMapStage(Stages.Count);
        Stage.Levels.Add(new SecurityAuditMapLevel());
        Stages.Add(Stage);
        
        Stage = new ProductRoadMapStage(Stages.Count);
        Stage.Levels.Add(new SocketChatMapLevel());
        Stage.Levels.Add(new GeoLocationMapLevel());
        Stages.Add(Stage);
        foreach (ProductRoadMapStage nextStage in Stages)
        {
            nextStage.Randomize();
        }
        
        
    }

    public MapLevel GetCurrentLevel()
    {
        return Stages[CurrentStage].GetSelectedLevel();
    }


    public void IncrStage()
    {
        CurrentStage += 1;
        GameManager.Instance.MetaStats.Incr(MetaStat.Sprint);
    }
}
public class ProductRoadMapStage
{
    public int SelectedLevel { get; protected set; } = -1;
    public List<MapLevel>  Levels { get; set; } = new List<MapLevel>();
    public int Stage { get; protected set; }
    public ProductRoadMapStage(int _stage)
    {
        Stage = _stage;
    }

    public void Randomize()
    {
        foreach (MapLevel level in Levels)
        {
            level.Randomize(Stage);
        }
        /*for (int i = 0; i < 3; i++)
        {
            MapLevel level = new MapLevel();
            level.Randomize(Stage);
        }*/


    }
    public MapLevel SetSelectedLevel(int level)
    {
        SelectedLevel = level;
        Levels[SelectedLevel].OnSprintStart();
        return Levels[SelectedLevel];
    }

    public MapLevel GetSelectedLevel()
    {
        if (SelectedLevel == -1)
        {
            throw new SystemException("Selected level is invalid.");
        }
        return Levels[SelectedLevel];
    }
}

public class MapLevel
{
    public enum ModifierType
    {
        LaunchDay,
        Level
    }
   public string Name { get; set; }
   protected string SpriteId { get; set; } = "IconFlag";
   public int SprintDuration { get; set; } = 5;
   public List<MapLevelVictoryCondition> VictoryConditions =  new List<MapLevelVictoryCondition>();

   public List<MapLevelModifier> LevelModifiers = new List<MapLevelModifier>();
   // TODO Stake holder? Sales, PR, etc?
   //TODO Add in modifiers and rewards
   protected Dictionary<ModifierType, List<StatModifier>> StatModifiers { get; set; } = new Dictionary<ModifierType, List<StatModifier>>();

   public MapLevel()
   {
       StatModifiers.Add(ModifierType.LaunchDay,  new List<StatModifier>());
       StatModifiers.Add(ModifierType.Level,  new List<StatModifier>());
   }

   public string GetSpriteId()
   {

       return SpriteId;
   }
   public virtual VictoryConditionState GetVictoryConditionState()
   {
       VictoryConditionState state = VictoryConditionState.Succeeded;
       foreach (MapLevelVictoryCondition condition in VictoryConditions)
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
   }

   public virtual void Randomize(int stage = 0)
   {
       // First get a victory condition
       if (VictoryConditions.Count == 0)
       {
           // throw new NotImplementedException();
           /*MapLevelVictoryCondition
               condition = MapLevelVictoryCondition.GetRandomCondition(stage);
           VictoryConditions.Add(condition);*/
       }
      
       // Chose random modifiers based on stage
       int levelDifficultyAdjustment = 0; // Use to figure out bigger reward
       for (int i = 0; i < stage; i++)
       {

           MapLevelModifier modifier = null;
           int saftyCheck = 0;
           while (modifier == null && saftyCheck < 10)
           {
               saftyCheck += 1;
               modifier = MapLevelModifier.GetRandom(stage);
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
           switch (modifier.Direction) 
           {
               case(MapLevelModifier.ModifierDirection.Negative):
                   // Get a negative
                   levelDifficultyAdjustment -= 1;
                   
                   break;
               case(MapLevelModifier.ModifierDirection.Positive):
                   // Get a positive
                   levelDifficultyAdjustment += 1;
                   break;
               default:
                   throw new NotImplementedException();
           }
           //TODO: Use the `levelDifficultyAdjustment` to determine the reward.
           
           
       }
   }

   public void CleanUpModifiers(ModifierType modifierType)
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
   }

   public virtual bool IsLaunchDay()
   {
       return SprintDuration == GameManager.Instance.GameLoopManager.GetCurrentDay();
   }

   public virtual void PlanPhaseCheck()
   {
       if (IsLaunchDay())
       {
           OnLaunchDayPlan();
       } else if (GameManager.Instance.GameLoopManager.GetCurrentDay() == 0)
       {
           OnStartDayPlan();
       }
      
   }

   public virtual void OnStartDayPlan()
   {
       
   } 
   public virtual void OnLaunchDayPlan()
   {
       
   }
   public virtual void SummaryPhaseCheck()
   {
       if (GameManager.Instance.GetStat(StatType.Money) < 0)
       {
           EndGame();
           return;
       }
       // TODO: Possibly make the amount of packets served up increase or decrease the amount of PURCHASE packets served up
       if (IsLaunchDay())
       {
           OnLaunchDaySummary();
       }
      
   }
   public virtual void OnLaunchDaySummary()
   {
       throw new NotImplementedException();
   }

   public virtual void OnLaunchDay()
   {
       throw new NotImplementedException();
   }

   public virtual string GetDescription()
   {
       string res = $"{Name}  - LevelModifiers:{LevelModifiers.Count}\n";
       foreach (MapLevelModifier modifier in LevelModifiers)
       {
           res += modifier.GetDescription() + "\n";
       }
       return res;
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
                        
                       GameManager.Instance.Reset();
                   }
               }/*,
               new DialogButtonOption() { Text = "Main Menu", OnClick = () =>
                   {
                        
                   }
               },*/
           }
       );
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
        Spring,
        LaunchDay
    }
    public enum ModifierType
    {
        SprintDuration,
        Stat
    }
    
    public ModifierDirection Direction;
    public ModifierType Type;
    public ModifierDuration Duration;
    public StatType? statType = null;
    public string Name;
    public float Value;

    public static ModifierDuration GetRandomDuration()
    {
        List<ModifierDuration> durations = Enum.GetValues(typeof( ModifierDuration))
            .Cast<ModifierDuration>()
            .ToList();
        int i = Random.Range(0, durations.Count);
        return durations[i];
    }
    public static ModifierDirection GetRandomDirection()
    {
        List<ModifierDirection> direction = Enum.GetValues(typeof( ModifierDirection))
            .Cast<ModifierDirection>()
            .ToList();
        int i = Random.Range(0, direction.Count);
        return direction[i];
    }
    public static ModifierType GetRandomType()
    {
        List<ModifierType> types = Enum.GetValues(typeof( ModifierType))
            .Cast<ModifierType>()
            .ToList();
        int i = Random.Range(0, types.Count);
        return types[i];
    }
    public static StatType GetRandomStat()
    {
        List<StatType> statTypes = new List<StatType>()
        {
            StatType.NetworkPacket_Probibility,
            StatType.Traffic,
            StatType.TechDebt_AccumulationRate
        };
        int i = Random.Range(0, statTypes.Count);
        return statTypes[i];
    }

    public static MapLevelModifier GetRandom(int stage)
    {
        MapLevelModifier modifier = new MapLevelModifier()
        {
            Type = GetRandomType(),
            Direction = GetRandomDirection()
        };
        
        
        switch (modifier.Type)
        {
            case(ModifierType.Stat):
                modifier.statType = GetRandomStat();
                modifier.Value = 0;
                switch (modifier.Direction)
                {
                    case(ModifierDirection.Positive):
                        modifier.Value = (float)Math.Pow(1.25f, stage);
                        break;
                    case(ModifierDirection.Negative):
                        modifier.Value = (float)Math.Pow(0.75f, stage);
                        break;
                    default:
                        throw new NotFiniteNumberException();
                }
                break;
                case(ModifierType.SprintDuration):
                    switch (modifier.Direction)
                    {
                        case(ModifierDirection.Positive):
                            modifier.Value = 5 + stage;
                            break;
                        case(ModifierDirection.Negative):
                            modifier.Value = 5 - stage;
                            break;
                        default:
                            throw new NotFiniteNumberException();
                    }
                break;
            default:
                throw new NotFiniteNumberException();
        }

        return modifier;
    }

    public void Apply(MapLevel level)
    {
        switch (Type)
        {
            case(ModifierType.SprintDuration):
                level.SprintDuration = (int)Value;
                break;
            case(ModifierType.Stat):
                StatType stat = GetRandomStat();
                switch (stat)
                {
                    case(StatType.NetworkPacket_Probibility):
                        // Find and apply this to 
                        NetworkPacketData networkPacketData =
                            GameManager.Instance.GetNetworkPacketDataByType(NetworkPacketData.PType.Purchase);
                        networkPacketData.Stats.AddModifier(stat, new StatModifier("level_modifier_temp", Value));
                        break;
                    case(StatType.Traffic):
                    case(StatType.TechDebt_AccumulationRate):
                        GameManager.Instance.Stats.AddModifier(stat, new StatModifier("level_modifier_temp", Value));
                        break;
                    default:
                        throw new NotFiniteNumberException();
                }
                break;
            default:
                throw new NotFiniteNumberException();
        }
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

    public string GetDescription()
    {
        return $"{Type} {Direction} {Duration} {statType}x{Value}";
    }
    
}

public class MapLevelVictoryCondition
{
    public enum ConditionType { InfrastructureActive };
  

    public ConditionType Type = ConditionType.InfrastructureActive;
    public string TargetId;

    
    public VictoryConditionState GetState()
    {
        switch (Type)
        {
            case(ConditionType.InfrastructureActive):
                InfrastructureInstance infrastructureInstance = GameManager.Instance.GetInfrastructureInstanceByID(TargetId);
                if (infrastructureInstance.IsActive())
                {
                    return VictoryConditionState.Succeeded;
                }

                return VictoryConditionState.NotMet;
            break;
            default:
                throw new NotImplementedException();
        }
    }

    public static MapLevelVictoryCondition GetRandomCondition(int stage)
    {
        MapLevelVictoryCondition condition = null;
        int saftyCheck = 0;
        while (condition == null && saftyCheck < 10)
        {
            saftyCheck += 1;

            List<string> infraIds = new List<string>()
            {
                "email-service",
                "sns"
            };
            condition = new MapLevelVictoryCondition();
            int i = Random.Range(0, infraIds.Count);
            condition.TargetId = infraIds[i];
            if (condition.GetState() == VictoryConditionState.NotMet)
            {
                return condition;
            }
        }

        throw new SystemException("Could not find a victory condition that was not met");

    }
}
