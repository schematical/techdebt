using System.Collections.Generic;
using NPCs;
using UI;
using UnityEngine;
using UnityEngine.Tilemaps;

[System.Serializable]
public class Stakeholder : UIMetaUnlockMapLeveledNode
{
    public string RoleName; 
    
    public NPCBase AttachedNPC { get; set; }

    public override string DisplayName
    {
        get
        {
            if (Levels != null && Levels.Count > 0)
            {
                int idx = CurrentLevelIndex;
                if (idx < 0) idx = 0; // Use lowest level if not unlocked
                if (idx < Levels.Count)
                    return Levels[idx].DisplayName;
            }
            return base.DisplayName ?? RoleName;
        }
        set => base.DisplayName = value;
    }

    public override string Description
    {
        get
        {
            if (Levels != null && Levels.Count > 0)
            {
                int idx = CurrentLevelIndex;
                if (idx < 0) idx = 0; // Use lowest level if not unlocked
                if (idx < Levels.Count)
                    return Levels[idx].Description;
            }
            return base.Description;
        }
        set => base.Description = value;
    }

    public bool CanUpgrade()
    {
        if (Levels == null || CurrentLevelIndex >= Levels.Count - 1) return false;
        return true; 
    }

    public void Upgrade()
    {
        if (CanUpgrade())
        {
            // Note: In the meta-progression system, this is handled via MetaGameManager.
            // This method might be for in-game upgrades if they are separate.
            MetaGameManager.UpdatePrestigePointAllocation(AllocationId, CurrentLevelIndex + 2);
        }
    }

    public override float GetProgress()
    {
        return 0f;
    }

    public override TileBase GetTile()
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

    public override void OnSelected(UIMapPanel panel)
    {
        panel.UpdateDetailsArea();
    }
}