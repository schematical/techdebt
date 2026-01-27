
using Items;
using UnityEngine;

public class BugConsumeItemTask : NPCTask
{
    private ItemBase targetItem;

    public BugConsumeItemTask(ItemBase _targetItem)
    {
        // Find the nearest active ItemBase without a task
       
        if (_targetItem == null)
        {
            throw new System.Exception("targetItem is null");
        }

        targetItem = _targetItem;
        destination = targetItem.transform.position;
    }

    public override void OnStart(NPCBase npc)
    {
        if (targetItem != null)
        {
            base.OnStart(npc);
        }
    }

    public override void OnUpdate(NPCBase npc)
    {
        // No specific update logic needed for this task
    }

    public override bool IsFinished(NPCBase npc)
    {
        if (targetItem == null)
        {
            return true; // No item found, so the task is "finished"
        }

        if (isCloseEnough())
        {
            targetItem.Use(npc);
            return true;
        }

        return false;
    }
}
