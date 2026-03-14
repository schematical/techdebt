
public class FixFrozenTask : BuildTask
{
    public FixFrozenTask(InfrastructureInstance target, int priority = 8) : base(target, priority)
    {
        MetaStat = MetaChallenges.MetaStat.Infra_Fix;
        OnQueuedSetState = null;
    }

    public override string GetAssignButtonText()
    {
        return "Fix";
    }
    
    public override bool IsFinished(NPCBase npc)
    {
        return buildProgress >= TargetInfrastructure.GetWorldObjectType().BuildTime || TargetInfrastructure.data.CurrentState == InfrastructureData.State.Operational;
    }

}