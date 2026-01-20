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
        if (isCloseEnough())
        {
            npc.AddXP(Time.deltaTime);
            if (TargetInfrastructure.data.CurrentState == InfrastructureData.State.Operational)
            {
                TargetInfrastructure.SetState(InfrastructureData.State.Unlocked);
            }
            _duration -= Time.deltaTime;
        }
    }

    public override bool IsFinished(NPCBase npc)
    {
        return isCloseEnough() && _duration <= 0;
    }

    public override void OnEnd(NPCBase npc)
    {
        base.OnEnd(npc);
        // Apply the resize logic when the task is officially finished
        TargetInfrastructure.ApplyResize(SizeChange);
    }
    
    public override string GetAssignButtonText()
    {
        if (SizeChange == -1)
        {
            return "Downsize";
        }

        if (SizeChange == 1)
        {
            return "Upsize";
        }
        throw new System.Exception($"Invalid Size: {SizeChange}");
    }
}