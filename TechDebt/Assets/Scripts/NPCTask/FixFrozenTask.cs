
using Tutorial;

public class FixFrozenTask : InfrastructureTaskBase
{
    public FixFrozenTask(InfrastructureInstance target, int priority = 8) : base(target)
    {
        MetaStat = MetaChallenges.MetaStat.Infra_Fix;
        OnQueuedSetState = null;
        Priority = priority;
        TutorialStepId = Tutorial.TutorialStepId.Task_FixFrozen_Queued;
        npcWorkSpeedStatType = StatType.NPC_FixSpeed;
    }

    public override string GetAssignButtonText()
    {
        return "Fix";
    }

    protected override float GetProgressRequirement()
    {
        return 5;
    }

  

}