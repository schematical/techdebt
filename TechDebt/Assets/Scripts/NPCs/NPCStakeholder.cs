using UnityEngine;
using System.Linq;
using NPCs;

public class NPCStakeholder : NPCAnimatedBiped
{
    public InfrastructureInstance home;
    public Stakeholder stakeholder;

    public void Initialize(Stakeholder stakeholder)
    {
        base.Initialize();
        this.stakeholder = stakeholder;
        Id = stakeholder.Id;
        home = GameManager.Instance.ActiveInfrastructure.FirstOrDefault(i => i.data.Id == "boss-desk");
        if (home == null)
        {
            Debug.LogError("NPCStakeholder: Could not find 'boss-desk'. NPCStakeholder will wander.");
        }
    }

    /*public override void TriggerDefaultBehavior()
    {
        
        // If we are already at the desk, do nothing.
        if (Vector3.Distance(transform.position, home.GetInteractionPosition()) < 0.1f)
        {
            return; 
        }
        

        MoveTo(home.GetInteractionPosition());
        CurrentState = State.Wandering; // Use Wandering state to signify moving without a task
     
    }*/
    public override bool CanAssignTask(NPCTask task)
    {
        return task.Role == NPCTask.TaskRole.Boss;
    }
    public override Vector3 GetHomePoint()
    {
        return home.transform.position;
    }
}
