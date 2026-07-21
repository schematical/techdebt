// BuildTask.cs

using System;
using Effects.Infrastructure;
using NPCs;
using Stats;
using UnityEngine;

public class ShutdownTask : InfrastructureTaskBase
{

    public ShutdownTask(InfrastructureInstance target, int priority = 5) : base(target)
    {
        MetaStat = MetaChallenges.MetaStat.Infra_Shutdown;
        Priority = priority;
        // OnQueuedSetState = InfrastructureData.State.Planned;
        npcWorkSpeedStatType = StatType.NPC_DevOpsSpeed;
        EndState = InfrastructureData.State.Unlocked;
    }

    public override string GetAssignButtonText()
    {
        return "Shutdown";
    }
    public override bool IsFinished(NPCBase npc)
    {
        return base.IsFinished(npc) || !TargetInfrastructure.IsActive();
    }

    protected override float GetProgressRequirement()
    {
        return TargetInfrastructure.GetWorldObjectType().BuildTime;
    }
    public override string GetProgressText()
    {
        return "Shutting down...";
    }
}