// BuildTask.cs
using UnityEngine;

public class BuildTask : NPCTask
{
    public InfrastructureInstance TargetInfrastructure { get; }
    private float buildProgress = 0f;
    private bool _hasArrived = false;

    public BuildTask(InfrastructureInstance target, int priority = 10)
    {
        Debug.Log("Building infrastructure:"+ target.data.ID);
        TargetInfrastructure = target;
        Priority = priority;
    }

    public override void OnStart(NPCDevOps npc)
    {
        // Subscribe to the arrival event
        npc.OnDestinationReached += HandleArrival;
        Debug.Log("OnStart:" +TargetInfrastructure.data.ID + "    -Pos: " + TargetInfrastructure.transform.position);
        npc.MoveTo(TargetInfrastructure.transform.position);
    }

    private void HandleArrival()
    {
        _hasArrived = true;
    }

    public override void OnUpdate(NPCDevOps npc)
    {
        // Only start building after the NPC has arrived.
        if (_hasArrived)
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
        // Unsubscribe to prevent memory leaks
        npc.OnDestinationReached -= HandleArrival;
        
        CurrentStatus = Status.Completed; // Set status to completed
        
        TargetInfrastructure.SetState(InfrastructureData.State.Operational);
        GameManager.Instance.NotifyInfrastructureBuilt(TargetInfrastructure);
        GameManager.Instance.NotifyDailyCostChanged();

        var serverComponent = TargetInfrastructure.GetComponent<Server>();
        if(serverComponent != null && !GameManager.AllServers.Contains(serverComponent))
        {
            GameManager.AllServers.Add(serverComponent);
        }
    }
}