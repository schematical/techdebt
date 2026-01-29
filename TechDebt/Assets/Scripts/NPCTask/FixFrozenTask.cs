// BuildTask.cs

using System;
using Effects.Infrastructure;
using NPCs;
using Stats;
using UnityEngine;

public class FixFrozenTask : BuildTask
{
    public FixFrozenTask(InfrastructureInstance target, int priority = 8) : base(target, priority)
    {
        OnQueuedSetState = null;
    }

    public override string GetAssignButtonText()
    {
        return "Fix";
    }
    
}