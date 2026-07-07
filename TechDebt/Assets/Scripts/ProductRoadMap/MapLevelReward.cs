
using System.Collections.Generic;
using DefaultNamespace;

public class MapLevelReward
{   
    public enum MapLevelRewardApplied
    {
        Start,
        End
    }
    public enum MapLevelRewardType
    {
        Normal, // Applies every time.
        Meta // Can only be applied once
    }
    public string Id;
    public string Description;
    public MapLevelRewardType Type = MapLevelRewardType.Normal; 
    public List<MapLevelVictoryConditionBase> VictoryConditions = new();
    public MapLevelRewardApplied AppliedAt =  MapLevelRewardApplied.End;
    public List<string> DependencyIds = new();
    public RewardBase Reward { get; set; }
    public GameStage GameStage { get; set; }
}