
using UnityEngine;

public class RedirectTrafficTask: NPCTask
{
    private float coolDown = 0f;
    public int packetsRedirected = 0;
    protected GameObject sparks;
    public RedirectTrafficTask(iTargetable target, int priority = 7) : base(target)
    {
       
        this.target = target;
        interactionType = InteractionType.Block;
        Priority = priority;
        maxTaskRange = 1.5f;
        if (sparks != null)
        {
            sparks.gameObject.SetActive(false);
            sparks = null;
        }
    }
    public override void OnUpdate(NPCBase npc)
    {
        coolDown -= Time.deltaTime;
    
        if (IsCloseEnough())
        {
            npc.animator.SetBool("isAttacking", true);
            if (coolDown < 0)
            {
                coolDown = 1;
                // Check to see if there are network packets near
                NetworkPacket networkPacket = GameManager.Instance.activePackets.Find((packet =>
                {
                    if (packet.IsReturning() || packet.CurrentState != NetworkPacket.State.Running)
                    {
                        return false;
                    }
                    float dist = Vector3.Distance(npc.transform.position, packet.transform.position);
                    return (
                        dist < maxTaskRange
                    );
                }));
                if (networkPacket != null)
                {
                    if (networkPacket.data.Type == NetworkPacketData.PType.PII)
                    {
                        networkPacket.MarkStolen();
                    }
                    else
                    {
                        networkPacket.MarkFailed();
                    }
                    sparks = GameManager.Instance.prefabManager.Create("NetworkPacketSparks", Vector3.zero, networkPacket.transform);
                    sparks.transform.localPosition = new Vector3(0, 0, -0.1f);
                    sparks.GetComponent<SpriteRenderer>().color = Color.purple;
                    GameManager.Instance.UIManager.TriggerScreenShake(1, .5f);
                    InternetPipe internetPipe = GameManager.Instance.GetRandomInfrastructureInstanceByClass<InternetPipe>();
                    if (internetPipe == null)
                    {
                        throw new System.Exception("No Internet Pipe found");
                    }

                    networkPacket.nextHop = internetPipe;
                    packetsRedirected += 1;
                    npc.ResetCooldown(NPCBase.CoolDownType.Attack, 5f);
                }
                
            }

        } else if (!npc.isMoving)
        {
          
            npc.MoveTo(target.GetInteractionPosition(interactionType));
        }
    }

    public override bool IsFinished(NPCBase npc)
    {
        return packetsRedirected >= 3;
    }

    public override void OnEnd(NPCBase npc)
    {
        npc.animator.SetBool("isAttacking", false);
    }
}
