
using UnityEngine;

public class RedirectTrafficTask: NPCTask
{
    private float coolDown = 0f;
    public RedirectTrafficTask(iAttackable target, int priority = 7) : base(target)
    {
       
        this.target = target;
        Priority = priority;
        maxTaskRange = 1.5f;
    }
    public override void OnUpdate(NPCBase npc)
    {
        coolDown -= Time.deltaTime;
    
        if (IsCloseEnough())
        {
            if (coolDown < 0)
            {
                // Check to see if there are network packets near
                NetworkPacket networkPacket = GameManager.Instance.activePackets.Find((packet =>
                {
                    float dist = Vector3.Distance(target.transform.position, packet.transform.position);
                    return (
                        dist < maxTaskRange
                        
                    );
                }));
                if (networkPacket != null)
                {
                    networkPacket.MarkFailed();  
                }
                coolDown = 1;
            }
          
        

          

        } else if (!npc.isMoving)
        {
          
            npc.MoveTo(target.transform.position);
        }
    }

    public override bool IsFinished(NPCBase npc)
    {
        return false;
    }
}
