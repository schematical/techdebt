using System.Collections.Generic;
using DefaultNamespace.Rewards;
using Rewards;
using UnityEngine.Serialization;

public enum MetaResourceType
{
    Technology,
    WorldObject,
    Bonus
}

[System.Serializable]
public class MetaUnlockResource
{
    public MetaResourceType Type;
    public string Id;

    public RewardBase ToReward()
    {
        switch (Type)
        {
            case MetaResourceType.Technology:
                return new TechnologyStartStateReward()
                {
                    TechnologyId = Id,
                    StartState = Technology.State.Unlocked
                };
            // other types can be added here
            default:
                throw new System.NotImplementedException();
        }
    }
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
    
    public List<MetaUnlockResource> prestigePointAllocations;
    public List<MetaMapLevelData> mapLevelData;
    
    public MetaStatSaveData metaStats;

    public MetaProgressData()
    {
        completedRuns = 0;
        successfulExits = 0;
        researchPoints = 0;
        prestigePoints = 0;
        prestigePointAllocations = new List<MetaUnlockResource>();
        mapLevelData = new List<MetaMapLevelData>();
        metaStats = new MetaStatSaveData();
    }
}
