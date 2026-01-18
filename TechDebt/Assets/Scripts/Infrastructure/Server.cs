// Server.cs

using System.Collections.Generic;
using Stats;
using Unity.VisualScripting;
using UnityEngine;

public class Server : InfrastructureInstance
{
    

    public override List<NPCTask> GetAvailableTasks()
    {
        List<NPCTask> availableTasks = base.GetAvailableTasks();
        foreach (ReleaseBase deploymentBase in GameManager.Instance.Releases)
        {
            if (deploymentBase.State == ReleaseBase.ReleaseState.InProgress)
            {
                availableTasks.Add(new DeploymentTask(this, deploymentBase));
            }
        }

        return availableTasks;
    }
}
