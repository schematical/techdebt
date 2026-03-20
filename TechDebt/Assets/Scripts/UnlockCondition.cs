// UnlockCondition.cs
using System;

[Serializable]
public class UnlockCondition
{

    public enum ConditionType { Technology, Sprint, Tutorial }

    public ConditionType Type;
    public int SprintNumber;
    public string TechnologyID;

    /*public bool IsUnlocked()
    {
        switch (Type)
        {
            case
        }
    }*/
    
    public string GetDescription()
    {
        switch (Type)
        {
            case ConditionType.Technology:
                return $"Requires Technology_Locked {TechnologyID}";
            default:
                return "Unknown requirement";
        }
    }
}

