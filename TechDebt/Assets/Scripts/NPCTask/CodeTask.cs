// ResearchTask.cs
using UnityEngine;
using System.Linq;

public class CodeTask : InfrastructureTaskBase, iProgressable
{
    public ReleaseBase ReleaseBase { get; private set; }
    private readonly Desk desk;

    public CodeTask(ReleaseBase release) : base(GameManager.Instance.ActiveInfrastructure.FirstOrDefault(infra => infra.data.Id == "desk"))
    {
        ReleaseBase = release;
        Priority = 3; // Research is a low-priority, background task.
        maxTaskRange = .1f;
        npcWorkSpeedStatType = StatType.NPC_CodeSpeed;
        globalSpeedStatType = StatType.Global_DeploymentSpeed;
        npcWorkQualityStatType =  StatType.NPC_CodeQuality;
        // Find the desk to navigate to.
        InfrastructureInstance deskInstance = GameManager.Instance.ActiveInfrastructure.FirstOrDefault(infra => infra.data.Id == "desk");
        if (deskInstance != null)
        {
            desk = deskInstance.GetComponent<Desk>();
        }
        else
        {
            Debug.LogError("ResearchTask could not be created. No 'desk' infrastructure found or its instance is null.");
            desk = null;
        }
    }
    public override void OnStart(NPCBase npc)
    {
        npc.AddStatusBar(this);
        base.OnStart(npc);
    }
    public override void OnUpdate(NPCBase npc)
    {
        if (desk == null) return;
        
        // Apply research points only if the NPC is at the desk
        if (IsCloseEnough())
        {
            NPCDevOps devOpsNpc = npc as NPCDevOps;
     
            float progressGained = GetNpcWorkSpeed(devOpsNpc) * Time.fixedDeltaTime;
            ReleaseBase.ApplyProgress(progressGained, AssignedNPC);
            devOpsNpc.AddXP(Time.fixedDeltaTime);
            devOpsNpc.FaceTarget(target.GetInteractionPosition());
        }
    }
    
    public override void OnEnd(NPCBase npc)
    {
        base.OnEnd(npc);
        npc.HideProgressBar();
        // No specific end action is needed beyond base functionality.
    }

    public override bool IsFinished(NPCBase npc)
    {
        switch (ReleaseBase.State)
        {
            case(ReleaseBase.ReleaseState.DeploymentReady):
            case(ReleaseBase.ReleaseState.DeploymentInProgress):
            case(ReleaseBase.ReleaseState.DeploymentRewardReady):
            case(ReleaseBase.ReleaseState.DeploymentCompleted):
                return true;
        }

        return false;
    }

    protected override float GetProgressRequirement()
    {
        throw new System.NotImplementedException();
    }

    public override string GetAssignButtonText()
    {
        return "Research????";
    }

    public float GetProgress()
    {
       return ReleaseBase.GetProgress();
    }
}
