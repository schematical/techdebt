// ResizeTask.cs

using Effects.Infrastructure;
using Tutorial;
using UnityEngine;

public class ResizeTask : InfrastructureTaskBase
{
    public const float RESIZE_DURATION = 5f;
    public int SizeChange { get; private set; }

    public ResizeTask(InfrastructureInstance target, int sizeChange)
        : base(target)
    {
      
        SizeChange = sizeChange;
        Priority = 5; // Mid-level priority
        TutorialStepId = Tutorial.TutorialStepId.Task_Resize_Queued;
    }

    protected override float GetProgressRequirement()
    {
        return RESIZE_DURATION;
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