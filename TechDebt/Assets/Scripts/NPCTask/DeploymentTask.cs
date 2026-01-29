// DeploymentTask.cs

using Events;
using UnityEngine;

public class DeploymentTask : NPCTask
{
    public ApplicationServer TargetInfrastructure { get; }
    private float deploymentProgress = 0f;
    private const float DeploymentTime = 5f; // Time in seconds to complete deployment
    private ReleaseBase _release;

    public DeploymentTask(ApplicationServer target, ReleaseBase release) : base(target)
    {
        TargetInfrastructure = target;
        Priority = 4;
        _release = release;
        maxTaskRange = .1f;
    }

    public virtual void OnStart(NPCBase npc)
    {
    base.OnStart(npc);
        _release.SetState(ReleaseBase.ReleaseState.DeploymentInProgress);
    }
    public override void OnUpdate(NPCBase npc)
    {
        if (IsCloseEnough())
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
        TargetInfrastructure.Version =  _release.GetVersionString();
        TargetInfrastructure.HideAttentionIcon();
        CurrentState = State.Completed;
        _release.CheckIsOver();
    }
    public override string GetAssignButtonText()
    {
        return $"Deploy {_release.GetVersionString()}";
    }
}
