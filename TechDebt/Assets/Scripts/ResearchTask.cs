// ResearchTask.cs
using UnityEngine;
using System.Linq;

public class ResearchTask : NPCTask
{
    public Technology TargetTechnology { get; private set; }
    private readonly Desk desk;

    public ResearchTask(Technology technology) : base(GameManager.Instance.ActiveInfrastructure.FirstOrDefault(infra => infra.data.ID == "desk")?.transform.position)
    {
        TargetTechnology = technology;
        Priority = 2; // Research is a low-priority, background task.

        // Find the desk to navigate to.
        var deskInstance = GameManager.Instance.ActiveInfrastructure.FirstOrDefault(infra => infra.data.ID == "desk");
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

    public override void OnUpdate(NPCDevOps npc)
    {
        if (desk == null) return;
        
        // Apply research points only if the NPC is at the desk
        if (hasArrived)
        {
            float researchGained = npc.GetResearchPointsPerSecond(TargetTechnology) * Time.deltaTime;
            GameManager.Instance.ApplyResearchProgress(researchGained);
      
            desk.OnResearchProgress(
                npc.transform.position
            );
        }
    }
    
    public override void OnEnd(NPCDevOps npc)
    {
        base.OnEnd(npc);
        // No specific end action is needed beyond base functionality.
    }

    public override bool IsFinished(NPCDevOps npc)
    {
        // The task is finished if the technology is no longer being researched.
        return TargetTechnology.CurrentState != Technology.State.Researching;
    }
}
