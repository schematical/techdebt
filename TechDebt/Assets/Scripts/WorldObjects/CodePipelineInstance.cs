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

        GameManager.OnReleaseChanged -= ReleaseChanged;
        GameManager.OnReleaseChanged += ReleaseChanged;
    }

    protected virtual void OnDestroy()
    {
        GameManager.OnReleaseChanged -= ReleaseChanged;
    }



    public void ReleaseChanged(ReleaseBase releaseBase, ReleaseBase.ReleaseState previousState)
    {
      
       
        if (!IsActive() || !gameObject.activeInHierarchy)
        {
            return;
        }
        
        if (releaseBase.State != ReleaseBase.ReleaseState.DeploymentReady)
        {
            return;
        }
       
    }

    public override void FixedUpdate()
    {

        _currentRelease = GameManager.Instance.GetCurrentRelease();
        if (_currentRelease == null)
        {
            return;
        }

        switch (_currentRelease.State)
        {
            case(ReleaseBase.ReleaseState.DeploymentReady):
                Debug.Log($"CodePipelineInstance::FixedUpdate - HITTTTT {gameObject.name}");
                _currentRelease.SetState(ReleaseBase.ReleaseState.DeploymentInProgress);
                _deploymentProgress = 0;
                _targetServer = FindTargetServer();
                if (_targetServer == null)
                {
                    Debug.LogError($"CodePipelineInstance::FindTargetServer - Could not find target server");
                    return;
                }
                _targetServer.AddStatusBar(this);
                break;
            case(ReleaseBase.ReleaseState.DeploymentInProgress):
                break;
            default:
                return;
        }
     
        _deploymentProgress += Time.deltaTime * _deploymentSpeed;
       
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

            if (_targetServer != null)
            {
                _targetServer.AddStatusBar(this);
            }else{

                FloatingTextFactory.Instance.ShowText(
                    $"Done Deploying {_currentRelease.GetVersionString()}",
                    transform.position,
                    Color.green
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


