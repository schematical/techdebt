// Technology_Locked.cs
using UnityEngine;
using System.Collections.Generic;
using System;
using Tutorial;
using UnityEngine.Serialization; // Added for SerializableAttribute

[Serializable]
public class Technology: iUnlockable, iUIMapNode
{
    public enum State { MetaLocked, Locked, Researching, Unlocked }
    public enum TechTreeDirection { Up, Down, Left, Right }
    public string TechnologyID;
    public string DisplayName;
    public string Description;
    public TechTreeDirection Direction = TechTreeDirection.Up;
    [FormerlySerializedAs("ResearchPointCost")] public int ResearchTime;
    public float CurrentResearchProgress = 0;
    [FormerlySerializedAs("RequiredTechnologies")] public List<UnlockCondition> UnlockConditions;

    public State CurrentState = State.MetaLocked;
    public State OriginalState = State.MetaLocked;
    public TutorialStepId TutorialStepId { get; set; } = TutorialStepId.None;

    // iUIMapNode Implementation
    string iUIMapNode.Id => TechnologyID;
    string iUIMapNode.DisplayName => DisplayName;
    string iUIMapNode.Description => Description;
    MapNodeState iUIMapNode.CurrentState => (MapNodeState)CurrentState;
    MapNodeDirection iUIMapNode.Direction => (MapNodeDirection)Direction;

    public List<string> DependencyIds
    {
        get
        {
            List<string> ids = new List<string>();
            if (UnlockConditions != null)
            {
                foreach (UnlockCondition condition in UnlockConditions)
                {
                    if (condition.Type == UnlockCondition.ConditionType.Technology)
                    {
                        ids.Add(condition.TechnologyID);
                    }
                }
            }
            return ids;
        }
    }

    public bool IsUnlocked()
    {
        return CurrentState == State.Unlocked;
    }

    public void OnInterrupt()
    {
        CurrentState = State.Locked;
    }

    public float GetProgress()
    {
        if (ResearchTime <= 0) return 0;
        return CurrentResearchProgress / ResearchTime;
    }

    public UnityEngine.Tilemaps.TileBase GetTile()
    {
        string tileId = "TechTreeLockedTile";
        switch (CurrentState)
        {
            case State.MetaLocked:
                tileId = "TechTreeLockedTile";
                break;
            case State.Locked:
                tileId = "TechTreeUnlockedTile";
                break;
            case State.Researching:
                tileId = "TechTreeResearching";
                break;
            case State.Unlocked:
                tileId = "TechTreeResearched";
                break;
        }
        return GameManager.Instance.prefabManager.GetTile(tileId);
    }

    public void OnSelected(UI.UIMapPanel panel)
    {
        // Technology selection logic handled by UITechTreePanel or overridden in it.
    }
}
