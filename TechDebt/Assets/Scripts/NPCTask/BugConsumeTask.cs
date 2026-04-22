
using Items;
using UnityEngine;

public class BugConsumeTask : NPCTask
{


    public BugConsumeTask(iTargetable target, int prioity = 1): base(target, prioity)
    {
        maxTaskRange = .5f;
        toastComplete = false;
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
      
        if (IsCloseEnough())
        {
 
            return true;
        }

        return false;
    }

    public override void OnEnd(NPCBase npc)
    {
        if (target == null || !target.gameObject.activeInHierarchy)
        {
            // Do not reset cool down as the packet just completed its journey.
            base.OnEnd(npc);
            return;
        }
        NetworkPacket networkPacket = target.gameObject.GetComponent<NetworkPacket>();
        if (networkPacket != null && networkPacket.isActiveAndEnabled)
        {
            networkPacket.MarkFailedAndDestroy();
        }
        else
        {
            // This is for the item consume stuff.
            target.gameObject.SetActive(false);
        }

        npc.ResetCooldown(NPCBase.CoolDownType.Consume, 5);
        base.OnEnd(npc);
    }
}
