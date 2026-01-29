
using Items;
using UnityEngine;

public class BugConsumeTask : NPCTask
{
    private iTargetable target;

    public BugConsumeTask(iTargetable target)
    {
        // Find the nearest active ItemBase without a task
       
        if (target == null)
        {
            throw new System.Exception("targetItem is null");
        }

        this.target = target;
        destination = this.target.transform.position;
    }

    public override void OnStart(NPCBase npc)
    {
        if (target != null)
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
        if (target == null)
        {
            return true; // No item found, so the task is "finished"
        }

        if (isCloseEnough())
        {
 
            return true;
        }

        return false;
    }

    public override void OnEnd(NPCBase npc)
    {
        NetworkPacket networkPacket = target.gameObject.GetComponent<NetworkPacket>();
        if (networkPacket != null)
        {
            networkPacket.MarkFailed();
        }
        base.OnEnd(npc);
    }
}
