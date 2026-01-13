// QueueInstance.cs

using System;
using System.Collections.Generic;
using UnityEngine;

public class CodePipelineInstance : InfrastructureInstance
{

    private DeploymentBase _currentDeployment;
    private float _deploymentProgress;
    private float _deploymentSpeed = 1f; // Adjust as needed

    
    private InfrastructureInstance _targetServer;
    private int lastDisplayedProgress;

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
                int progress = (int)Math.Floor((_deploymentProgress / _currentDeployment.GetDuration() )  * 100);
                if (progress % 10 == 0 && lastDisplayedProgress != progress)
                {
                    FloatingTextFactory.Instance.ShowText(
                        $"Deploying {_currentDeployment.GetVersionString()} to {_targetServer.data.DisplayName}: {progress}%",
                        transform.position
                    );
                    lastDisplayedProgress = progress;
                }
                
            }
            
            if (_deploymentProgress >= _currentDeployment.GetDuration())
            {
                _targetServer.Version =  _currentDeployment.GetVersionString();
                
                
                _deploymentProgress = 0;
                _targetServer = FindTargetServer();
                if (_targetServer == null)
                {
                    FloatingTextFactory.Instance.ShowText(
                        $"Done Deploying {_currentDeployment.GetVersionString()}",
                        transform.position
                    );
                    _currentDeployment.CheckIsOver();
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


