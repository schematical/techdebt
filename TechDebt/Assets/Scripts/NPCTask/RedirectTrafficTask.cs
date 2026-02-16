
using UnityEngine;

public class RedirectTrafficTask: NPCTask
{
    private float coolDown = 0f;
    public RedirectTrafficTask(iTargetable target, int priority = 7) : base(target)
    {
       
        this.target = target;
        interactionType = InteractionType.Block;
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
                    float dist = Vector3.Distance(npc.transform.position, packet.transform.position);
                    return (
                        dist < maxTaskRange
                        
                    );
                }));
                Debug.Log($"Checking for NetworkPackets: {networkPacket}");
                if (networkPacket != null)
                {
                    networkPacket.MarkFailed();  
                }
                coolDown = 1;
            }
          
        

          

        } else if (!npc.isMoving)
        {
          
            npc.MoveTo(target.GetInteractionPosition(interactionType));
        }
    }

    public override bool IsFinished(NPCBase npc)
    {
        return false;
    }
}
