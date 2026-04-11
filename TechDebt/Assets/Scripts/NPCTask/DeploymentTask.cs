// DeploymentTask.cs

using Effects.Infrastructure;
using Tutorial;
using UnityEngine;

public class DeploymentTask : InfrastructureTaskBase
{


    private const float DeploymentTime = 30f; // Time in seconds to complete deployment
    private ReleaseBase _release;
    

    public DeploymentTask(ApplicationServer target, ReleaseBase release) : base(target)
    {
        Priority = 4;
        _release = release;
        maxTaskRange = .1f;
        globalSpeedStatType = StatType.Global_DeploymentSpeed;
    }

    public virtual void OnStart(NPCBase npc)
    {
        base.OnStart(npc);
        _release.SetState(ReleaseBase.ReleaseState.DeploymentInProgress);
    }
    

    public override void OnEnd(NPCBase npc)
    {
        base.OnEnd(npc);
        TargetInfrastructure.Version =  _release.GetVersionString();
        TargetInfrastructure.HideAttentionIcon();
        CurrentState = State.Completed;
        _release.CheckIsOver();
    }

    protected override float GetProgressRequirement()
    {
        return DeploymentTime;
    }

    public override string GetAssignButtonText()
    {
        return $"Deploy {_release.GetVersionString()}";
    }

}
