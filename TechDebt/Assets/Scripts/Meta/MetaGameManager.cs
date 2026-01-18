using System;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using MetaChallenges;
using Release;

public static class MetaGameManager
{
    private static List<MetaChallengeBase> _challenges = new List<MetaChallengeBase>();

    public static MetaProgressData ProgressData
    {
        get { return LoadProgress(); }
    }

    static MetaGameManager()
    {
        // Static constructor called once when the class is first accessed

        LoadProgress();
    }

    static string GetSavePath()
    {
        return Path.Combine(Application.persistentDataPath, "meta_progress.json");
    }

    public static void SaveProgress(MetaProgressData metaProgressData)
    {
        string json = JsonUtility.ToJson(metaProgressData, true);
        File.WriteAllText(GetSavePath(), json);
        Debug.Log($"Progress saved to {GetSavePath()}");
    }

    public static MetaProgressData LoadProgress()
    {
        if (!File.Exists(GetSavePath()))
        {
            return new MetaProgressData();
        }

        string json = File.ReadAllText(GetSavePath());
        return JsonUtility.FromJson<MetaProgressData>(json);
    }


    // Optional: Reset progress for testing or new game
    public static void ResetProgress()
    {
        SaveProgress(new MetaProgressData());
    }

    public static MetaProgressData GetUpdatedMetaStats(List<InfrastructureInstance> activeInfrastructure)
    {
        MetaProgressData progressData = LoadProgress();
        
        if (progressData.metaStats.game == null)
        {
            progressData.metaStats.game = new List<MetaStatPair>();
        }

        
        foreach (var stat in GameManager.Instance.MetaStats.Stats)
        {
            var statPair = 
                progressData.metaStats.game.Find(s => s.statName == stat.Key.ToString());
            if (statPair == null)
            {
                statPair = new MetaStatPair() { statName = stat.Key.ToString() };
                progressData.metaStats.game.Add(statPair);
            }

            statPair.value += stat.Value;
        }
        
        
        
        if (progressData.metaStats == null)
        {
            progressData.metaStats = new MetaStatSaveData();
        }

        foreach (var instance in activeInfrastructure)
        {
            var infraStats = progressData.metaStats.infra.Find(i => i.infraId == instance.data.ID);
            if (infraStats == null)
            {
                infraStats = new InfraMetaStatSaveData() { infraId = instance.data.ID };
                progressData.metaStats.infra.Add(infraStats);
            }

            foreach (var stat in instance.metaStatCollection.Stats)
            {
                var statPair = infraStats.stats.Find(s => s.statName == stat.Key.ToString());
                if (statPair == null)
                {
                    statPair = new MetaStatPair() { statName = stat.Key.ToString() };
                    infraStats.stats.Add(statPair);
                }

                statPair.value += stat.Value;
            }
        }

        return progressData;
    }

    public static bool IsChallengeCompleted(MetaProgressData state, MetaChallengeBase challenge)
    {
        int value = 0;
        if (state == null)
        {
            throw new SystemException("`state` is null");
        }
        if (state.metaStats == null)
        {
            throw new SystemException("`state.metaStats` is null");
        }
        if (state.metaStats.infra == null)
        {
            throw new SystemException("`state.metaStats.infra` is null");
        }
        var prevInfraStats = state.metaStats.infra.Find(i => i.infraId == challenge.InfrastructureId);
        if (prevInfraStats != null)
        {
            var statPair = prevInfraStats.stats.Find(s => s.statName == challenge.metaStat.ToString());
            if (statPair != null)
            {
                value = statPair.value;
            }
        }
        // Check if the challenge was incomplete before but is complete now.
        return (value >= challenge.RequiredValue);
    }
    public static List<MetaChallengeBase> GetUnlockedChallenges(MetaProgressData state = null)
    {
        if (state == null)
        {
            state = LoadProgress();
        }
        List<MetaChallengeBase> completedChallenges = new List<MetaChallengeBase>();
        List<MetaChallengeBase> allChallenges = GetAllChallenges(); // Get challenge definitions

        foreach (var challenge in allChallenges)
        {
            // Calculate the progress from AFTER this run (from the current in-memory data).
            if(IsChallengeCompleted(state, challenge)){
                completedChallenges.Add(challenge);
            }
        }

        return completedChallenges;
    }

    public static List<MetaChallengeBase> CheckChallengeProgress(
        MetaProgressData prevState,
        MetaProgressData nextState
    )
    {
        List<MetaChallengeBase> prevCompletedChallenges = GetUnlockedChallenges(prevState);
        List<MetaChallengeBase> currCompletedChallenges = GetUnlockedChallenges(nextState);
        List<MetaChallengeBase> diffChallenges = new List<MetaChallengeBase>();
        foreach (MetaChallengeBase currChallenge in currCompletedChallenges)
        {
            if(!prevCompletedChallenges.Contains(currChallenge)){
                diffChallenges.Add(currChallenge);
            }
        }
        return diffChallenges;
    }

