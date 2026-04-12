using System.Collections.Generic;
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
public class MetaProgressData
{
    public int completedRuns;
    public int successfulExits;
    public int researchPoints;
    public int prestigePoints;
    
    public List<MetaUnlockResource> prestigePointAllocations;
    
    public MetaStatSaveData metaStats;

    public MetaProgressData()
    {
        completedRuns = 0;
        successfulExits = 0;
        researchPoints = 0;
        prestigePoints = 0;
        prestigePointAllocations = new List<MetaUnlockResource>();
        metaStats = new MetaStatSaveData();
    }
}
