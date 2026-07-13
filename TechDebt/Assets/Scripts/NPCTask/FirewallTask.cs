// DeploymentTask.cs

using Effects.Infrastructure;
using Tutorial;
using UnityEngine;

public class FirewallTask : InfrastructureTaskBase
{


    private const float TaskDuration = 5f; // Time in seconds to complete deployment
    private InternetPipe.InternetPipeFirewallState GoalFirewallState;
    

    public FirewallTask(InternetPipe target, InternetPipe.InternetPipeFirewallState _goalFirewallState) : base(target)
    {
        Priority = 4;
        GoalFirewallState = _goalFirewallState;
        maxTaskRange = .1f;
        globalSpeedStatType = StatType.Global_DeploymentSpeed;
    }

    /*public virtual void OnStart(NPCBase npc)
    {
        base.OnStart(npc);
        _release.SetState(ReleaseBase.ReleaseState.DeploymentInProgress);
    }*/
    

    public override void OnEnd(NPCBase npc)
    {
        base.OnEnd(npc);
        (TargetInfrastructure as InternetPipe).SetFirewallState(GoalFirewallState);
        TargetInfrastructure.HideAttentionIcon();
        CurrentState = State.Completed;
    }

    protected override float GetProgressRequirement()
    {
        return TaskDuration;
    }

    public override string GetAssignButtonText()
    {
        return $"{GoalFirewallState} Traffic";
    }

}
