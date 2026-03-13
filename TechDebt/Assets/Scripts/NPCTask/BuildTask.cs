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
        return buildProgress >= TargetInfrastructure.GetWorldObjectType().BuildTime || TargetInfrastructure.IsActive();
    }

    public override string GetDescription()
    {
        return $"{base.GetDescription()} Progress: {buildProgress}";
    }
    
    public override void OnEnd(NPCBase npc)
    {
        npc.HideProgressBar();
   

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


    public override float GetProgress()
    {
        return buildProgress / TargetInfrastructure.GetWorldObjectType().BuildTime;
    }
}