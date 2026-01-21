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

    public bool isCloseEnough()
    {
        if (destination == null)
        {
            return false;
        }

        if (AssignedNPC == null)
        {
            Debug.LogError("AssignedNPC is null");
            return false;
        }
        return Vector3.Distance(destination.Value, AssignedNPC.transform.position) < 1f;
    }
    public TaskRole Role { get; private set; } = TaskRole.DevOps;
    public NPCTask(Vector3? destination = null)
    {
        this.destination = destination;
     
        
    }

    public virtual void OnInterrupt()
    {
        CurrentState = State.Interrupted;

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

        CurrentState = State.Queued;
    }

    public virtual void OnStart(NPCBase npc)
    {
        if (destination == null)
        {
            Debug.LogWarning("No destination assigned");
            return;
        }
        else
        {
            npc.MoveTo(destination.Value);
        }
    }

    public virtual void OnEnd(NPCBase npc)
    {
        GameManager.Instance.CompleteTask(this);
    }
    
    public void HandleArrival()
    {
     
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
    public virtual string GetDescription()
    {
        return $"State: {CurrentState} - Priority: {Priority} - `isCloseEnough`: {isCloseEnough()}";
    }
}