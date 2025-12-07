// NPCDevOps.cs
using UnityEngine;

public class NPCDevOps : NPCBase
{
    public enum State { Idle, ExecutingTask, Wandering }
    public State CurrentState { get; set; } = State.Idle;

    public NPCDevOpsData Data { get; private set; }
    private NPCTask currentTask;

    private Vector3 wanderDestination;
    public float wanderRadius = 10f;


    public void Initialize(NPCDevOpsData data)
    {
        Data = data;
        gameObject.name = $"NPCDevOps_{Data.Name}";
    }

    void Update()
    {
        // NPCs should only perform actions during the Play phase.
        if (GameLoopManager.Instance.CurrentState != GameLoopManager.GameState.Play)
        {
            return;
        }

        switch (CurrentState)
        {
            case State.Idle:
                TryToFindWork();
                break;
            
            case State.ExecutingTask:
                if (currentTask != null)
                {
                    currentTask.OnUpdate(this);
                   
                    if (currentTask.IsFinished(this))
                    {
       					Debug.Log("NPC Finished Task:");
                        currentTask.OnEnd(this);
                        currentTask = null; // Clear the completed task
                        CurrentState = State.Idle;
                    }
                }
                else
                {
                    Debug.Log("NPC No Task:");
                    // If task is null for some reason, go back to idle.
                    CurrentState = State.Idle;
                }
                break;

            case State.Wandering:
                if (!isMoving)
                {
                    CurrentState = State.Idle;
                    // Consider adding a small delay here before looking for work again
                }
                break;
        }
		base.Update();
    }

    public void OnPlayPhaseStart()
    {
        // Unassign from current task so it can be picked up again later
        if (currentTask != null)
        {
            currentTask.Unassign();
            currentTask = null;
        }
        
        StopMovement();
        CurrentState = State.Idle;
    }

    public void OnBuildPhaseStart()
    {
        // Unassign from current task so it can be picked up again later
        if (currentTask != null)
        {
            currentTask.Unassign();
            currentTask = null;
        }

        // Stop moving and go idle
        StopMovement();
        CurrentState = State.Idle;
    }

    private void TryToFindWork()
    {
        if (CurrentState != State.Idle) return;

        currentTask = GameManager.Instance.RequestTask(this);
        if (currentTask != null)
        {
			Debug.Log("NPC Starting Task:");
            CurrentState = State.ExecutingTask;
            currentTask.OnStart(this);
        }
        else
        {
            // No tasks available, start wandering
            Wander();
        }
    }
    
    private void Wander()
    {
        if (GetRandomWalkablePoint(transform.position, wanderRadius, out wanderDestination))
        {
            CurrentState = State.Wandering;
            MoveTo(wanderDestination);
        }
        else
        {
            // Can't find a wander point, just stay idle for a bit.
            // A coroutine could handle a delay here before trying again.
        }
    }

    private bool GetRandomWalkablePoint(Vector3 origin, float radius, out Vector3 result)
    {
        for (int i = 0; i < 30; i++)
        {
            Vector3 randomDirection = Random.insideUnitSphere * radius;
            randomDirection += origin;
            
            Node node = GridManager.Instance.NodeFromWorldPoint(randomDirection);
            if (node != null && node.isWalkable)
            {
                result = randomDirection;
                return true;
            }
        }
        result = Vector3.zero;
        return false;
    }
}