using System.Collections.Generic;
using NPCs;
using UI;
using UnityEngine;
using UnityEngine.Tilemaps;

[System.Serializable]
public class StakeholderLevelConfig
{
    public string Title;
    public string Description;
    public int RequiredDesks; // Placeholder for upgrade requirement
}

[System.Serializable]
public class Stakeholder : iUIMapNode
{
    public string Id { get; set; }
    public string RoleName; // e.g., "Marketing", "Engineering"
    
    public int CurrentLevelIndex { get; set; } = 0;
    public List<StakeholderLevelConfig> Levels = new List<StakeholderLevelConfig>();

    public MapNodeState State { get; set; } = MapNodeState.Locked;
    public MapNodeDirection NodeDirection { get; set; } = MapNodeDirection.Right;
    public List<string> Dependencies { get; set; } = new List<string>();

    public NPCBase AttachedNPC { get; set; }

    public string DisplayName => Levels.Count > 0 ? Levels[CurrentLevelIndex].Title : RoleName;
    public string Description => Levels.Count > 0 ? Levels[CurrentLevelIndex].Description : "";
    public MapNodeState? CurrentState => State;
    public MapNodeDirection Direction => NodeDirection;
    public List<string> DependencyIds => Dependencies;

    public bool CanUpgrade()
    {
        if (CurrentLevelIndex >= Levels.Count - 1) return false;
        // Future logic: Check against actual infrastructure (e.g., GameManager.Instance.GetTotalDesks())
        // or a new 'Influence' currency.
        return true; 
    }

    public void Upgrade()
    {
        if (CanUpgrade())
        {
            CurrentLevelIndex++;
            State = MapNodeState.Unlocked;
        }
    }

    public float GetProgress()
    {
        return 0f;
    }

    public TileBase GetTile()
    {
        string tileId = "TechTreeLockedTile";
        switch (CurrentState)
        {
            case MapNodeState.MetaLocked:
                tileId = "TechTreeLockedTile";
                break;
            case MapNodeState.Locked:
                tileId = "TechTreeUnlockedTile";
                break;
            case MapNodeState.Active:
                tileId = "TechTreeResearching";
                break;
            case MapNodeState.Unlocked:
                tileId = "TechTreeResearched";
                break;
        }
        return GameManager.Instance.prefabManager.GetTile(tileId);
    }

    public void OnSelected(UIMapPanel panel)
    {
        panel.UpdateDetailsArea();
    }
}