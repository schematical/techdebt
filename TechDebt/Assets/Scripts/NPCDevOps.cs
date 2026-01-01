// NPCDevOps.cs
using UnityEngine;
using UnityEngine.EventSystems; // Add this namespace

public class NPCDevOps : NPCBase, IPointerClickHandler // Implement IPointerClickHandler
{
    



    public NPCDevOpsData Data { get; private set; }

    public NPCTask CurrentTask
    {
        get { return currentTask; }
    }

    private NPCTask currentTask;

    
    public bool IsBusy => currentTask != null;

    private float taskCheckTimer = 0f;
    public const float TaskCheckInterval = 1f;


    public void AssignTask(NPCTask newTask)
    {
        // Debug.Log($"AssignTask - {gameObject.name} - Task: {newTask.GetType().Name}");
        if (currentTask != null)
        {
            currentTask.OnInterrupt();
        }

        currentTask = newTask;
        if (newTask.TryAssign(this))
        {
            CurrentState = State.ExecutingTask;
            currentTask.OnStart(this);
        }
        else
        {
            Debug.LogError($"Failed to assign task {newTask.GetType().Name} to {name}. It might be already assigned.");
            currentTask = null;
        }
    }

    public void Initialize(NPCDevOpsData data)
    {
        Data = data;
        gameObject.name = $"NPCDevOps_{Data.Name}";
    }

    void Update()
    {
        // NPCs should only perform actions during the Play phase.
        if (GameManager.Instance.GameLoopManager.CurrentState != GameLoopManager.GameState.Play)
        {
            base.Update();
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
                        // Debug.Log("NPC Finished Task:");
                        currentTask.OnEnd(this);
                        currentTask = null; // Clear the completed task
                        CurrentState = State.Idle;
                    }
                    else
                    {
                        taskCheckTimer += Time.deltaTime;
                        if (taskCheckTimer >= TaskCheckInterval)
                        {
                            taskCheckTimer = 0f;
                            CheckForHigherPriorityTask();
                        }
                    }
                }
                else
                {
                    // Debug.Log("NPC No Task:");
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

    private void CheckForHigherPriorityTask()
    {
        NPCTask highestPriorityTask = GameManager.Instance.GetHighestPriorityTask();
        if (highestPriorityTask != null && highestPriorityTask.Priority > currentTask.Priority)
        {
            // Debug.Log($"CheckForHigherPriorityTask - {gameObject.name} - highestPriorityTask: {highestPriorityTask.Priority} > {currentTask.Priority}");
            AssignTask(highestPriorityTask);
        }
    }

    public float GetResearchPointsPerSecond(Technology technology)
    {
        // Later, this could be influenced by the NPC's skills or the technology type
        return 1f;
    }
    public void OnBuildPhaseStart()
    {
        if (currentTask != null)
        {
            currentTask.Unassign();
            currentTask = null;
        }

        StopMovement();
        CurrentState = State.Idle;
    }
    private void TryToFindWork()
    {
        if (CurrentState != State.Idle) return;
        // Debug.Log($"TryToFindWork - {gameObject.name} - CurrentState: {CurrentState}");
        NPCTask availableTask = GameManager.Instance.GetHighestPriorityTask();
        if (availableTask != null)
        {
            AssignTask(availableTask);
        }
        else
        {
            // No tasks available, start wandering
            Wander();
        }
    }

    public override void OnPlayPhaseStart()
    {
        if (currentTask != null)
        {
            currentTask.Unassign();
            currentTask = null;
        }

        base.OnPlayPhaseStart();
    }

   

    // IPointerClickHandler implementation
    public void OnPointerClick(PointerEventData eventData)
    {
        GameManager.Instance.UIManager.ShowNPCDetail(this);
    }
}