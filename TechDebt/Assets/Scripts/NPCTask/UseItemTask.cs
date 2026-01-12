using Items;
using UnityEngine;

public class UseItemTask : NPCTask
{
    public ItemBase TargetItem { get; }

    public UseItemTask(ItemBase targetItem) : base(targetItem.transform.position)
    {
        TargetItem = targetItem;
        Priority = 10; // High priority
    }

    public override void OnUpdate(NPCBase npc)
    {
        // The base NPCTask's Update handles movement. 
        // We only need to act when the NPC has arrived.
        if (hasArrived && !IsFinished(npc))
        {
            if (TargetItem != null)
            {
                TargetItem.Use(npc);
            }
            else
            {
                Debug.LogWarning("TargetItem was null or destroyed before UseItemTask could complete.");
            }
 
        }
    }

    public override bool IsFinished(NPCBase npc)
    {
        return !TargetItem.gameObject.activeInHierarchy;
    }

    public override void OnEnd(NPCBase npc)
    {
        base.OnEnd(npc);
        CurrentStatus = Status.Completed;
    }
    public override string GetAssignButtonText()
    {
        return "Use Item?????";
    }
}