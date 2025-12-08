// Technology.cs
using UnityEngine;
using System.Collections.Generic;
using System; // Added for SerializableAttribute

[Serializable]
public class Technology 
{
    public enum State { Locked, Unlocked }

    public string TechnologyID;
    public string DisplayName;

    public string Description;


    public int ResearchPointCost;
    public List<string> RequiredTechnologies;
    
    public State CurrentState = State.Locked;
}
