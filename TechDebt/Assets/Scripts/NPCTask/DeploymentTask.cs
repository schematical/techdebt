// DeploymentTask.cs

using Events;
using UnityEngine;

public class DeploymentTask : NPCTask
{
    public Server TargetInfrastructure { get; }
    private float deploymentProgress = 0f;
    private const float DeploymentTime = 5f; // Time in seconds to complete deployment
    private DeploymentBase Deployment;

    public DeploymentTask(Server target, DeploymentBase deployment) : base(target.transform.position)
    {
        TargetInfrastructure = target;
        Priority = 4;
        Deployment = deployment;
    }

    public override void OnUpdate(NPCBase npc)
    {
        if (hasArrived)
        {
            deploymentProgress += Time.deltaTime;
            npc.AddXP(Time.deltaTime);
        }
    }
 
    public override bool IsFinished(NPCBase npc)
    {
        return deploymentProgress >= DeploymentTime;
    }

    public override void OnEnd(NPCBase npc)
    {
        base.OnEnd(npc);
        TargetInfrastructure.Version =  Deployment.GetVersionString();
        CurrentStatus = Status.Completed;
        Deployment.CheckIsOver();
    }
}
