// ResearchTask.cs
using UnityEngine;
using System.Linq;

public class ResearchTask : NPCTask
{
    public Technology TargetTechnology { get; private set; }
    private Vector3 deskPosition;
    private bool atDesk = false;

    public ResearchTask(Technology technology)
    {
        TargetTechnology = technology;
        Priority = 0; // Research is a low-priority, background task.

        // Find the desk to navigate to.
        var deskData = GameManager.Instance.AllInfrastructure.FirstOrDefault(infra => infra.ID == "desk");
        if (deskData != null && deskData.Instance != null)
        {
            deskPosition = deskData.Instance.transform.position;
        }
        else
        {
            Debug.LogError("ResearchTask could not be created. No 'desk' infrastructure found or its instance is null.");
            // In a real game, we might want to handle this more gracefully.
            deskPosition = Vector3.zero; // Default position
        }
    }

    public override void OnStart(NPCDevOps npc)
    {
        if (deskPosition != Vector3.zero)
        {
            npc.MoveTo(deskPosition);
        }
    }

    public override void OnUpdate(NPCDevOps npc)
    {
        // Check if the NPC has arrived at the desk.
        if (!atDesk && !npc.isMoving)
        {
            // A simple distance check to confirm arrival, since isMoving might be false for a frame before starting.
            if (Vector3.Distance(npc.transform.position, deskPosition) < 1.5f) // Using a small tolerance
            {
                atDesk = true;
            }
        }
        
        // Apply research points only if the NPC is at the desk
        if (atDesk)
        {
            float researchGained = npc.GetResearchPointsPerSecond(TargetTechnology) * Time.deltaTime;
            GameManager.Instance.ApplyResearchProgress(researchGained);
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
