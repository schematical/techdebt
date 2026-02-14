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

    protected override void FixedUpdate()
    {
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

    public override void OnPlayPhaseStart()
    {
        // Do nothing
    }
}
