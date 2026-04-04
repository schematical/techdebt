// BuildTask.cs

using System;
using Effects.Infrastructure;
using NPCs;
using Stats;
using UnityEngine;

public class AttackTask : NPCTask
{
    public iAttackable target { get; }

    private float coolDown = 0f;
    // public EnvEffectBase buildEffect;
  

    public AttackTask(iAttackable target, int priority = 7) : base(target)
    {
       
        this.target = target;
        Priority = priority;
        maxTaskRange = 1f;
        interactionType = InteractionType.Attack;
    }

    public override void OnUpdate(NPCBase npc)
    {
        // Debug.Log($"AttackTask - Dest: {target.GetInteractionPosition(interactionType)} - {Vector3.Distance(target.GetInteractionPosition(interactionType), AssignedNPC.transform.position)} <= {maxTaskRange}");
        coolDown -= Time.deltaTime;
        // Only start building after the NPC has arrived.
        if (IsCloseEnough())
        {

            if (coolDown <= 0)
            {
                npc.StopMovement();
                npc.Attack(target);
                coolDown = 1;

            }

        } else if (!npc.isMoving || coolDown <= 0)
        {
            coolDown = .5f;
            npc.MoveTo(target.GetInteractionPosition(interactionType));
        }
    }

    public override bool IsFinished(NPCBase npc)
    {
        return target.IsDead();
    }

   
    
    
    public override string GetAssignButtonText()
    {
        return "Debug";
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