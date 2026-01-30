// NPCBase.cs
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Stats;
using UI;
using Unity.VisualScripting;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public abstract class NPCBase : MonoBehaviour, IPointerClickHandler, iAssignable, iAttackable, iTargetable
{
    public enum State
    {
        Idle,
        ExecutingTask,
        Wandering,
        Exiting,
        Exited,
        Dead
    }

    [field: SerializeField]public bool isDebugging { get; set; } = false;
    [field: SerializeField]public bool flipMoventSprite { get; set; } = false;
    public UIAttentionIcon uiAttentionIcon;
    public State CurrentState { get; set; } = State.Idle;
    public event System.Action OnDestinationReached;
    public Animator animator;
    public bool isMoving { get; private set; } = false;

    private List<Vector3> currentPath;
    private int pathIndex;
    private SpriteRenderer _spriteRenderer;
    private Vector3 _lastPosition;
    
    void Awake()
    {
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        _lastPosition = transform.position;
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

       
    }
    
    public StatsCollection Stats = new StatsCollection();
    
    // --- Task Management ---
    public NPCTask CurrentTask { get; protected set; }
    public bool IsBusy => CurrentTask != null;
    private float taskCheckTimer = 0f;
    public const float TaskCheckInterval = 1f;

    private WordBubble _currentWordBubble;


    public virtual void Initialize()
    {
        Stats.Clear();
        Stats.Add(new StatData(StatType.NPC_MovmentSpeed, 3f));
        Stats.Add(new StatData(StatType.NPC_HP, 1f));
        if (!GameManager.Instance.AllNpcs.Contains(this))
        {
            GameManager.Instance.AllNpcs.Add(this);
        }
    }


    public void ShowWordBubble(string message)
    {
        if (_currentWordBubble != null)
        {
            _currentWordBubble.Close();
        }

        GameObject bubbleGO = GameManager.Instance.prefabManager.GetPrefab("WordBubble");
        if (bubbleGO != null)
        {
            GameObject instance = Instantiate(bubbleGO, transform.position, Quaternion.identity);
            _currentWordBubble = instance.GetComponent<WordBubble>();
            if (_currentWordBubble != null)
            {
                _currentWordBubble.Setup(message, transform);
            }
            else
            {
                Debug.LogError("The 'WordBubble' prefab does not have a WordBubble component attached.", this);
                Destroy(instance);
            }
        }
    }

    protected virtual void Update()
    {
        if (
            
            GameManager.Instance.GameLoopManager.CurrentState != GameLoopManager.GameState.Play &&
            GameManager.Instance.GameLoopManager.CurrentState != GameLoopManager.GameState.WaitingForNpcsToExpire
         )   
        {
            HideAttentionIcon();
            return;
        }
        if(
            GameManager.Instance.UIManager.GetCurrentTimeState() == UIManager.TimeState.Paused
        )
        {
            return;
        }

        if (CurrentState == State.Dead)
        {
            return;
        }
        HandleMovement();

        switch (CurrentState)
        {  /* case(State.Dead):
            //Do nothing for now.
                break;*/
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
        return false;
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
        Vector3 wanderDestination = GetRandomWalkablePoint(GetHomePoint(), 10f);
  
        if (!Vector3.zero.Equals(wanderDestination))
        {
            CurrentState = State.Wandering;
            MoveTo(wanderDestination);

        }
        else
        {
            Debug.LogError($"{gameObject.name} could not find a walkable point for {wanderDestination}.");
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
    public void OnPlanPhaseStart()
    {
        if (CurrentTask != null)
        {
            CurrentTask.Unassign();
        }
        HideAttentionIcon();
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
        animator.SetBool("isWalking", isMoving);
    }

    public void StopMovement()
    {
        isMoving = false;
        currentPath = null;
        pathIndex = 0;
        animator.SetBool("isWalking", isMoving);
    }


    public void EndDay()
    {
        CurrentState = State.Exiting;
        if (CurrentTask != null)
        {
            CurrentTask.OnInterrupt();
        }
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
        targetWaypoint.z = transform.position.z; // 1 - targetWaypoint.y * -0.1f;
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
                animator.SetBool("isWalking", isMoving);
                currentPath = null;
                if (CurrentTask != null)
                {
                    CurrentTask.HandleArrival();
                }
                OnDestinationReached?.Invoke();
            }
        }
        
        // --- Sprite Flipping Logic ---
        float xMovement = transform.position.x - _lastPosition.x;
        if (Mathf.Abs(xMovement) > 0.01f) // Add a small threshold to prevent flipping when idle
        {
            if (xMovement > 0)
            {
                _spriteRenderer.flipX = flipMoventSprite; // Moving right
            }
            else
            {
                _spriteRenderer.flipX = !flipMoventSprite; // Moving left
            }
        }
        _lastPosition = transform.position;
    }

    public virtual void AddXP(float ammount = 1)
    {
     
    }
    public virtual void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            OnLeftClick(eventData);
        }
    }

    public virtual void OnLeftClick(PointerEventData eventData)
    {
        GameManager.Instance.UIManager.npcDetailPanel.Show(this);
    }
    public virtual List<NPCTask> GetAvailableTasks()
    {
        return new List<NPCTask>();
    }

    public void ShowAttentionIcon(UnityAction onClick)
    {
        if (uiAttentionIcon != null && uiAttentionIcon.gameObject.activeSelf)
        {
            Debug.LogWarning($"{gameObject.name} - uiAttentionIcon is already active");
            return;
        }
        uiAttentionIcon = GameManager.Instance.UIManager.AddAttentionIcon(
            transform,
            Color.purple,
            () =>
            {
                GameManager.Instance.cameraController.ZoomTo(transform, () =>
                {
                    onClick.Invoke();
                });
               
            }
        );
    }

    public void HideAttentionIcon()
    {
        if (uiAttentionIcon == null)
        {
            return;
        }
        uiAttentionIcon.gameObject.SetActive(false);
        uiAttentionIcon = null;
    }

    protected void DebugLog(string message)
    {
        if (isDebugging)
        {
            Debug.Log($"{gameObject.name} - {message}");
        }
    }

    public virtual string GetDetailText()
    {
        string content = $"<b>{name}</b>\n";
        // Add the current task
        content += $"\nState: {CurrentState}\n";
        if (CurrentTask != null)
        {
            content += $"Task: {CurrentTask.GetDescription()}\n\n";
        }
        else
        {
            content += "Task: None\n\n";
        }
        content += "\n<b>Stats:</b>\n";

        foreach (var stat in Stats.Stats.Values)
        {
            content += $"- {stat.Type}: {stat.Value:F2} (Base: {stat.BaseValue:F2})\n";
            if (stat.Modifiers.Any())
            {
                content += "  <i>Modifiers:</i>\n";
                foreach (var mod in stat.Modifiers)
                {
                    string sourceName = mod.Source != null ? mod.Source.GetType().Name : "Unknown";
                    content += $"  - {mod.Value:F2} ({mod.Type}) @ {sourceName}\n";
                }
            }
        }
        return content;
    }


    public void Attack(iAttackable targetNpc)
    {
        targetNpc.ReceiveAttack(this);
        animator.SetBool("isAttacking", true);
        AddXP();
    }

    public void ReceiveAttack(NPCBase npcBase)
    {
        float damage = -1;
        float currentHP = Stats.Stats[StatType.NPC_HP].IncrStat(damage);
        GameManager.Instance.FloatingTextFactory.ShowText($"{damage} HP",
            transform.position); 
        
        if (currentHP <= 0)
        {
            SetState(State.Dead);
        }
    }

    protected void SetState(State state)
    {
        switch (state)
        {
            case(State.Dead):
                    StopMovement();
                    animator.SetBool("isDead", true);
                break;
        }
        CurrentState = state;
    }

    public bool IsDead()
    {
        return CurrentState == State.Dead;
    }

    protected void FixedUpdate()
    {
    }

    public Vector3 GetInteractionPosition()
    {
        return transform.position;
    }

    public virtual Vector3 GetHomePoint()
    {
        return GameManager.Instance.GetInfrastructureInstanceByID("door").transform.position;
    }
    public void ZoomToAndFollow()
    {
        GameManager.Instance.cameraController.ZoomToAndFollow(transform);
    }
}
