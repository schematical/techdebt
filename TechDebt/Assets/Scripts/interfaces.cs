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