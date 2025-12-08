// UnlockCondition.cs
using System;

[Serializable]
public class UnlockCondition
{

    public StatType Type = StatType.ResearchPoints;

    public int RequiredValue = 0;
    
    public string GetDescription()
    {
        switch (Type)
        {
            case StatType.ResearchPoints:
                return $"Requires ResearchPoints {RequiredValue}";
            default:
                return "Unknown requirement";
        }
    }
}

