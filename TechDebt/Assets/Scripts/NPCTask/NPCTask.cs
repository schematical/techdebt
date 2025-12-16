// NPCTask.cs
using UnityEngine;

public abstract class NPCTask
{
    public enum Status
    {
        Pending,
        Executing,
        Completed,
        Interrupted
    }

    public Status CurrentStatus { get; protected set; } = Status.Pending;
    public int Priority { get; set; }
    public NPCDevOps AssignedNPC { get; private set; }
    public bool IsAssigned => AssignedNPC != null;

    protected Vector3? destination;
    protected bool hasArrived;

    public NPCTask(Vector3? destination = null)
    {
        this.destination = destination;
        this.hasArrived = false;
    }

    public virtual void OnInterrupt()
    {
        CurrentStatus = Status.Interrupted;
        if (AssignedNPC != null && destination.HasValue)
        {
            AssignedNPC.OnDestinationReached -= HandleArrival;
        }
        hasArrived = false;
        Unassign();
    }

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

    public virtual void OnStart(NPCDevOps npc)
    {
        if (destination.HasValue)
        {
            npc.OnDestinationReached += HandleArrival;
            npc.MoveTo(destination.Value);
        }
        else
        {
            hasArrived = true;
        }
    }

    public virtual void OnEnd(NPCDevOps npc)
    {
        if (destination.HasValue)
        {
            npc.OnDestinationReached -= HandleArrival;
        }
    }
    
    private void HandleArrival()
    {
        hasArrived = true;
        if (AssignedNPC != null)
        {
            AssignedNPC.OnDestinationReached -= HandleArrival;
        }
    }

    // Abstract methods to be implemented by concrete tasks
    public abstract void OnUpdate(NPCDevOps npc);
    public abstract bool IsFinished(NPCDevOps npc);
}