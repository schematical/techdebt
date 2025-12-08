// UnlockCondition.cs
using System;

[Serializable]
public class UnlockCondition
{

    public enum ConditionType { Stat, Technology }

    public ConditionType Type;
    public StatType StatType = StatType.Money;
    
    public string TechnologyID;

    public int RequiredValue = 0;
    
    
    public string GetDescription()
    {
        switch (Type)
        {
            case ConditionType.Technology:
                return $"Requires Technology {TechnologyID}";
            default:
                return "Unknown requirement";
        }
    }
}

