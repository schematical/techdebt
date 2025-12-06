// UnlockCondition.cs
using System;

[Serializable]
public class UnlockCondition
{
    public enum ConditionType { Day }
    public ConditionType Type = ConditionType.Day;

    public int RequiredValue = 0;
    
    public string GetDescription()
    {
        switch (Type)
        {
            case ConditionType.Day:
                return $"Requires Day {RequiredValue}";
            default:
                return "Unknown requirement";
        }
    }
}

