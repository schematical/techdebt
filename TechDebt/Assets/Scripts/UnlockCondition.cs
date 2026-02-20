// UnlockCondition.cs
using System;

[Serializable]
public class UnlockCondition
{

    public enum ConditionType { Technology }

    public ConditionType Type;

    public string TechnologyID;
    
    
    
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

