
using Items;
using UnityEngine;

public class BugConsumeTask : NPCTask
{


    public BugConsumeTask(iTargetable target, int prioity = 1): base(target, prioity)
    {

    }
/*
    public override void OnStart(NPCBase npc)
    {

            base.OnStart(npc);

    }
*/
    public override void OnUpdate(NPCBase npc)
    {

    }

    public override bool IsFinished(NPCBase npc)
    {
        if (target == null)
        {
            return true; // No item found, so the task is "finished"
        }
        if (!target.gameObject.activeInHierarchy)
        {
            return true; // No item found, so the task is "finished"
        }
        if (IsCloseEnough())
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
