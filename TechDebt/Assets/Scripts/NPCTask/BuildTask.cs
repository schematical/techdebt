// BuildTask.cs

using System;
using Effects.Infrastructure;
using NPCs;
using Stats;
using UnityEngine;

public class BuildTask : InfrastructureTaskBase
{

    private float buildProgress = 0f;
    private int displayBuildProgress = -1;
    public InfrastructureData.State? OnQueuedSetState = InfrastructureData.State.Planned;

    public BuildTask(InfrastructureInstance target, int priority = 5) : base(target)
    {
        MetaStat = MetaChallenges.MetaStat.Infra_Built;
        Priority = priority;
    }

    public override void OnUpdate(NPCBase npc)
    {
        base.OnUpdate(npc);
        // Only start building after the NPC has arrived.
        if (IsCloseEnough())
        {
          
            NPCDevOps npcDevOps = npc.GetComponent<NPCDevOps>();
        

            float adjustedProgress = Time.deltaTime * npcDevOps.GetBuildSpeed();
            buildProgress += adjustedProgress;
            int checkBuildProgress = (int)Math.Round(buildProgress/TargetInfrastructure.GetWorldObjectType().BuildTime * 100f);
            if (checkBuildProgress % 10 == 0 && displayBuildProgress != checkBuildProgress)
            {
                displayBuildProgress = checkBuildProgress;
                GameManager.Instance.FloatingTextFactory.ShowText($"{displayBuildProgress}%",
                    TargetInfrastructure.transform.position); //  + new Vector3(0, 1, 3));
                npc.AddXP();
            }
        }
    }

    public override bool IsFinished(NPCBase npc)
    {
        return buildProgress >= TargetInfrastructure.GetWorldObjectType().BuildTime;
    }

    public override string GetDescription()
    {
        return $"{base.GetDescription()} Progress: {buildProgress}";
    }
    
    public override void OnEnd(NPCBase npc)
    {
        NPCDevOps npcDevOps = npc.GetComponent<NPCDevOps>();
        /*foreach (ModifierBase modifier in npcDevOps.Modifiers.Modifiers)
        {
            if (modifier.Type == ModifierBase.ModifierType.NPC_InfraStat)
            {
                StatModifier existingStatModifier = TargetInfrastructure.GetWorldObjectType().Stats.Stats[modifier.StatType].Modifiers
                    .Find((statModifier => statModifier.Id == modifier.Id));
                if(existingStatModifier == null)  {
                    TargetInfrastructure.GetWorldObjectType().Stats.AddModifier(modifier.StatType, new StatModifier(modifier.Id, modifier.GetScaledValue()));
                    GameManager.Instance.FloatingTextFactory.ShowText($"Bonus Applied: ${modifier.StatType} x {Math.Round(modifier.GetScaledValue() * 100)}%",
                        TargetInfrastructure.transform.position);
                }
                else
                {
                    // existingStatModifier.Value = npcTrait.GetScaledValue()
                }
            }
        }*/

        base.OnEnd(npc);
        
        CurrentState = State.Completed; // Set status to completed
        
        TargetInfrastructure.SetState(InfrastructureData.State.Operational);
        

        
        GameManager.Instance.NotifyDailyCostChanged();

    }
    public override string GetAssignButtonText()
    {
        return "Build";
    }
    public override void OnQueued()
    {
        if (OnQueuedSetState != null)
        {
            TargetInfrastructure.SetState(OnQueuedSetState.Value);
        }

        base.OnQueued();
    }

   
}