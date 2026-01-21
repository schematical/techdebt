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
        switch (data.CurrentState)
        {
            case (InfrastructureData.State.Operational):
                foreach (ReleaseBase releaseBase in GameManager.Instance.Releases)
                {
                    if (releaseBase.State == ReleaseBase.ReleaseState.DeploymentReady)
                    {
                        availableTasks.Add(new DeploymentTask(this, releaseBase));
                    }
                }

                break;
        }

        return availableTasks;
    }
}
