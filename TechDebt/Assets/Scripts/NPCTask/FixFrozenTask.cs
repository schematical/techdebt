
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

}