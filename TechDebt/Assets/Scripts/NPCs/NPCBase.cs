// NPCBase.cs
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using DefaultNamespace;
using MetaChallenges;
using Stats;
using UI;
using Unity.VisualScripting;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.U2D.Animation;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public abstract class NPCBase : MonoBehaviour, IPointerClickHandler, iAssignable, iAttackable, iTargetable, iModifiable
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
  
    private Vector3 _lastPosition;
    public enum CoolDownType { Attack, Consume }
    protected Dictionary<CoolDownType, float> coolDowns = new Dictionary<CoolDownType, float>();
    
    
    void Awake()
    {
        _lastPosition = transform.position;
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

       
    }
    
    public StatsCollection Stats { get; } = new StatsCollection();
    
    // --- Task Management ---
    public NPCTask CurrentTask { get; protected set; }
    public bool IsBusy => CurrentTask != null;
    private float taskCheckTimer = 0f;
    public const float TaskCheckInterval = 1f;

    private WordBubble _currentWordBubble;
    public ShadowObject  shadow;
    public Vector2 shadowOffset = new Vector2(-0.1f, -0.25f);
    public float shadowScale = 0.75f;

    public virtual void Initialize()
    {
        Stats.Clear();
        Stats.Add(new StatData(StatType.NPC_MovmentSpeed, 3f));
        Stats.Add(new StatData(StatType.NPC_HP, 5f));
        Stats.Add(new StatData(StatType.NPC_CoolDown, 1f));
        Stats.Add(new StatData(StatType.NPC_AttackDamage, 1f));
        coolDowns[CoolDownType.Attack] = 5f;
        coolDowns[CoolDownType.Consume] = 5f;
        if (!GameManager.Instance.AllNpcs.Contains(this))
        {
            GameManager.Instance.AllNpcs.Add(this);
        }
        if (shadow == null)
        {
            shadow = GameManager.Instance.prefabManager.Create("Shadow", transform.position).GetComponent<ShadowObject>();
            shadow.Initialize(gameObject, shadowOffset);
            shadow.transform.localScale = new Vector3(shadowScale, shadowScale, 1f);
        }
        shadow.gameObject.SetActive(false);
    }


    public void ShowWordBubble(string message)
    {
        if (_currentWordBubble != null)
        {
            _currentWordBubble.Close();
        }

        _currentWordBubble = GameManager.Instance.prefabManager.Create("WordBubble", transform.position).GetComponent<WordBubble>();
        _currentWordBubble.Setup(message, transform);
    }

    protected virtual void FixedUpdate()
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
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer.color.a == 0)
            {
                gameObject.SetActive(false);
            }
            spriteRenderer.color = new Color(
                spriteRenderer.color.r,
                spriteRenderer.color.g,
                spriteRenderer.color.b,
                spriteRenderer.color.a - Time.fixedDeltaTime
            );
            shadow.gameObject.SetActive(false);
            return;
        }
        foreach (CoolDownType t in coolDowns.Keys.ToArray())
        {
            float coolDown = coolDowns[t];
            if (coolDown > 0f)
            {
                coolDowns[t] = coolDown - Time.deltaTime;
            }
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
        if (IsDead())
        {
            return;
        }
        gameObject.SetActive(true);
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
        if (IsDead())
        {
            return;
        }
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
        if (CurrentState == State.Dead)
        {
            return;
        }
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
            animator.SetBool("isWalking", false);
            return;
        }

        Vector3 targetWaypoint = currentPath[pathIndex];
        
        if (Vector2.Distance(transform.position, targetWaypoint) > 0.01f)
        {
            Vector3 nextPos = Vector2.MoveTowards(transform.position, targetWaypoint, Stats.GetStatValue(StatType.NPC_MovmentSpeed) * Time.deltaTime);
            transform.position = new Vector3(nextPos.x, nextPos.y, targetWaypoint.z);
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
        Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);
        Vector3 lastScreenPos = Camera.main.WorldToScreenPoint(_lastPosition);

        float xMovement = screenPos.x - lastScreenPos.x;
        if (Mathf.Abs(xMovement) > 0.01f) // Add a small threshold to prevent flipping when idle
        {
            if (xMovement > 0)
            {
                FaceRight();
            }
            else
            {
                  FaceLeft();
            }
        }

        float yMovement = screenPos.y - lastScreenPos.y;
        if (Mathf.Abs(yMovement) > 0.01f) // Add a small threshold to prevent flipping when idle
        {
            if (yMovement > 0)
            {
                animator.SetBool("isFront", false);
                FaceUp();
            }
            else
            {
                animator.SetBool("isFront", true);
                FaceDown();
            }
        }
        _lastPosition = transform.position;
    }

    protected virtual void FaceRight()
    {
       
    }
    protected virtual void FaceLeft()
    {
       
    }
    protected virtual void FaceUp()
    {
       
    }
    protected virtual void FaceDown()
    {
       
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
                foreach (StatModifier modifier in stat.Modifiers)
                {

                    content += $"  - {modifier.Id} - {modifier.Value:F2} ({modifier.Type})\n";
                }
            }
        }
        content += "\n<b>Cool Downs:</b>\n";
        foreach (CoolDownType coolDownType in coolDowns.Keys)
        {
            content += $"- {coolDownType}: {coolDowns[coolDownType]}\n";
        }
        return content;
    }


    public void Attack(iAttackable targetNpc)
    {
        targetNpc.ReceiveAttack(this);
        animator.SetBool("isAttacking", true);
        AddXP();
        float coolDownTime = Stats.GetStatValue(StatType.NPC_CoolDown);
        if (coolDownTime == 0f)
        {
            coolDownTime = 5;
        }
        coolDowns[CoolDownType.Attack] = coolDownTime;
    }

    public bool CanTakeAction(CoolDownType t)
    {
        if (!coolDowns.ContainsKey(t))
        {
            return true; // throw new System.Exception($"{gameObject.name} Has no CoolDown {t} ");
        }
        return (coolDowns[t] <= 0f) ;
    }

    public void ResetCooldown(CoolDownType t, float value)
    {
        coolDowns[t] = value;
    }

    public void ReceiveAttack(NPCBase npcBase)
    {
        float damage = npcBase.Stats.GetStatValue(StatType.NPC_AttackDamage);
        ReceiveDamage(damage);
    }

    public void ReceiveDamage(float damage)
    {
        float currentHP = Stats.Stats[StatType.NPC_HP].IncrStat(-1 * damage);
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



    public Vector3 GetInteractionPosition(InteractionType interactionType = InteractionType.Basic)
    {
        return transform.position;
    }

    public void IncrMetaStat(MetaStat metaStat)
    {
        throw new System.NotImplementedException();
    }

    public void IncrMetaStat(MetaStat metaStat, int value = 1)
    {
        // metaStatCollection.Set(MetaStat.Infra_MaxSize, value);
        // Do nothing for now.
    }

    public virtual Vector3 GetHomePoint()
    {
        return GameManager.Instance.GetInfrastructureInstanceByID("door").transform.position;
    }
    public void ZoomToAndFollow()
    {
        GameManager.Instance.cameraController.ZoomToAndFollow(transform);
    }

    public void EndAttackAnimation()
    {
        animator.SetBool("isAttacking", false);
    }

}
