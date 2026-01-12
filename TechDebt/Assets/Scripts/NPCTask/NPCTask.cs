// NPCTask.cs

using System;
using UnityEngine;

public abstract class NPCTask
{
    public enum State
    {
        Available,
        Queued,
        Executing,
        Completed,
        Interrupted
    }
    public enum TaskRole { DevOps, Boss, Dev, Intern}
    public State CurrentState { get; protected set; } = State.Available;
    public int Priority { get; set; }
    public NPCBase AssignedNPC { get; private set; }
    public bool IsAssigned => AssignedNPC != null;

    protected Vector3? destination;
    protected bool hasArrived;
    public TaskRole Role { get; private set; } = TaskRole.DevOps;
    public NPCTask(Vector3? destination = null)
    {
        this.destination = destination;
        hasArrived = false;
        
    }

    public virtual void OnInterrupt()
    {
        CurrentState = State.Interrupted;
        if (AssignedNPC != null && destination.HasValue)
        {
            AssignedNPC.OnDestinationReached -= HandleArrival;
        }
        hasArrived = false;
        Unassign();
    }

    // Method to assign an NPC to this task
    public bool TryAssign(NPCBase npc)
    {
        if (IsAssigned)
        {
            return false;
        }
        AssignedNPC = npc;
        CurrentState = State.Executing;
        return true;
    }

    public void Unassign()
    {
        AssignedNPC = null;
        hasArrived = false;
        CurrentState = State.Queued;
    }

    public virtual void OnStart(NPCBase npc)
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

    public virtual void OnEnd(NPCBase npc)
    {
        if (destination.HasValue)
        {
            npc.OnDestinationReached -= HandleArrival;
        }

        GameManager.Instance.CompleteTask(this);
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
    public abstract void OnUpdate(NPCBase npc);
    public abstract bool IsFinished(NPCBase npc);
    
    public virtual string GetAssignButtonText()
    {
        throw new SystemException("Overwrite me");
    }

    public virtual void OnQueued()
    {
        CurrentState = State.Queued;
    }
}