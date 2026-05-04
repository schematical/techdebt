using System.Collections.Generic;
using DefaultNamespace;
using DefaultNamespace.Rewards;
using MetaChallenges;
using Rewards;
using UnityEngine.Serialization;

public enum MetaResourceType
{
    Technology,
    WorldObject,
    GlobalStatBaseStat,
    // GlobalStatMultiplier,
}



[System.Serializable]
public class MetaStatData
{
    public string statName;
    [FormerlySerializedAs("value")] public int cumulativeValue;
    public int highestValue;
}

[System.Serializable]
public class InfraMetaStatSaveData
{
    public string infraId;
    public List<MetaStatData> stats;

    public InfraMetaStatSaveData()
    {
        stats = new List<MetaStatData>();
    }
}

[System.Serializable]
public class MetaStatSaveData
{
    public List<InfraMetaStatSaveData> infra;
    public List<MetaStatData> game;

    public MetaStatSaveData()
    {
        infra = new List<InfraMetaStatSaveData>();
        game = new List<MetaStatData>();
    }
}

[System.Serializable]
public class MetaMapLevelData
{
    public string levelId;
    public int completedCount;
}

[System.Serializable]
public class MetaProgressData
{
    public int completedRuns;
    public int successfulExits;
    public int researchPoints;
    public int prestigePoints;
    
    public List<MetaPrestigePointAllocation> prestigePointAllocations;
    public List<MetaMapLevelData> mapLevelData;
    public List<string> claimedMetaRewardIds;
    
    public MetaStatSaveData metaStats;
    public GameStage gameStage = GameStage.Bootstrapped;

    public MetaProgressData()
    {
        completedRuns = 0;
        successfulExits = 0;
        researchPoints = 0;
        prestigePoints = 0;
        prestigePointAllocations = new List<MetaPrestigePointAllocation>();
        mapLevelData = new List<MetaMapLevelData>();
        claimedMetaRewardIds = new List<string>();
        metaStats = new MetaStatSaveData();
    }
}
