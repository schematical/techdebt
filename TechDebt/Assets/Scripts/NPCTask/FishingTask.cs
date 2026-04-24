// BuildTask.cs

using System;
using Effects.Infrastructure;
using NPCs;
using Stats;
using UnityEngine;

public class FishingTask : NPCTask
{
    public iTargetable target { get; set; }
    private float coolDown = 0f;

    private bool isRetreating = false;
    // public EnvEffectBase buildEffect;
  

    public FishingTask(iAttackable target, int priority = 7) : base(target)
    {
       
        this.target = target;
        Priority = priority;
        maxTaskRange = 1f;
        interactionType = InteractionType.Attack;
    }

    public override void OnStart(NPCBase npc)
    {
        base.OnStart(npc);
        if (npc is NPCAnimatedBiped)
        {
            (npc as NPCAnimatedBiped).SetExpression(NPCAnimatedBiped.FacialExpression.AngryFrown);
        }
    }

    public override void OnUpdate(NPCBase npc)
    {
        
        // Debug.Log($"AttackTask - Dest: {target.GetInteractionPosition(interactionType)} - {Vector3.Distance(target.GetInteractionPosition(interactionType), AssignedNPC.transform.position)} <= {maxTaskRange}");
        coolDown -= Time.fixedDeltaTime;
        // Only start building after the NPC has arrived.
        if (isRetreating)
        {
            return;
        }
        else
        {
            if (IsCloseEnough())
            {

                if (coolDown <= 0)
                {
                    // npc.StopMovement();
                    isRetreating = true;
                    InternetPipe internetPipe = GameManager.Instance.GetRandomInfrastructureInstanceByClass<InternetPipe>();
                    target = internetPipe;
                    npc.MoveTo(internetPipe.GetInteractionPosition(InteractionType.PacketEnter));
                    coolDown = 1;
                    (npc as NPCFishingAttack).MarkReturning();

                }

            }
            else if (!npc.isMoving || coolDown <= 0)
            {
                coolDown = .5f;
                npc.MoveTo(target.GetInteractionPosition(interactionType));
            }
        }
    }

    public override bool IsFinished(NPCBase npc)
    {
        return coolDown <= 0 && isRetreating && IsCloseEnough();
    }

    public override void OnEnd(NPCBase npc)
    {
        base.OnEnd(npc);
        npc.gameObject.SetActive(false);
        GameManager.Instance.UIManager.ShowPacketFail(GameManager.Instance.SpriteManager.GetSprite("FishingAttack", "1"));
    }

   
    
    
    public override string GetAssignButtonText()
    {
        return "Stop Fishing Attack";
    }



    /*public override void OnQueued()
    {
        if (OnQueuedSetState != null)
        {
            TargetInfrastructure.SetState(OnQueuedSetState.Value);
        }

        base.OnQueued();
    }

    public override void OnInterrupt()
    {
        base.OnInterrupt();
        if (buildEffect != null)
        {
            buildEffect.gameObject.SetActive(false);
        }
    }*/
}