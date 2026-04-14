using System;
using System.Collections.Generic;
using System.Linq;
using Stats;
using UnityEngine;
using Random = UnityEngine.Random;


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
    public NetworkPacketData.PType networkPacketType;

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
        return Enum.GetValues(typeof(ModifierDirection))
            .Cast<ModifierDirection>()
            .ToList();
    }


    private static List<ModifierType> GetTypes()
    {
        return Enum.GetValues(typeof(ModifierType))
            .Cast<ModifierType>()
            .ToList();
    }


    private static List<StatType> GetStats()
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
                case (ModifierType.SprintDuration):
                    foreach (ModifierDirection direction in GetDirections())
                    {
                        MapLevelModifier modifier = new MapLevelModifier();
                        modifier.Direction = direction;
                        modifier.Type = type;

                        modifiers.Add(modifier);
                    }

                    break;
                case (ModifierType.Stat):
                    foreach (ModifierDirection direction in GetDirections())
                    {
                        foreach (ModifierDuration duration in GetDurations())
                        {
                            switch (duration)
                            {
                                case (ModifierDuration.Sprint):
                                case (ModifierDuration.LaunchDay):
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
                                case (ModifierDuration.None):
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
            case (ModifierType.SprintDuration):
                level.SprintDuration = (int)CalcValue(level);
                break;
            case (ModifierType.Stat):

                switch (statType)
                {
                    case (StatType.NetworkPacket_Probibility):
                        // Find and apply this to 
                        NetworkPacketData networkPacketData =
                            GameManager.Instance.GetNetworkPacketDataByType(networkPacketType);
                        float val = CalcValue(level);
                        Debug.Log($"MapLevelModifier.Apply  {networkPacketType} = {val}");
                        _statModifier = new StatModifier($"level_modifier_temp_{level.Id}", val);
                        networkPacketData.Stats.AddModifier(statType.Value, _statModifier);
                        break;
                    case (StatType.Traffic):
                    case (StatType.TechDebt_AccumulationRate):
                        _statModifier = new StatModifier($"level_modifier_temp_{level.Id}",
                            CalcValue(level));
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
            case (ModifierType.SprintDuration):
                switch (Direction)
                {
                    case (ModifierDirection.Positive):
                        return 5 + GameManager.Instance.Map.CurrentSprintNumber; // TODO Make this dynamic
                        break;
                    case (ModifierDirection.Negative):
                        return 5 - GameManager.Instance.Map.CurrentSprintNumber; // TODO Make this dynamic
                        break;
                    default:
                        throw new NotFiniteNumberException();
                }
            case (ModifierType.Stat):
                ModifierDirection direction = Direction;
                switch (statType)
                {
                    case (StatType.TechDebt_AccumulationRate):
                        // Invert the direction for calculation purpuses
                        switch (direction)
                        {
                            case (ModifierDirection.Positive):
                                direction = ModifierDirection.Negative;
                                break;
                            case (ModifierDirection.Negative):
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
                    case (ModifierDuration.LaunchDay):
                        baseValue = .25f;
                        break;
                    case (ModifierDuration.Sprint):
                        baseValue = .1f;
                        break;
                    default:
                        throw new NotImplementedException();
                }

                switch (direction)
                {
                    case (ModifierDirection.Positive):
                        return (float)Math.Pow(1 + baseValue, GameManager.Instance.Map.CurrentSprintNumber);

                    case (ModifierDirection.Negative):
                        return (float)Math.Pow(1 - baseValue, GameManager.Instance.Map.CurrentSprintNumber);
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
            case (ModifierType.Stat):
                if (_statModifier != null)
                {
                    _statModifier.Remove();
                }

                break;
            case (ModifierType.SprintDuration):
                break;
            default:
                throw new NotImplementedException();
        }
    }
}