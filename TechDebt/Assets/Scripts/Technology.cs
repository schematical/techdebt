// Technology_Locked.cs
using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.Serialization; // Added for SerializableAttribute

[Serializable]
public class Technology
{
    public enum State { MetaLocked, Locked, Researching, Unlocked }
    public enum TechTreeDirection { Up, Down, Left, Right }
    public string TechnologyID;
    public string DisplayName;
    public string Description;
    public TechTreeDirection Direction = TechTreeDirection.Up;
    [FormerlySerializedAs("ResearchPointCost")] public int ResearchTime;
    public float CurrentResearchProgress = 0;
    public List<string> RequiredTechnologies;

    public State CurrentState = State.MetaLocked;
    public State OriginalState = State.MetaLocked;

    public void OnInterrupt()
    {
        CurrentState = State.Locked;
    }

    public float GetProgress()
    {
        return CurrentResearchProgress / ResearchTime;
    }
}
