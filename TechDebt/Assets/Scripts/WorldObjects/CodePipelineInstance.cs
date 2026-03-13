// QueueInstance.cs

using System;
using System.Collections.Generic;
using UnityEngine;

public class CodePipelineInstance : InfrastructureInstance, iProgressable
{

    private ReleaseBase _currentRelease;
    private float _deploymentProgress;
    private float _deploymentSpeed = 1f; // Adjust as needed

    
    private InfrastructureInstance _targetServer;
    private int lastDisplayedProgress;

    public override void Initialize()
    {
        base.Initialize();

        GameManager.OnReleaseChanged += (releaseBase, previousState) =>
        {
            ReleaseChanged(releaseBase, previousState);
        };
    }



    public void ReleaseChanged(ReleaseBase releaseBase, ReleaseBase.ReleaseState previousState)
    {
       
        if (!IsActive())
        {
            return;
        }
        
        if (releaseBase.State != ReleaseBase.ReleaseState.DeploymentReady)
        {
            return;
        }
        Debug.Log($"CodePipelineInstance::ReleaseChanged - HITTTTT {gameObject.name}");
        _currentRelease =  releaseBase;
        _currentRelease.SetState(ReleaseBase.ReleaseState.DeploymentInProgress);
        _deploymentProgress = 0;
        _targetServer = FindTargetServer();
        if (_targetServer == null)
        {
            Debug.LogError($"CodePipelineInstance::FindTargetServer - Could not find target server");
            return;
        }
        _targetServer.AddStatusBar(this);
    }

    public override void FixedUpdate()
    {
Debug.Log($"CodePipelineInstance::FixedUpdate - {_currentRelease}");
        if (_currentRelease == null)
        {
            if (_targetServer != null)
            {
                Debug.LogError($"This shouldn't be possible.");
            }

            return;
        }
        _deploymentProgress += Time.deltaTime * _deploymentSpeed;
        Debug.Log($"CodePipelineInstance::FixedUpdate - Deploying {_currentRelease.GetVersionString()} - _deploymentProgress");
     
        if (_targetServer != null)
        {
            int progress = (int)Math.Floor((_deploymentProgress / _currentRelease.GetRequiredProgress() )  * 100);
            if (progress % 10 == 0 && lastDisplayedProgress != progress)
            {
                FloatingTextFactory.Instance.ShowText(
                    $"Deploying {_currentRelease.GetVersionString()} to {_targetServer.data.Id}: {progress}%",
                    transform.position
                );
                lastDisplayedProgress = progress;
            }
        }
        
        if (_deploymentProgress >= _currentRelease.GetRequiredProgress())
        {
            _targetServer.Version =  _currentRelease.GetVersionString();
            
            
            _deploymentProgress = 0;
            _targetServer.HideProgressBar();
            _targetServer = FindTargetServer();
            _targetServer.AddStatusBar(this);
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

    private InfrastructureInstance FindTargetServer()
    {
        Debug.Log("CodePipelineInstance::FindTargetServer");
        foreach (var infra in GameManager.Instance.ActiveInfrastructure)
        {
            ApplicationServer applicationServer = infra.GetComponent<ApplicationServer>();
            if (
                applicationServer == null ||
                !infra.IsActive()
            )
            {
                continue;
            }
            if (
                infra.Version != _currentRelease.GetVersionString()
            )
            {

                Debug.Log($"CodePipelineInstance::FindTargetServer - Found {infra.gameObject.name}");
                return infra;
            }
        }

        return null;
    }

    public float GetProgress()
    {
        return _deploymentProgress / _currentRelease.GetRequiredProgress();
    }
}


