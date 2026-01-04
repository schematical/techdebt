// ResizeTask.cs
using UnityEngine;

public class ResizeTask : NPCTask
{
    public InfrastructureInstance TargetInfrastructure { get; private set; }
    public int SizeChange { get; private set; }
    private float _duration;

    public ResizeTask(InfrastructureInstance target, int sizeChange)
        : base(target.transform.position)
    {
        TargetInfrastructure = target;
        SizeChange = sizeChange;
        _duration = 5f; // 5-second duration for resizing
        Priority = 5; // Mid-level priority
    }

    public override void OnUpdate(NPCBase npc)
    {
        if (hasArrived)
        {
            _duration -= Time.deltaTime;
        }
    }

    public override bool IsFinished(NPCBase npc)
    {
        return hasArrived && _duration <= 0;
    }

    public override void OnEnd(NPCBase npc)
    {
        base.OnEnd(npc);
        // Apply the resize logic when the task is officially finished
        TargetInfrastructure.ApplyResize(SizeChange);
    }
}