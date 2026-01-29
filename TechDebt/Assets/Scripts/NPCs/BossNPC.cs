using UnityEngine;
using System.Linq;

public class BossNPC : NPCBase
{
    private InfrastructureInstance _bossDesk;

    public override void Initialize()
    {
        base.Initialize();
        _bossDesk = GameManager.Instance.ActiveInfrastructure.FirstOrDefault(i => i.data.ID == "boss-desk");
        if (_bossDesk == null)
        {
            Debug.LogError("BossNPC: Could not find 'boss-desk'. Boss will wander.");
        }
    }

    public override void TriggerDefaultBehavior()
    {
        // Boss-specific idle behavior: 50% chance to go to desk, otherwise wander

        // If we are already at the desk, do nothing.
        if (Vector3.Distance(transform.position, _bossDesk.GetInteractionPosition()) < 0.1f)
        {
            return; 
        }
        
        // ShowWordBubble("I better get back to work");
        Sprite portrait = GetComponent<SpriteRenderer>().sprite;
       
        MoveTo(_bossDesk.transform.position);
        CurrentState = State.Wandering; // Use Wandering state to signify moving without a task
     
    }
    public override bool CanAssignTask(NPCTask task)
    {
        return task.Role == NPCTask.TaskRole.Boss;
    }
    public override Vector3 GetHomePoint()
    {
        return GameManager.Instance.GetInfrastructureInstanceByID("big-desk").transform.position;
    }
}
