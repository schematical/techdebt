
using System.Collections.Generic;

public class MapLevelReward
{   
    public enum MapLevelRewardApplied
    {
        Start,
        End
    }

    public string Id;
    public string Description;
    public List<MapLevelVictoryConditionBase> VictoryConditions = new();
    public MapLevelRewardApplied AppliedAt =  MapLevelRewardApplied.End;
    public RewardBase Reward { get; set; }
}