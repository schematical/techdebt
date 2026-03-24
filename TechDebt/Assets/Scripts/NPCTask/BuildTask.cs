// BuildTask.cs

using System;
using Effects.Infrastructure;
using NPCs;
using Stats;
using UnityEngine;

public class BuildTask : InfrastructureTaskBase
{

    public BuildTask(InfrastructureInstance target, int priority = 5) : base(target)
    {
        MetaStat = MetaChallenges.MetaStat.Infra_Built;
        Priority = priority;
        OnQueuedSetState = InfrastructureData.State.Planned;
        npcWorkSpeedStatType = StatType.NPC_DevOpsSpeed;
    }

    public override string GetAssignButtonText()
    {
        return "Build";
    }
    public override bool IsFinished(NPCBase npc)
    {
        return base.IsFinished(npc) || TargetInfrastructure.IsActive();
    }

    protected override float GetProgressRequirement()
    {
        return TargetInfrastructure.GetWorldObjectType().BuildTime;
    }
}