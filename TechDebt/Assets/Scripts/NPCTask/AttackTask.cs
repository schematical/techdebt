// BuildTask.cs

using System;
using Effects.Infrastructure;
using NPCs;
using Stats;
using UnityEngine;

public class AttackTask : NPCTask
{
    public NPCBase targetNPC { get; }

    private float coolDown = 0f;
    // public EnvEffectBase buildEffect;
    public InfrastructureData.State? OnQueuedSetState = InfrastructureData.State.Planned;

    public AttackTask(NPCBase target, int priority = 7) : base(target.transform.position)
    {
        targetNPC = target;
        Priority = priority;
    }

    public override void OnUpdate(NPCBase npc)
    {
        destination = targetNPC.transform.position;
        coolDown -= Time.deltaTime;
        // Only start building after the NPC has arrived.
        if (isCloseEnough())
        {

            if (coolDown <= 0)
            {
              
                npc.AttackNPC(targetNPC);
                coolDown = 5;

            }


        } else if (!npc.isMoving || coolDown <= 0)
        {
            coolDown = .5f;
            npc.MoveTo(targetNPC.transform.position);
        }
    }

    public override bool IsFinished(NPCBase npc)
    {
        return targetNPC.IsDead();
    }

    public override string GetDescription()
    {
        return $"{base.GetDescription()} - Target: {targetNPC.name}";
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