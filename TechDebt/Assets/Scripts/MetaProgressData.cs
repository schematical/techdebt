using System.Collections.Generic;

[System.Serializable]
public class MetaProgressData
{
    public int researchPoints;
    public List<string> unlockedNodeIds;

    public MetaProgressData()
    {
        researchPoints = 0;
        unlockedNodeIds = new List<string>();
    }
}
