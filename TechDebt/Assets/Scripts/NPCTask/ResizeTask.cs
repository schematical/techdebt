// ResizeTask.cs

using Effects.Infrastructure;
using Tutorial;
using UnityEngine;

public class ResizeTask : InfrastructureTaskBase
{
    public const float RESIZE_DURATION = 5f;
    public int SizeChange { get; private set; }
    private float _duration;
    public EnvEffectBase buildEffect { get; set; }

    public ResizeTask(InfrastructureInstance target, int sizeChange)
        : base(target)
    {
      
        SizeChange = sizeChange;
        _duration = RESIZE_DURATION; // 5-second duration for resizing
        Priority = 5; // Mid-level priority
    }

    public override void OnUpdate(NPCBase npc)
    {
        base.OnUpdate(npc);
        if (IsCloseEnough())
        {
            
            npc.AddXP(Time.deltaTime);
            if (TargetInfrastructure.data.CurrentState == InfrastructureData.State.Operational)
            {
                TargetInfrastructure.SetState(InfrastructureData.State.Planned);
            }
            _duration -= Time.deltaTime;
        }
    }



    public override bool IsFinished(NPCBase npc)
    {
        return IsCloseEnough() && _duration <= 0;
    }

    public override void OnEnd(NPCBase npc)
    {
        base.OnEnd(npc);
        // Apply the resize logic when the task is officially finished
        TargetInfrastructure.ApplyResize(SizeChange);
    }

    public override float GetProgress()
    {
        return 1 - (_duration / RESIZE_DURATION);
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
    public override void OnQueued()
    {
        if (GameManager.Instance.TutorialManager != null)
        {
            GameManager.Instance.TutorialManager.Trigger(TutorialStepId.Task_Resize_Queued);
        }

        base.OnQueued();
    }
  
}