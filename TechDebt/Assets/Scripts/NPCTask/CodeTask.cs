// ResearchTask.cs
using UnityEngine;
using System.Linq;

public class CodeTask : NPCTask
{
    public ReleaseBase ReleaseBase { get; private set; }
    private readonly Desk desk;

    public CodeTask(ReleaseBase release) : base(GameManager.Instance.ActiveInfrastructure.FirstOrDefault(infra => infra.data.ID == "desk")?.transform.position)
    {
        ReleaseBase = release;
        Priority = 3; // Research is a low-priority, background task.

        // Find the desk to navigate to.
        InfrastructureInstance deskInstance = GameManager.Instance.ActiveInfrastructure.FirstOrDefault(infra => infra.data.ID == "desk");
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

    public override void OnUpdate(NPCBase npc)
    {
        if (desk == null) return;
        
        // Apply research points only if the NPC is at the desk
        if (hasArrived)
        {
            var devOpsNpc = npc as NPCDevOps;
            if (devOpsNpc != null)
            {
                float progressGained = /*devOpsNpc.GetResearchPointsPerSecond(TargetTechnology) **/ Time.deltaTime;
                ReleaseBase.ApplyProgress(progressGained);
                devOpsNpc.AddXP(Time.deltaTime);
            }
        }
    }
    
    public override void OnEnd(NPCBase npc)
    {
        base.OnEnd(npc);
        // No specific end action is needed beyond base functionality.
    }

    public override bool IsFinished(NPCBase npc)
    {
  
        return ReleaseBase.State == ReleaseBase.ReleaseState.DeploymentReady;
    }
    public override string GetAssignButtonText()
    {
        return "Research????";
    }
}
