
using System;
using MetaChallenges;
using NPCs;
using Stats;
using UnityEngine;

public class ReleaseBase
{

    public static int GlobalVersion = 0;
    public enum ReleaseState
    {
        InDevelopment,
        InTesting,
        InReview,
        DeploymentReady,
        DeploymentInProgress,
        DeploymentCompleted,
        Failed
    }

    public string ServiceId { get; set; } = "monolith";
    public int Version  { get; set; }
    public StatsCollection Stats = new StatsCollection();
    // TODO Create a hidden "Bug Count"
    // MinorBugs - that has a negative impact on the amount of money you make each day.
    // MajorBugs - Knock down the whole infrastructureInstance
    // TODO: Optimized score, gives a latency bonus or a load bonus
    public ReleaseState State { get; private set; } = ReleaseState.InDevelopment;

    public ModifierBase RewardModifier;
    public float CurrentProgress = 0f;
    public float RequiredProgress =  30f;
    public ReleaseBase()
    {
        SetState(ReleaseState.InDevelopment);
        // Stats.Add(new StatData(StatType.Release_Security));
    }

    public ReleaseBase(int version, ModifierBase modifierBase)
    {
        Version = version;
        RewardModifier = modifierBase;
        RequiredProgress = (float) (30f * Math.Pow(1.25f, modifierBase.Level));
    }


    public void QueueUpTask()
    {
        foreach (var infra in GameManager.Instance.ActiveInfrastructure)
        {
            ApplicationServer applicationServer = infra.GetComponent<ApplicationServer>();
            if (applicationServer != null && infra.data.CurrentState == InfrastructureData.State.Operational)
            {
                GameManager.Instance.AddTask(new DeploymentTask(applicationServer, this));
            }
        }
    }
    public string GetVersionString()
    {
        return $"0.{Version}.0";
    }
    public bool CheckIsOver()
    {
            
            
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
                infra.Version != GetVersionString()
            )
            {
                
                return false;
            }
        }
        SetState(ReleaseState.DeploymentCompleted);
        GameManager.Instance.UIManager.rewardPanel.Show(() =>
        {
            Debug.Log("TODO Add a marker to the whiteboard to create new release.");
        });
        OnDeploymentCompleted();
        return true;
    }

    public void OnDeploymentCompleted()
    {
        if (RewardModifier != null)
        {
            if(!GameManager.Instance.Modifiers.Modifiers.Contains(RewardModifier)){
                GameManager.Instance.AddModifier(RewardModifier);
            }
            else
            {
                RewardModifier.LevelUp();
            }
        }

        GameManager.Instance.MetaStats.Incr(MetaStat.Deployments);
    }
    public void SetState(ReleaseState state)
    {
        ReleaseState prevState = State;
        
        State = state;
        GameManager.Instance.InvokeReleaseChanged(this, prevState);
    }

    public float GetDuration()
    {
        return 30;
    }

    public string GetDescription()
    {
        return $"{GetVersionString()} {State.ToString()}";
    }

    public void ApplyProgress(float progressGained, NPCBase NPCBase)
    {
        // Debug.Log($"ReleaseBase.ApplyProgress: {CurrentProgress} += {progressGained}");
        CurrentProgress += progressGained;
        GameManager.Instance.InvokeReleaseChanged(this, this.State);
        if (CurrentProgress >= RequiredProgress)
        {
            NextState();
        }
    }

    public void NextState()
    {
        switch (State)
        {
            case(ReleaseState.InDevelopment):
                SetState(ReleaseState.DeploymentReady);
                CurrentProgress = 0;
                RequiredProgress = 30f;
                break;
            /*case(ReleaseState.InReview):
                SetState(ReleaseState.InTesting);
                CurrentProgress = 0;
                RequiredProgress = 30f;
                break;
            case(ReleaseState.InTesting):
                SetState(ReleaseState.DeploymentReady);
                CurrentProgress = 0;
                RequiredProgress = 30f;
                break; */
            case(ReleaseState.DeploymentReady):
                SetState(ReleaseState.DeploymentInProgress);
                CurrentProgress = 0;
                RequiredProgress = 30f;
                break;
            case(ReleaseState.DeploymentInProgress):
                SetState(ReleaseState.DeploymentCompleted);
                CurrentProgress = 0;
                RequiredProgress = 30f;
                break;
            default:
                throw new System.Exception($"ReleaseBase.NextState - No NextStep for {State}");
        }
    }

    public static int IncrGlobalVersion()
    {
        GlobalVersion += 1;
        return GlobalVersion;
    }
}
