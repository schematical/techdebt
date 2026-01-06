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
            Debug.LogWarning("BossNPC: Could not find 'boss-desk'. Boss will wander.");
        }
    }

    public override void TriggerDefaultBehavior()
    {
       
        // Boss-specific idle behavior: 50% chance to go to desk, otherwise wander
        if (_bossDesk != null && Random.value > 0.5f)
        {
            MoveTo(_bossDesk.transform.position);
            CurrentState = State.Wandering; // Use Wandering state to signify moving without a task
        }
        else
        {
            Wander();
        }
        
    }
}
