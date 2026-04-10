using System.Collections.Generic;
using Infrastructure;
using MetaChallenges;
using Stats;
using UnityEngine;

public interface iProgressable
{
    float GetProgress();
}

public interface iTargetable
{

    Transform transform { get; }
    string name { get; }
    GameObject gameObject { get;  }
    public Vector3 GetInteractionPosition(InteractionType interactionType = InteractionType.Basic);
    void IncrMetaStat(MetaStat metaStat, int value = 1);
}
public interface iAttackable: iTargetable
{
    public void ReceiveAttack(NPCBase npcBase);
    Transform transform { get; }
    string name { get; }
    public bool IsDead();
}

public interface iModifiable
{
    public StatsCollection Stats { get; }
}
public interface iUnlockable
{
    public bool IsUnlocked();
}

public enum MapNodeState
{
    MetaLocked,
    Locked,
    Active, // e.g. Researching
    Unlocked
}

public enum MapNodeDirection
{
    Up,
    Down,
    Left,
    Right
}

public interface iUIMapNode
{
    string Id { get; }
    string DisplayName { get; }
    string Description { get; }
    MapNodeState CurrentState { get; }
    MapNodeDirection Direction { get; }
    List<string> DependencyIds { get; }
    
    // Progress for Active state (0.0 to 1.0)
    float GetProgress();

    UnityEngine.Tilemaps.TileBase GetTile();

    void OnSelected(UI.UIMapPanel panel);
}