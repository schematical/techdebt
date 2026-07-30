// ResearchTask.cs
using UnityEngine;
using System.Linq;
using Infrastructure;

public class CodeTask : InfrastructureTaskBase, iProgressable
{
    public ReleaseBase ReleaseBase { get; private set; }
    private readonly Desk desk;

    public CodeTask(ReleaseBase release) : base(GameManager.Instance.ActiveInfrastructure.FirstOrDefault(infra => infra.Id == "desk"))
    {
        ReleaseBase = release;
        Priority = 3; // Research is a low-priority, background task.
        maxTaskRange = .1f;
        npcWorkSpeedStatType = StatType.NPC_CodeSpeed;
        globalSpeedStatType = StatType.Global_DeploymentSpeed;
        npcWorkQualityStatType =  StatType.NPC_CodeQuality;
        // Find the desk to navigate to.
        WorldObjectBase deskInstance = GameManager.Instance.ActiveInfrastructure.FirstOrDefault(infra => infra.Id == "desk");
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
        npc.AddProgressBar(this);
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
    public override string GetProgressText()
    {
        return "Coding...";
    }
}
