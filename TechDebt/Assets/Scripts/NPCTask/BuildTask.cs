// BuildTask.cs

using System;
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

    public override void OnUpdate(NPCDevOps npc)
    {
        // Only start building after the NPC has arrived.
        if (hasArrived)
        {
            buildProgress += Time.deltaTime;
        }
    }

    public override bool IsFinished(NPCDevOps npc)
    {
        return buildProgress >= TargetInfrastructure.data.BuildTime;
    }
    
    public override void OnEnd(NPCDevOps npc)
    {
        base.OnEnd(npc);
        
        CurrentStatus = Status.Completed; // Set status to completed
        
        TargetInfrastructure.SetState(InfrastructureData.State.Operational);
        GameManager.Instance.NotifyInfrastructureBuilt(TargetInfrastructure);
        GameManager.Instance.NotifyDailyCostChanged();

        var serverComponent = TargetInfrastructure.GetComponent<Server>();
        if(serverComponent != null && !GameManager.Instance.ActiveInfrastructure.Contains(serverComponent))
        {
            GameManager.Instance.ActiveInfrastructure.Add(serverComponent);
        }
    }
}