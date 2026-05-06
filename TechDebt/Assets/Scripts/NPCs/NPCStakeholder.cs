using UnityEngine;
using System.Linq;
using Infrastructure;
using NPCs;

public class NPCStakeholder : NPCAnimatedBiped
{
    public WorldObjectBase home;
    public Stakeholder stakeholder;

    public void Initialize(Stakeholder stakeholder, WorldObjectBase home)
    {
        base.Initialize();
        this.stakeholder = stakeholder;
        Id = stakeholder.Id;
        this.home = home;
     
    }

    public override void TriggerDefaultBehavior()
    {
        
        // If we are already at the desk, do nothing.
        if (Vector3.Distance(transform.position, home.GetInteractionPosition()) < 0.1f)
        {
            FaceDown();
            return; 
        }
        

        MoveTo(home.GetInteractionPosition());
        CurrentState = State.Wandering; // Use Wandering state to signify moving without a task
     
    }
    public override bool CanAssignTask(NPCTask task)
    {
        return task.Role == NPCTask.TaskRole.Boss;
    }
    public override Vector3 GetHomePoint()
    {
        return home.GetInteractionPosition();
    }
}
