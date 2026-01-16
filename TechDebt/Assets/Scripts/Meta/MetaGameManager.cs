using UnityEngine;
using System.IO;
using System.Collections.Generic;
using MetaChallenges;

public static class MetaGameManager
{
    private static MetaProgressData _progressData;
    private static string _savePath;
    private static List<MetaChallengeBase> _challenges = new List<MetaChallengeBase>();

    public static MetaProgressData ProgressData
    {
        get
        {
            if (_progressData == null)
            {
                LoadProgress();
            }
            return _progressData;
        }
    }

    static MetaGameManager()
    {
        // Static constructor called once when the class is first accessed
        _savePath = Path.Combine(Application.persistentDataPath, "meta_progress.json");
        LoadProgress();
    }

    public static void SaveProgress()
    {
        string json = JsonUtility.ToJson(_progressData, true);
        File.WriteAllText(_savePath, json);
        Debug.Log($"Progress saved to {_savePath}");
    }

    public static void LoadProgress()
    {
        if (File.Exists(_savePath))
        {
            string json = File.ReadAllText(_savePath);
            _progressData = JsonUtility.FromJson<MetaProgressData>(json);
            Debug.Log($"Progress loaded from {_savePath}");
        }
        else
        {
            _progressData = new MetaProgressData();
            Debug.Log("No save file found. Created new progress data.");
        }
    }

    // Optional: Reset progress for testing or new game
    public static void ResetProgress()
    {
        _progressData = new MetaProgressData();
        SaveProgress();
        Debug.Log("Meta progress reset.");
    }

    public static void AggregateMetaStats(List<InfrastructureInstance> activeInfrastructure)
    {
        if (_progressData.metaStats == null)
        {
            _progressData.metaStats = new MetaStatSaveData();
        }

        if (_progressData.metaStats.infra == null)
        {
            _progressData.metaStats.infra = new List<InfraMetaStatSaveData>();
        }

        foreach (var instance in activeInfrastructure)
        {
            var infraStats = _progressData.metaStats.infra.Find(i => i.infraId == instance.data.ID);
            if (infraStats == null)
            {
                infraStats = new InfraMetaStatSaveData() { infraId = instance.data.ID };
                _progressData.metaStats.infra.Add(infraStats);
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
                RequiredTechnologies = new List<string>()
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
                TechnologyID = "load-balencer",
                DisplayName = "Load Balencer",
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
                RequiredTechnologies = new List<string>() { "load-balencer" }
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
                RequiredTechnologies = new List<string>() { "load-balencer" }
                // survive Y packets in a single run.
            },
            new Technology()
            {
                TechnologyID = "cognito",
                DisplayName = "Cognito User Pools",
                Description = "",
                ResearchPointCost = 25,
                RequiredTechnologies = new List<string>() { "load-balencer" }
                // Survive Y user info leaked
            },
            new Technology()
            {
                TechnologyID = "codepipeline",
                DisplayName = "Code Pipeline",
                Description = "",
                ResearchPointCost = 25,
                RequiredTechnologies = new List<string>() { "load-balencer" }
                // Push out Y deployments
            }
        };
        return technologies;
    }

    public static List<MetaChallengeBase> GetAllChallenges()
    {
        List<MetaChallengeBase> challenges = new List<MetaChallengeBase>()
        {
            new MetaChallengeBase()
            {
                ChallengeID = "dedicated-db",
                DisplayName =  "Dedicated Databases",
                metaStat = MetaStat.Infra_MaxSize,
                InfrastructureId = "server1",
                RewardId = "dedicated-db",
                RequiredValue = 2
                
            },
            new MetaChallengeBase()
            {
                ChallengeID = "binary-storage",
                DisplayName =  "Binary Storage",
                metaStat = MetaStat.Infra_HandleNetworkPacket,
                InfrastructureId = "server1",
                RewardId = "binary-storage",
                RequiredValue = 1000
            },
            new MetaChallengeBase()
            {
                ChallengeID = "redis",
                DisplayName =  "Redis Cache",
                metaStat = MetaStat.Infra_MaxSize,
                InfrastructureId = "dedicated-db",
                RewardId = "redis",
                RequiredValue = 2500
            }
        };
        return challenges;
    }
}
