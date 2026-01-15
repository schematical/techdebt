using UnityEngine;
using System.IO;
using System.Collections.Generic;

public static class MetaGameManager
{
    private static MetaProgressData _progressData;
    private static string _savePath;

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
            },
            new Technology()
            {
                TechnologyID = "binary-storage",
                DisplayName = "Binary Storage",
                Description = "",
                ResearchPointCost = 40,
                RequiredTechnologies = new List<string>()
            },
            new Technology()
            {
                TechnologyID = "redis",
                DisplayName = "Redis Caching",
                Description = "",
                ResearchPointCost = 25,
                RequiredTechnologies = new List<string>() { "dedicated-db" }
            },
            new Technology()
            {
                TechnologyID = "cdn",
                DisplayName = "Content Delivery Network(CDN)",
                Description = "",
                ResearchPointCost = 5,
                RequiredTechnologies = new List<string>() { "binary-storage" }
            },
            new Technology()
            {
                TechnologyID = "load-balencer",
                DisplayName = "Load Balencer",
                Description = "",
                ResearchPointCost = 30,
                RequiredTechnologies = new List<string>() { "dedicated-db", "binary-storage" }
            },
            new Technology()
            {
                TechnologyID = "read-replicas",
                DisplayName = "Read Replicas",
                Description = "",
                ResearchPointCost = 60,
                RequiredTechnologies = new List<string>() { "dedicated-db" }
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
            },
            new Technology()
            {
                TechnologyID = "secret-manager",
                DisplayName = "Secret Manager",
                Description = "",
                ResearchPointCost = 25,
                RequiredTechnologies = new List<string>()
            },
            new Technology()
            {
                TechnologyID = "sqs",
                DisplayName = "Simple Queue Service",
                Description = "",
                ResearchPointCost = 25,
                RequiredTechnologies = new List<string>() { "load-balencer" }
            },
            new Technology()
            {
                TechnologyID = "cognito",
                DisplayName = "Cognito User Pools",
                Description = "",
                ResearchPointCost = 25,
                RequiredTechnologies = new List<string>() { "load-balencer" }
            },
            new Technology()
            {
                TechnologyID = "codepipeline",
                DisplayName = "Code Pipeline",
                Description = "",
                ResearchPointCost = 25,
                RequiredTechnologies = new List<string>() { "load-balencer" }
            }
        };
        return technologies;
    }
}
