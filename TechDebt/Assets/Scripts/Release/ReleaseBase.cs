
using System;
using System.Collections.Generic;
using DefaultNamespace;
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
    public List<NPCBug> bugs = new List<NPCBug>();
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
        RequiredProgress = (float) (30f * Math.Pow(1.25f, modifierBase.GetLevel()));
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

    public List<ApplicationServer> GetAllReleaseTargets()
    {
        List<ApplicationServer> targets = new List<ApplicationServer>();
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

            targets.Add(applicationServer);
        }
        return targets;
        
    }
    public List<ApplicationServer> GetUndeployedReleaseTargets()
    {
        List<ApplicationServer> targets = new List<ApplicationServer>();
        foreach (var applicationServer in GetAllReleaseTargets())
        {
           
            if (
                applicationServer.Version != GetVersionString()
            )
            {
                
                targets.Add(applicationServer);
            }
        }

        return targets;
    }
    public bool CheckIsOver()
    {


        if (GetUndeployedReleaseTargets().Count > 0)
        {
            return false;
        }
        SetState(ReleaseState.DeploymentCompleted);
        List<ApplicationServer> targets = GetAllReleaseTargets();
        if (targets.Count == 0)
        {
            throw new SystemException("How did this release go with no targets?");
        }
        GameManager.Instance.cameraController.ZoomTo(targets[0].transform);
        foreach (ApplicationServer applicationServer in targets)
        {
            applicationServer.ShowLevelUpGraphic(Rarity.Common, () =>
            {
                GameManager.Instance.UIManager.Resume();
                if (GameManager.Instance.Tutorial != null)
                {
                    GameManager.Instance.Tutorial.OnRewardsPanelDone();
                }
            });
        }
        
   
        
        OnDeploymentCompleted();
        return true;
    }

    public void OnDeploymentCompleted()
    {
        if (RewardModifier != null)
        {
            Rarity rarity = RarityHelper.GetRandomRarity(); //TODO: Feed in release quality to this
            if(!GameManager.Instance.Modifiers.Modifiers.Contains(RewardModifier)){
                GameManager.Instance.AddModifier(RewardModifier);
            }
            else
            {
                RewardModifier.LevelUp(rarity);
            }
        }

        GameManager.Instance.MetaStats.Incr(MetaStat.Deployments);
        // TODO Rework this so it has to do with the devs skill
        for (int i = 0; i < RewardModifier.GetLevel(); i++)
        {
            NPCBug npcBug = GameManager.Instance.SpawnNPCBug();
            bugs.Add(npcBug);
        }
        
    }
    public void SetState(ReleaseState state)
    {
        ReleaseState prevState = State;
        
        State = state;
        switch (State)
        {
            case(ReleaseState.DeploymentReady):
                foreach (ApplicationServer applicationServer in GetUndeployedReleaseTargets())
                {
                    applicationServer.ShowAttentionIcon();
                }

                break;
        }
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

    public bool HasOpenBugs()
    {
        foreach (NPCBug bug in bugs)
        {
            if (!bug.IsDead())
            {
                return true;
            }
        }

        return false;
    }
    public static int IncrGlobalVersion()
    {
        GlobalVersion += 1;
        return GlobalVersion;
    }
    
}