    public static List<Technology> GetAllTechnologies()
    {
        List<Technology> technologies = new List<Technology>()
        {
            new Technology()
            {
                TechnologyID = "dedicated-db",
                DisplayName = "Dedicated Database",
                Description = "",
                ResearchPointCost = 30,
                RequiredTechnologies = new List<string>(),
                CurrentState = Technology.State.Locked
                // UnlockConditions - Get and instance to size 2?
            },
            new Technology()
            {
                TechnologyID = "binary-storage",
                DisplayName = "Binary Storage",
                Description = "",
                ResearchPointCost = 40,
                RequiredTechnologies = new List<string>()
                // Serve up X binary image packets with server-1
            },
            new Technology()
            {
                TechnologyID = "redis",
                DisplayName = "Redis Caching",
                Description = "",
                ResearchPointCost = 25,
                RequiredTechnologies = new List<string>() { "dedicated-db" }

                // Serve up X text packets with the Dedicated DB
            },
            new Technology()
            {
                TechnologyID = "cdn",
                DisplayName = "Content Delivery Network(CDN)",
                Description = "",
                ResearchPointCost = 5,
                RequiredTechnologies = new List<string>() { "binary-storage" }
                // serve up X binary packets with the s3 bucket
            },
            new Technology()
            {
                TechnologyID = "load-balancer",
                DisplayName = "Load Balancer",
                Description = "",
                ResearchPointCost = 30,
                RequiredTechnologies = new List<string>() { "dedicated-db", "binary-storage" }
                // ??? Make it to day y?
            },
            new Technology()
            {
                TechnologyID = "read-replicas",
                DisplayName = "Read Replicas",
                Description = "",
                ResearchPointCost = 60,
                RequiredTechnologies = new List<string>() { "dedicated-db" }
                // Scale up your dedicated-db to level 2
            },
            new Technology()
            {
                TechnologyID = "water-cooler",
                DisplayName = "Water Cooler",
                Description = "",
                ResearchPointCost = 5,
                RequiredTechnologies = new List<string>() { "dedicated-db" }
            },
            new Technology()
            {
                TechnologyID = "waf",
                DisplayName = "Web Application Firewall(WAF)",
                Description = "",
                ResearchPointCost = 25,
                RequiredTechnologies = new List<string>() { "load-balancer" }
                // Survive X malicious packets
            },
            new Technology()
            {
                TechnologyID = "secret-manager",
                DisplayName = "Secret Manager",
                Description = "",
                ResearchPointCost = 25,
                RequiredTechnologies = new List<string>()
                // Cycle credentials Y times
            },
            new Technology()
            {
                TechnologyID = "sqs",
                DisplayName = "Simple Queue Service",
                Description = "",
                ResearchPointCost = 25,
                RequiredTechnologies = new List<string>() { "load-balancer" }
                // survive Y packets in a single run.
            },
            new Technology()
            {
                TechnologyID = "cognito",
                DisplayName = "Cognito User Pools",
                Description = "",
                ResearchPointCost = 25,
                RequiredTechnologies = new List<string>() { "load-balancer" }
                // Survive Y user info leaked
            },
            new Technology()
            {
                TechnologyID = "codepipeline",
                DisplayName = "Code Pipeline",
                Description = "",
                ResearchPointCost = 25,
                RequiredTechnologies = new List<string>() { "load-balancer" }
                // Push out Y deployments
            }
        };
        return technologies;
    }


    public static List<MetaChallengeBase> GetAllChallenges()
    {
        List<MetaChallengeBase> challenges = new List<MetaChallengeBase>()
        {
            /*new MetaChallengeBase()
            {
                ChallengeID = "dedicated-db",
                DisplayName = "Dedicated Databases",
                metaStat = MetaStat.Infra_MaxSize,
                InfrastructureId = "server1",
                RewardId = "dedicated-db",
                RequiredValue = 2
            },*/
            new MetaChallengeBase()
            {
                ChallengeID = "binary-storage",
                DisplayName = "Binary Storage",
                metaStat = MetaStat.Infra_HandleNetworkPacket,
                InfrastructureId = "server1",
                RewardId = "binary-storage",
                RequiredValue = 100
            },
            new MetaChallengeBase()
            {
                ChallengeID = "redis",
                DisplayName = "Redis Cache",
                metaStat = MetaStat.Infra_HandleNetworkPacket,
                InfrastructureId = "dedicated-db",
                RewardId = "redis",
                RequiredValue = 100
            },
            new MetaChallengeBase()
            {
                ChallengeID = "cash1",
                DisplayName = "Extra Starting Cash 1",
                metaStat = MetaStat.Day,
                RewardId = StatType.Money.ToString(),
                RewardValue = 2f,
                RequiredValue = 5
            },
            new MetaChallengeBase()
            {
                ChallengeID = "cdn",
                DisplayName = "Content Delivery Network(CDN)",
                metaStat = MetaStat.Infra_HandleNetworkPacket,
                InfrastructureId = "s3-bucket",
                RewardId = "cdn",
                RequiredValue = 200
            },
            new MetaChallengeBase()
            {
                ChallengeID = "load-balancer",
                DisplayName = "Load Balancer",
                metaStat = MetaStat.Day,
                RewardId = StatType.Money.ToString(),
                RewardValue = 2f,
                RequiredValue = 9
            },
            new MetaChallengeBase()
            {
                ChallengeID = "read-replicas",
                DisplayName = "Read Replicas",
                metaStat = MetaStat.Infra_MaxSize,
                InfrastructureId = "dedicated-db",
                RewardId = "read-replicas",
                RequiredValue = 2
            },
            new MetaChallengeBase()
            {
                ChallengeID = "codepipeline",
                DisplayName = "Code Pipeline",
                metaStat = MetaStat.Deployments,
                RewardId = "codepipeline",
                RequiredValue = 50
            },
        };
        
        return challenges;
    }

    public static List<ReleaseRewardBase> GetAllReleaseRewards()
    {
        return new List<ReleaseRewardBase>()
        {
            new ImageOptimizationReward()
            
        };
    }
}