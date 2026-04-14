
using System.Collections.Generic;

public class MapLevelReward
{   
    public enum MapLevelRewardApplied
    {
        Start,
        End
    }

    public List<MapLevelVictoryConditionBase> MapLevelVictoryConditionBaseList = new();
    public MapLevelRewardApplied AppliedAt =  MapLevelRewardApplied.End;
    public RewardBase Reward { get; set; }
}