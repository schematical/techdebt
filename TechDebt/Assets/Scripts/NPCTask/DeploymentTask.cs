// DeploymentTask.cs

using Events;
using UnityEngine;

public class DeploymentTask : NPCTask
{
    public Server TargetInfrastructure { get; }
    private float deploymentProgress = 0f;
    private const float DeploymentTime = 5f; // Time in seconds to complete deployment
    private DeploymentEvent Event;

    public DeploymentTask(Server target, DeploymentEvent _event) : base(target.transform.position)
    {
        TargetInfrastructure = target;
        Priority = 4;
        Event = _event;
    }

    public override void OnUpdate(NPCDevOps npc)
    {
        if (hasArrived)
        {
            deploymentProgress += Time.deltaTime;
        }
    }

    public override bool IsFinished(NPCDevOps npc)
    {
        return deploymentProgress >= DeploymentTime;
    }

    public override void OnEnd(NPCDevOps npc)
    {
        base.OnEnd(npc);
        TargetInfrastructure.Version =  Event.GetVersionString();
        CurrentStatus = Status.Completed;
        Event.CheckIsOver();
    }
}
