// DeliveryNPC.cs
using UnityEngine;

public class DeliveryNPC : NPCBase
{
    public void Initialize(Vector3 startPosition)
    {
        transform.position = startPosition;
        base.Initialize();
        
        // This NPC only ever performs one task, which is assigned externally.
        // It should never pull from the main task queue.
    }

    protected override void Update()
    {
        // We only call the base Update method, which handles movement and task execution.
        // We do NOT want the state machine from NPCBase's Update that tries to find new work.
        HandleMovement();

        if (CurrentState == State.ExecutingTask && CurrentTask != null)
        {
            CurrentTask.OnUpdate(this);
            if (CurrentTask.IsFinished(this))
            {
                CurrentTask.OnEnd(this);
                CurrentTask = null;
                CurrentState = State.Idle;
            }
        }
    }

    public override bool CanAssignTask(NPCTask task)
    {
        // This NPC can be assigned a task externally (its delivery task),
        // but it should not be able to pull tasks from the general queue.
        // We return false to prevent TryToFindWork from ever assigning a task.
        return false;
    }
}
