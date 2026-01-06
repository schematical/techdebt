// NPCBase.cs
using UnityEngine;
using System.Collections.Generic;
using Stats;

public abstract class NPCBase : MonoBehaviour
{
    public enum State
    {
        Idle,
        ExecutingTask,
        Wandering,
        Exiting,
        Exited
    }
    public State CurrentState { get; set; } = State.Idle;
    public event System.Action OnDestinationReached;
    
    public bool isMoving { get; private set; } = false;

    private List<Vector3> currentPath;
    private int pathIndex;
    private SpriteRenderer _spriteRenderer;
    private Vector3 _lastPosition;
    
    void Awake()
    {
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        _lastPosition = transform.position;
    }
    
    public StatsCollection Stats = new StatsCollection();
    
    // --- Task Management ---
    public NPCTask CurrentTask { get; protected set; }
    public bool IsBusy => CurrentTask != null;
    private float taskCheckTimer = 0f;
    public const float TaskCheckInterval = 1f;


    public virtual void Initialize()
    {
        Stats.Clear();
        Stats.Add(new StatData(StatType.NPC_MovmentSpeed, 1.5f));
    }

    protected virtual void Update()
    {
        HandleMovement();

        switch (CurrentState)
        {
            case State.Idle:
                TryToFindWork();
                break;

            case State.ExecutingTask:
                if (CurrentTask != null)
                {
                    CurrentTask.OnUpdate(this);

                    if (CurrentTask.IsFinished(this))
                    {
                        CurrentTask.OnEnd(this);
                        CurrentTask = null; 
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
                    CurrentState = State.Idle;
                }
                break;

            case State.Wandering:
                if (!isMoving)
                {
                    CurrentState = State.Idle;
                }
                break;
        }
    }

    public virtual bool CanAssignTask(NPCTask task)
    {
        return task != null;
    }

    public void AssignTask(NPCTask newTask)
    {
        if (CurrentTask != null)
        {
            CurrentTask.OnInterrupt();
        }

        CurrentTask = newTask;
        if (newTask.TryAssign(this))
        {
            CurrentState = State.ExecutingTask;
            CurrentTask.OnStart(this);
        }
        else
        {
            Debug.LogError($"Failed to assign task {newTask.GetType().Name} to {name}. It might be already assigned.");
            CurrentTask = null;
        }
    }

    private void CheckForHigherPriorityTask()
    {
        if (CurrentTask == null) return;
        
        NPCTask highestPriorityTask = GameManager.Instance.GetHighestPriorityTask();
        if (highestPriorityTask != null && highestPriorityTask.Priority > CurrentTask.Priority && CanAssignTask(highestPriorityTask))
        {
            AssignTask(highestPriorityTask);
        }
    }
    
    protected virtual void TryToFindWork()
    {
        if (CurrentState != State.Idle) return;
        
        NPCTask availableTask = GameManager.Instance.GetHighestPriorityTask();
        if (availableTask != null && CanAssignTask(availableTask))
        {
            AssignTask(availableTask);
        }
        else
        {
            TriggerDefaultBehavior();
        }
    }

    public virtual void TriggerDefaultBehavior()
    {
        Wander();
    }
    public void Wander()
    {
        Vector3 wanderDestination = GetRandomWalkablePoint(transform.position, 10f);
  
        if (!Vector3.zero.Equals(wanderDestination))
        {
            CurrentState = State.Wandering;
            MoveTo(wanderDestination);
        }
    }
    
    private Vector3 GetRandomWalkablePoint(Vector3 origin, float radius)
    {
        for (int i = 0; i < 30; i++)
        {
            Vector3 randomDirection = Random.insideUnitSphere * radius;
            randomDirection += origin;

            Node node = GridManager.Instance.NodeFromWorldPoint(randomDirection);
            if (node != null && node.isWalkable)
            {
                return randomDirection;
            }
        }
        
        return Vector3.zero;
    }

    public virtual void OnPlayPhaseStart()
    {
        if (CurrentTask != null)
        {
            CurrentTask.Unassign();
            CurrentTask = null;
        }
        StopMovement();
        CurrentState = State.Idle;
    }
    
    public void MoveTo(Vector3 destination)
    {
        List<Vector3> path = Pathfinding.FindPath(transform.position, destination);
        if (path != null && path.Count > 0)
        {
            currentPath = path;
            pathIndex = 0;
            isMoving = true;
        }
        else
        {
            Debug.LogWarning($"{gameObject.name} could not find a path to {destination}.");
            isMoving = false;
        }
    }

    public void StopMovement()
    {
        isMoving = false;
        currentPath = null;
        pathIndex = 0;
    }


    public void EndDay()
    {
        CurrentState = State.Exiting;
        InfrastructureInstance doorInstance = GameManager.Instance.GetInfrastructureInstanceByID("door");
        if (doorInstance != null)
        {
            MoveTo(doorInstance.transform.position);
        }
        else
        {
            Debug.LogError($"NavigateToDoorTask: Door infrastructure with ID 'door' not found.");
        }
    }
    
    protected void HandleMovement()
    {
        if (!isMoving || currentPath == null || pathIndex >= currentPath.Count)
        {
            if (CurrentState == State.Exiting)
            {
                CurrentState = State.Exited;
                gameObject.SetActive(false);
            }
            return;
        }

        Vector3 targetWaypoint = currentPath[pathIndex];
        targetWaypoint.z = transform.position.z; 

        if (Vector3.Distance(transform.position, targetWaypoint) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetWaypoint, Stats.GetStatValue(StatType.NPC_MovmentSpeed) * Time.deltaTime);
        }
        else
        {
            pathIndex++;
            if (pathIndex >= currentPath.Count)
            {
                isMoving = false;
                currentPath = null;
                OnDestinationReached?.Invoke();
            }
        }
        
        // --- Sprite Flipping Logic ---
        float xMovement = transform.position.x - _lastPosition.x;
        if (Mathf.Abs(xMovement) > 0.01f) // Add a small threshold to prevent flipping when idle
        {
            if (xMovement > 0)
            {
                _spriteRenderer.flipX = false; // Moving right
            }
            else
            {
                _spriteRenderer.flipX = true; // Moving left
            }
        }
        _lastPosition = transform.position;
    }
}
