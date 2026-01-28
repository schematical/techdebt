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
    public InfrastructureData.State? OnQueuedSetState = InfrastructureData.State.Planned;

    public AttackTask(iAttackable target, int priority = 7) : base(target.transform.position)
    {
       
        this.target = target;
        Priority = priority;
    }

    public override void OnUpdate(NPCBase npc)
    {
        destination = target.transform.position;
        coolDown -= Time.deltaTime;
        // Only start building after the NPC has arrived.
        if (isCloseEnough())
        {

            if (coolDown <= 0)
            {
              
                npc.Attack(target);
                coolDown = 1;

            }


        } else if (!npc.isMoving || coolDown <= 0)
        {
            coolDown = .5f;
            npc.MoveTo(target.transform.position);
        }
    }

    public override bool IsFinished(NPCBase npc)
    {
        return target.IsDead();
    }

    public override string GetDescription()
    {
        return $"{base.GetDescription()} - Target: {target.name}";
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