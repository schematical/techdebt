// QueueInstance.cs
using UnityEngine;

public class CodePipelineInstance : InfrastructureInstance
{
   
    public override void Initialize()
    {
        base.Initialize();
        
    }

    public void Start()
    {
        base.Start();
        GameManager.OnDeploymentChanged += OnDeploymentChanged;
    }

    public void OnDeploymentChanged(DeploymentBase deploymentBase, DeploymentBase.DeploymentState state)
    {
        if (!IsActive())
        {
            return;
        }

        if (deploymentBase.State != DeploymentBase.DeploymentState.InProgress)
        {
            return;
        }
        // Start deploying
    }
   
    
    
  
}


