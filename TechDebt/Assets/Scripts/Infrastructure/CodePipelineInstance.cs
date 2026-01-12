// QueueInstance.cs
using System.Collections.Generic;
using UnityEngine;

public class CodePipelineInstance : InfrastructureInstance
{

    private DeploymentBase _currentDeployment;
    private float _deploymentProgress;
    private float _deploymentSpeed = 1f; // Adjust as needed

    
    private InfrastructureInstance _targetServer;
   
    public override void Initialize()
    {
        base.Initialize();

        GameManager.OnDeploymentChanged += OnDeploymentChanged;
    }

    public new void Start()
    {
        base.Start();
        
        
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
        
        _currentDeployment =  deploymentBase;
        _deploymentProgress = 0;
        _targetServer = FindTargetServer();
    }

    private void Update()
    {
        if (_currentDeployment != null)
        {
            _deploymentProgress += Time.deltaTime * _deploymentSpeed;
            if (_targetServer != null)
            {
                FloatingTextFactory.Instance.ShowText(
                    $"Deploying to {_targetServer.data.DisplayName}: {_deploymentProgress:F0}%",
                    transform.position
                );
            }
            
            if (_deploymentProgress >= 100)
            {
                _deploymentProgress = 0;
                _targetServer = FindTargetServer();
                if (_targetServer == null)
                {
                    _currentDeployment.State = DeploymentBase.DeploymentState.Completed;
                    _currentDeployment = null;
                }
                
            }
        }
        
    }

    private InfrastructureInstance FindTargetServer()
    {
        foreach (var infra in GameManager.Instance.ActiveInfrastructure)
        {
            Server server = infra.GetComponent<Server>();
            if (
                server == null ||
                !infra.IsActive()
            )
            {
                continue;
            }
            if (
                infra.Version != _currentDeployment.GetVersionString()
            )
            {

                return infra;
            }
        }

        return null;
    }
}


