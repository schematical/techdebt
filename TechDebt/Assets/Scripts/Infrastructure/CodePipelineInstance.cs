// QueueInstance.cs

using System;
using System.Collections.Generic;
using UnityEngine;

public class CodePipelineInstance : InfrastructureInstance
{

    private ReleaseBase _currentRelease;
    private float _deploymentProgress;
    private float _deploymentSpeed = 1f; // Adjust as needed

    
    private InfrastructureInstance _targetServer;
    private int lastDisplayedProgress;

    public override void Initialize()
    {
        base.Initialize();

        GameManager.OnReleaseChanged += ReleaseChanged;
    }

    public new void Start()
    {
        base.Start();
        
        
    }

    public void ReleaseChanged(ReleaseBase releaseBase, ReleaseBase.ReleaseState state)
    {

        if (!IsActive())
        {
            return;
        }

        if (releaseBase.State != ReleaseBase.ReleaseState.InProgress)
        {
            return;
        }
        
        _currentRelease =  releaseBase;
        _deploymentProgress = 0;
        _targetServer = FindTargetServer();
    }

    private void Update()
    {
        if (_currentRelease != null)
        {
            _deploymentProgress += Time.deltaTime * _deploymentSpeed;
            if (_targetServer != null)
            {
                int progress = (int)Math.Floor((_deploymentProgress / _currentRelease.GetDuration() )  * 100);
                if (progress % 10 == 0 && lastDisplayedProgress != progress)
                {
                    FloatingTextFactory.Instance.ShowText(
                        $"Deploying {_currentRelease.GetVersionString()} to {_targetServer.data.DisplayName}: {progress}%",
                        transform.position
                    );
                    lastDisplayedProgress = progress;
                }
                
            }
            
            if (_deploymentProgress >= _currentRelease.GetDuration())
            {
                _targetServer.Version =  _currentRelease.GetVersionString();
                
                
                _deploymentProgress = 0;
                _targetServer = FindTargetServer();
                if (_targetServer == null)
                {
                    FloatingTextFactory.Instance.ShowText(
                        $"Done Deploying {_currentRelease.GetVersionString()}",
                        transform.position
                    );
                    _currentRelease.CheckIsOver();
                    _currentRelease = null;
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
                infra.Version != _currentRelease.GetVersionString()
            )
            {

                return infra;
            }
        }

        return null;
    }
}


