// ResearchTask.cs
using UnityEngine;
using System.Linq;

public class ResearchTask : NPCTask
{
    public Technology TargetTechnology { get; private set; }
    private Desk desk;
    private bool atDesk = false;

    public ResearchTask(Technology technology)
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

    public override void OnStart(NPCDevOps npc)
    {
        if (desk != null)
        {
            npc.MoveTo(desk.transform.position);
        }
    }

    public override void OnUpdate(NPCDevOps npc)
    {
        if (desk == null) return;
        
        // Check if the NPC has arrived at the desk.
        if (!atDesk && !npc.isMoving)
        {
            if (Vector3.Distance(npc.transform.position, desk.transform.position) < 1.5f)
            {
                atDesk = true;
            }
        }
        
        // Apply research points only if the NPC is at the desk
        if (atDesk)
        {
            float researchGained = npc.GetResearchPointsPerSecond(TargetTechnology) * Time.deltaTime;
            GameManager.Instance.ApplyResearchProgress(researchGained);
            if (desk == null) return;
            if (npc == null)
            {
                Debug.LogError("Missing `npc`");
            }
            if (npc.transform == null)
            {
                Debug.LogError("Missing `npc.transform`");
            }

      
            desk.OnResearchProgress(
                npc.transform.position
            );
        }
    }

    public override void OnEnd(NPCDevOps npc)
    {
        // No specific end action is needed.
    }

    public override bool IsFinished(NPCDevOps npc)
    {
        // The task is finished if the technology is no longer being researched.
        return TargetTechnology.CurrentState != Technology.State.Researching;
    }
}
