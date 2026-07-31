// Server.cs

using System.Collections.Generic;
using Stats;
using Unity.VisualScripting;
using UnityEngine;

public class ApplicationServer : InfrastructureInstance
{
    

    public override List<NPCTask> GetAvailableTasks()
    {
        List<NPCTask> availableTasks = base.GetAvailableTasks();
        availableTasks.Reverse();
        switch (CurrentState)
        {
            case (State.Operational):
                foreach (ReleaseBase releaseBase in GameManager.Instance.Releases)
                {
                    if (releaseBase.State == ReleaseBase.ReleaseState.DeploymentReady)
                    {
                        availableTasks.Add(new DeploymentTask(this, releaseBase));
                    }
                }

                break;
        }
        availableTasks.Reverse();
        return availableTasks;
    }

    public override void SetState(State newState)
    {

        if (
            CurrentState == State.Planned && 
            newState == State.Operational
        )
        {
            List<InternetPipe> instances =
                GameManager.Instance.GetWorldObjectByClass<InternetPipe>();
            foreach (InternetPipe pipe in instances)
            {
                pipe.TrafficCheck();
            }
        }
        base.SetState(newState);
    }
}
