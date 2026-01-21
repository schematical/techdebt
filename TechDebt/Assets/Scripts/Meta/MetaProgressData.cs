using System.Collections.Generic;
using UnityEngine.Serialization;

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
    public List<MetaStatData>  game;

    public MetaStatSaveData()
    {
        infra = new List<InfraMetaStatSaveData>();
        game = new List<MetaStatData>();
    }
}

[System.Serializable]
public class MetaProgressData
{
    public int researchPoints;
    public List<string> unlockedNodeIds;
    public MetaStatSaveData metaStats;

    public MetaProgressData()
    {
        researchPoints = 0;
        unlockedNodeIds = new List<string>();
        metaStats = new MetaStatSaveData();
    }
}
