
using System;
using System.Collections.Generic;
using DefaultNamespace;
using DefaultNamespace.Rewards;
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
        DeploymentRewardReady,
        DeploymentCompleted,
        Failed
    }

    public string ServiceId { get; set; } = "monolith";
    public int Version  { get; set; }
    public StatsCollection Stats = new StatsCollection();
    public List<NPCBug> bugs = new List<NPCBug>();
 
    // MinorBugs - that has a negative impact on the amount of money you make each day.
    // MajorBugs - Knock down the whole infrastructureInstance
    // TODO: Optimized score, gives a latency bonus or a load bonus
    public ReleaseState State { get; private set; } = ReleaseState.InDevelopment;

    public RewardBase RewardModifier;
    public float CurrentProgress = 0f;
    public float RequiredProgress =  30f;
    public Rarity rewardRarity = Rarity.Common;
    public float CurrentQuality = 0f;
    public float TechDebtMultiplier = 1f;
    public ReleaseBase()
    {
        SetState(ReleaseState.InDevelopment);
        // Stats.Add(new StatData(StatType.Release_Security));
    }

    public ReleaseBase(int version, RewardBase modifierBase)
    {
        Version = version;
        RewardModifier = modifierBase;
        float durationMultiplier = 1;
        if (modifierBase is LeveledRewardBase)
        {
            durationMultiplier = (modifierBase as LeveledRewardBase).GetLevel();
        }
        RequiredProgress = (float) (30f * Math.Pow(1.25f, durationMultiplier));
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
        SetState(ReleaseState.DeploymentRewardReady);
        List<ApplicationServer> targets = GetAllReleaseTargets();
        if (targets.Count == 0)
        {
            throw new SystemException("How did this release go with no targets?");
        }
        targets[0].ZoomTo();
        rewardRarity = RarityHelper.GetRandomRarity(GetQuality()); //TODO: Feed in release quality to this
        GameManager.Instance.UIManager.rewardPanel.Show(this);
        
        
        return true;
    }

    public float GetQuality()
    {
        return CurrentQuality;
    }
    public void OnDeploymentCompleted()
    {
       
        if (RewardModifier != null)
        {
         
            RewardBase existingModifierBase = GameManager.Instance.Rewards.Rewards.Find((r) => r.Id == RewardModifier.Id);
            if(existingModifierBase == null){
                
                Debug.Log($"Modifier Base Did NOT Exist: {RewardModifier.Id} - Count: {GameManager.Instance.Rewards.Rewards.Count}");
                GameManager.Instance.AddModifier(RewardModifier);
            }
            else
            {
                if (RewardModifier is LeveledRewardBase)
                {
                    (RewardModifier as LeveledRewardBase).LevelUp(rewardRarity);
                }
                Debug.Log($"Modifier Base Exists: {existingModifierBase.Id} - Count: {GameManager.Instance.Rewards.Rewards.Count}");
            }
            RewardModifier.Apply();
        
        }

        float techDebt = GameManager.Instance.GetStatValue(StatType.TechDebt);
        GameManager.Instance.SetStat(StatType.TechDebt, techDebt * TechDebtMultiplier);
        GameManager.Instance.MetaStats.Incr(MetaStat.Deployments);
       
    }

    public void SpawnBug()
    {
        NPCBug npcBug = GameManager.Instance.SpawnNPCBug();
        bugs.Add(npcBug);
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
            case(ReleaseState.DeploymentInProgress):
                foreach (ApplicationServer applicationServer in GetUndeployedReleaseTargets())
                {
                    applicationServer.HideAttentionIcon();
                }

                break;
            case(ReleaseState.DeploymentCompleted):
                OnDeploymentCompleted();
                break;
        }
        GameManager.Instance.InvokeReleaseChanged(this, prevState);
    }

    public float GetRequiredProgress()
    {
        return RequiredProgress;
    }

    public string GetDescription()
    {
        return $"{GetVersionString()} {State.ToString()} - Quality: {Math.Round(GetQuality() * 100)}% - Tech Debt Multiplier: {TechDebtMultiplier:F2}";
    }

    public void ApplyProgress(float progressGained, NPCBase NPCBase)
    {
        // Debug.Log($"ReleaseBase.ApplyProgress: {CurrentProgress} += {progressGained}");
        CurrentProgress += progressGained;
        GameManager.Instance.InvokeReleaseChanged(this, State);
        
        float qualityMultiplier = GameManager.Instance.GetStatValue(StatType.Release_Quality_Multiplier) * NPCBase.Stats.GetStatValue(StatType.NPC_CodeQuality);
       
        CurrentQuality = AvgOutStat(
            CurrentQuality,
            qualityMultiplier,
            CurrentProgress,
            progressGained
        );
        /*TechDebtMultiplier = (
            (
                (TechDebtMultiplier * (CurrentProgress - progressGained)) + (NPCBase.Stats.GetStatValue(StatType.NPC_Release_TechDebt) * progressGained)
            ) / (CurrentProgress));*/
        TechDebtMultiplier = AvgOutStat(
            TechDebtMultiplier,
            NPCBase.Stats.GetStatValue(StatType.NPC_Release_TechDebt),
            CurrentProgress,
            progressGained
        );
        if (CurrentProgress >= RequiredProgress)
        {
            NextState();
        }
    }

    private float AvgOutStat(float currentStatVal, float newStatVal, float currentProgress, float progressGained)
    {
        float currentWeight = (currentProgress - progressGained);
        return (
            (
                (currentStatVal * currentWeight) + (newStatVal * progressGained)
            ) 
            / (currentProgress)
        );
    }

    public float GetProgress()
    {
        return CurrentProgress / RequiredProgress;
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
                SetState(ReleaseState.DeploymentRewardReady);
                break; 
            case(ReleaseState.DeploymentRewardReady):
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
