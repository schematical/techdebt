// BuildTask.cs

using System;
using NPCs;
using Stats;
using UnityEngine;

public class BuildTask : NPCTask
{
    public InfrastructureInstance TargetInfrastructure { get; }
    private float buildProgress = 0f;
    private int displayBuildProgress = -1;

    public BuildTask(InfrastructureInstance target, int priority = 5) : base(target.transform.position)
    {
        Debug.Log("Building infrastructure:"+ target.data.ID);
        TargetInfrastructure = target;
        Priority = priority;
    }

    public override void OnUpdate(NPCBase npc)
    {
        // Only start building after the NPC has arrived.
        if (hasArrived)
        {
            NPCDevOps npcDevOps = npc.GetComponent<NPCDevOps>();
        

            float adjustedProgress = Time.deltaTime * npcDevOps.GetBuildSpeed();
            buildProgress += adjustedProgress;
            int checkBuildProgress = (int)Math.Round(buildProgress/TargetInfrastructure.data.BuildTime * 100f);
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
        return buildProgress >= TargetInfrastructure.data.BuildTime;
    }
    
    public override void OnEnd(NPCBase npc)
    {
        NPCDevOps npcDevOps = npc.GetComponent<NPCDevOps>();
        foreach (NPCTrait npcTrait in npcDevOps.Traits)
        {
            if (npcTrait.Type == NPCTrait.TraitType.InfraStat)
            {
                TargetInfrastructure.data.Stats.AddModifier(npcTrait.StatType, new StatModifier(StatModifier.ModifierType.Multiply, npcTrait.GetScaledValue(), npcTrait));
                GameManager.Instance.FloatingTextFactory.ShowText($"Bonus Applied: ${npcTrait.StatType} x {Math.Round(npcTrait.GetScaledValue() * 100)}%",
                    TargetInfrastructure.transform.position); 
            }
        }

        base.OnEnd(npc);
        
        CurrentStatus = Status.Completed; // Set status to completed
        
        TargetInfrastructure.SetState(InfrastructureData.State.Operational);
        GameManager.Instance.NotifyDailyCostChanged();

        var serverComponent = TargetInfrastructure.GetComponent<Server>();
        if(serverComponent != null && !GameManager.Instance.ActiveInfrastructure.Contains(serverComponent))
        {
            GameManager.Instance.ActiveInfrastructure.Add(serverComponent);
        }
    }
}