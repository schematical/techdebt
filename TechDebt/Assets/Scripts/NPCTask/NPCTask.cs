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

    public enum TaskRole
    {
        DevOps,
        Boss,
        Dev,
        Intern,
        Enemy
    }

    public State CurrentState { get; protected set; } = State.Available;
    public int Priority { get; set; }
    public NPCBase AssignedNPC { get; private set; }
    public bool IsAssigned => AssignedNPC != null;

    protected iTargetable? target;
    protected float maxTaskRange = 1f;

    public bool IsCloseEnough()
    {


        if (AssignedNPC == null)
        {
            Debug.LogError("AssignedNPC is null");
            return false;
        }

        if (target == null)
        {
            throw new SystemException("`target` is null");
        }

        return Vector3.Distance(target.GetInteractionPosition(), AssignedNPC.transform.position) <= maxTaskRange;
    }

    public TaskRole Role { get; private set; } = TaskRole.DevOps;

    public NPCTask(iTargetable target = null, int priority = 5)
    {
        this.target = target;
        this.Priority = priority;


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
        if (target == null)
        {
            return;
        }

        npc.MoveTo(target.GetInteractionPosition());

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
        Debug.Log($"target:" + (target == null ? "null" : target.name));
        string description = $"State: {CurrentState} " +
                             $"{target.name} - Priority: {Priority} - `";
        if (AssignedNPC != null) {
            description += $"isCloseEnough`: {IsCloseEnough()} - " +
                           $" Dist: {Vector3.Distance(target.GetInteractionPosition(), AssignedNPC.transform.position)} Range: {maxTaskRange}";
        }/*
        else
        {
            description += " No Assigned NPC";
        }*/
        return description;
}

}