// BuildTask.cs

using System;
using Effects.Infrastructure;
using NPCs;
using Stats;
using UnityEngine;

public class BuildTask : NPCTask
{
    public InfrastructureInstance TargetInfrastructure { get; }
    private float buildProgress = 0f;
    private int displayBuildProgress = -1;
    public EnvEffectBase buildEffect;

    public BuildTask(InfrastructureInstance target, int priority = 5) : base(target.transform.position)
    {
        TargetInfrastructure = target;
        Priority = priority;
    }

    public override void OnUpdate(NPCBase npc)
    {
        // Only start building after the NPC has arrived.
        if (hasArrived)
        {
            if (buildEffect == null)
            {
                var be = GameManager.Instance.prefabManager.Create("BuildInfraEffect", TargetInfrastructure.transform.position);
                // be.transform.localPosition = Vector3.zero;
     
                be.transform.SetParent(TargetInfrastructure.transform);
                be.transform.localPosition = new Vector3(0, 0, -1f);
                buildEffect = be.GetComponent<EnvEffectBase>();
            }
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

    public override string GetDescription()
    {
        return $"{base.GetDescription()} Progress: {buildProgress}";
    }
    
    public override void OnEnd(NPCBase npc)
    {
        NPCDevOps npcDevOps = npc.GetComponent<NPCDevOps>();
        foreach (NPCTrait npcTrait in npcDevOps.Traits)
        {
            if (npcTrait.Type == NPCTrait.TraitType.InfraStat)
            {
                StatModifier existingStatModifier = TargetInfrastructure.data.Stats.Stats[npcTrait.StatType].Modifiers
                    .Find((statModifier => statModifier.Source == npcTrait));
                if(existingStatModifier == null)  {
                    TargetInfrastructure.data.Stats.AddModifier(npcTrait.StatType, new StatModifier(StatModifier.ModifierType.Multiply, npcTrait.GetScaledValue(), npcTrait));
                    GameManager.Instance.FloatingTextFactory.ShowText($"Bonus Applied: ${npcTrait.StatType} x {Math.Round(npcTrait.GetScaledValue() * 100)}%",
                        TargetInfrastructure.transform.position);
                }
                else
                {
                    // existingStatModifier.Value = npcTrait.GetScaledValue()
                }
            }
        }

        base.OnEnd(npc);
        
        CurrentState = State.Completed; // Set status to completed
        
        TargetInfrastructure.SetState(InfrastructureData.State.Operational);
        
        buildEffect.gameObject.SetActive(false);
        
        GameManager.Instance.NotifyDailyCostChanged();

    }
    public override string GetAssignButtonText()
    {
        return "Build";
    }
    public override void OnQueued()
    {
        TargetInfrastructure.SetState(InfrastructureData.State.Planned);
        base.OnQueued();
    }

    public override void OnInterrupt()
    {
        base.OnInterrupt();
        if (buildEffect == null)
        {
            buildEffect.gameObject.SetActive(false);
        }
    }
}