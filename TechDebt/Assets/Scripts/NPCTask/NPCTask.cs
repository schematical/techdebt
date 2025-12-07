// NPCTask.cs
using UnityEngine;

public abstract class NPCTask
{
    public enum Status
    {
        Pending,
        Executing,
        Completed
    }

    public Status CurrentStatus { get; protected set; } = Status.Pending;
    public int Priority { get; protected set; }
    public NPCDevOps AssignedNPC { get; private set; }
    public bool IsAssigned => AssignedNPC != null;

    // Method to assign an NPC to this task
    public bool TryAssign(NPCDevOps npc)
    {
        if (IsAssigned)
        {
            return false;
        }
        AssignedNPC = npc;
        CurrentStatus = Status.Executing;
        return true;
    }

    public void Unassign()
    {
        AssignedNPC = null;
        CurrentStatus = Status.Pending;
    }

    // Abstract methods to be implemented by concrete tasks
    public abstract void OnStart(NPCDevOps npc);
    public abstract void OnUpdate(NPCDevOps npc);
    public abstract void OnEnd(NPCDevOps npc);
    public abstract bool IsFinished(NPCDevOps npc);
}