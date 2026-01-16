using System.Collections.Generic;

[System.Serializable]
public class MetaStatPair
{
    public string statName;
    public int value;
}

[System.Serializable]
public class InfraMetaStatSaveData
{
    public string infraId;
    public List<MetaStatPair> stats;

    public InfraMetaStatSaveData()
    {
        stats = new List<MetaStatPair>();
    }
}

[System.Serializable]
public class MetaStatSaveData
{
    public List<InfraMetaStatSaveData> infra;
    public List<MetaStatPair>  game;

    public MetaStatSaveData()
    {
        infra = new List<InfraMetaStatSaveData>();
        game = new List<MetaStatPair>();
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
