using System;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using DefaultNamespace;
using DefaultNamespace.Rewards;
using Infrastructure;
using MetaChallenges;
using NPCs;
using Stats;
using Tutorial;
using UnityEngine;
using Random = UnityEngine.Random;


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

    public static string GetSavePath(string foldername = "techdebt", string filename = "meta_progress.json")
    {

        #if !UNITY_EDITOR && UNITY_WEBGL
                var	path = System.IO.Path.Combine("idbfs", foldername);  //	Path: "/idbfs/<foldername>"
        #else         
		        var	path = System.IO.Path.Combine(Application.persistentDataPath, foldername); 
        #endif
        
        if (!System.IO.Directory.Exists(path)) {
            //Console.WriteLine("Creating save directory: " + path);
            System.IO.Directory.CreateDirectory(path);
        }
        var result = System.IO.Path.Combine(path, filename);	//	File Path: "/idbfs/<foldername>/<filename>"
        return result;
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

    public static MetaProgressData GetUpdatedMetaStats(List<WorldObjectType> worldObjectTypes)
    {
        MetaProgressData progressData = LoadProgress();
        
        if (progressData.metaStats.game == null)
        {
            progressData.metaStats.game = new List<MetaStatData>();
        }

        
        foreach (var stat in GameManager.Instance.MetaStats.Stats)
        {
            MetaStatData statData = 
                progressData.metaStats.game.Find(s => s.statName == stat.Key.ToString());
            if (statData == null)
            {
                statData = new MetaStatData() { statName = stat.Key.ToString() };
                progressData.metaStats.game.Add(statData);
            }
            if (stat.Value > statData.highestValue)
            {
                statData.highestValue = stat.Value;
            }
            statData.cumulativeValue += stat.Value;
        }
        
        
        
        if (progressData.metaStats == null)
        {
            progressData.metaStats = new MetaStatSaveData();
        }

        foreach (WorldObjectType worldObjectType in worldObjectTypes)
        {
            InfraMetaStatSaveData infraStats = progressData.metaStats.infra.Find(i => i.infraId == worldObjectType.GetTypeAsId());
            if (infraStats == null)
            {
               
                infraStats = new InfraMetaStatSaveData() { infraId = worldObjectType.GetTypeAsId() };
                progressData.metaStats.infra.Add(infraStats);
            }

            foreach (KeyValuePair<MetaStat, int> stat in worldObjectType.metaStatCollection.Stats)
            {
                MetaStatData statData = infraStats.stats.Find(s => s.statName == stat.Key.ToString());
                if (statData == null)
                {
                    statData = new MetaStatData()
                    {
                        statName = stat.Key.ToString(),
                        highestValue = 0,
                        cumulativeValue = 0
                    };
                    infraStats.stats.Add(statData);
                }

                if (stat.Value > statData.highestValue)
                {
                    statData.highestValue = stat.Value;
                }
                statData.cumulativeValue += stat.Value;
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
      
        MetaStatData statData = null;
        if (challenge.WorldObjectTypeId != null)
        {
            if (state.metaStats.infra == null)
            {
                throw new SystemException("`state.metaStats.infra` is null");
            }
            InfraMetaStatSaveData prevStats = state.metaStats.infra.Find(i => i.infraId == challenge.WorldObjectTypeId);
            if (prevStats != null)
            {
                statData = prevStats.stats.Find(s => s.statName == challenge.metaStat.ToString());
            }

        }
        else
        {
            statData = state.metaStats.game.Find(s => s.statName == challenge.metaStat.ToString());
        }
      
        if (statData != null)
        {
            value = GetChallengeStatData(statData, challenge);
        }
        // Debug.Log($"{challenge.ChallengeID} - metaStat: {challenge.metaStat} -RequirementType: {challenge.RequirementType} - WorldObjectTypeId: {challenge.WorldObjectTypeId} - {value} >= {challenge.RequiredValue} : {value >= challenge.RequiredValue}");
        // Check if the challenge was incomplete before but is complete now.
        return (value >= challenge.RequiredValue);
    }

    public static int GetChallengeStatData(MetaStatData statData, MetaChallengeBase challenge)
    {
        int value = 0;
        switch (challenge.RequirementType)
        {
            case(MetaChallengeBase.MetaChallengeRequirementType.Cumulative):
                value = statData.cumulativeValue;
                break;
            case(MetaChallengeBase.MetaChallengeRequirementType.Highest):
                value = statData.highestValue;
                break;
            default:
                throw new SystemException($"`challenge.RequirementType` '{challenge.RequirementType}' doesn't exist");
                        
        }

        return value;
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
            MetaChallengeBase existingChallengeBase = prevCompletedChallenges.Find((c) => c.ChallengeID == currChallenge.ChallengeID);
            if(existingChallengeBase == null){
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
                TechnologyID = "application-server",
                DisplayName = "Application Server",
                Description = "",
                ResearchTime = 30,
                UnlockConditions = new List<UnlockCondition>(),
                CurrentState = Technology.State.Locked,
                TutorialStepId = TutorialStepId.Infra_ApplicationServer_Tip
            },
            new Technology()
            {
                TechnologyID = "kanban-board",
                DisplayName = "Kanban",
                Description = "",
                ResearchTime = 60,
                UnlockConditions = new List<UnlockCondition>()
                {
                    new UnlockCondition()
                    {
                        Type = UnlockCondition.ConditionType.Technology, 
                        TechnologyID = "white-board"
                    }
                },
                CurrentState = Technology.State.Locked,
                Direction = Technology.TechTreeDirection.Right,
                TutorialStepId = TutorialStepId.Infra_KanbanBoard_Tip,
            },
            new Technology()
            {
                TechnologyID = "white-board",
                DisplayName = "Software Basics",
                Description = "",
                ResearchTime = 30,
                UnlockConditions = new List<UnlockCondition>() { new UnlockCondition() { Type = UnlockCondition.ConditionType.Technology, TechnologyID = "application-server" } },
                CurrentState = Technology.State.Locked,
                Direction = Technology.TechTreeDirection.Right,
                TutorialStepId = TutorialStepId.Infra_WhiteBoard_Tip
            },
        
            new Technology()
            {
                TechnologyID = "org-chart",
                DisplayName = "Org Chart",
                Description = "",
                ResearchTime = 120,
                UnlockConditions = new List<UnlockCondition>() { new UnlockCondition() { Type = UnlockCondition.ConditionType.Technology, TechnologyID = "kanban-board" } },
                CurrentState = Technology.State.MetaLocked,
                Direction = Technology.TechTreeDirection.Right,
                TutorialStepId = TutorialStepId.Infra_OrgChart_Tip
            },
            new Technology()
            {
                TechnologyID = "product-road-map",
                DisplayName = "Product Road Map",
                Description = "",
                ResearchTime = 30,
                UnlockConditions = new List<UnlockCondition>() { new UnlockCondition() { Type = UnlockCondition.ConditionType.Technology, TechnologyID = "white-board" } },
                CurrentState = Technology.State.Locked,
                Direction = Technology.TechTreeDirection.Right,
                TutorialStepId = TutorialStepId.Infra_ProductRoadMap_Tip
            },
            new Technology()
            {
                TechnologyID = "dedicated-db",
                DisplayName = "Dedicated Database",
                Description = "",
                ResearchTime = 30,
                UnlockConditions = new List<UnlockCondition>() { new UnlockCondition() { Type = UnlockCondition.ConditionType.Technology, TechnologyID = "application-server" } },
                CurrentState = Technology.State.Locked,
                TutorialStepId = TutorialStepId.Infra_DedicatedDB_Tip
                // UnlockConditions - Get and instance to size 2?
            },
            new Technology()
            {
                TechnologyID = "binary-storage",
                DisplayName = "Binary Storage",
                Description = "",
                ResearchTime = 40,
                UnlockConditions = new List<UnlockCondition>() { new UnlockCondition() { Type = UnlockCondition.ConditionType.Technology, TechnologyID = "application-server" } },
                TutorialStepId = TutorialStepId.Infra_BinaryStorage_Tip
            },
            new Technology()
            {
                TechnologyID = "redis",
                DisplayName = "Redis Caching",
                Description = "",
                ResearchTime = 25,
                UnlockConditions = new List<UnlockCondition>() { new UnlockCondition() { Type = UnlockCondition.ConditionType.Technology, TechnologyID = "dedicated-db" } },
                TutorialStepId = TutorialStepId.Infra_Redis_Tip
            },
            new Technology()
            {
                TechnologyID = "cdn",
                DisplayName = "Content Delivery Network(CDN)",
                Description = "",
                ResearchTime = 25,
                UnlockConditions = new List<UnlockCondition>() { new UnlockCondition() { Type = UnlockCondition.ConditionType.Technology, TechnologyID = "binary-storage" } },
                TutorialStepId = TutorialStepId.Infra_CDN_Tip
                // serve up X binary packets with the s3 bucket
            },
            new Technology()
            {
                TechnologyID = "load-balancer",
                DisplayName = "Load Balancer",
                Description = "",
                ResearchTime = 30,
                UnlockConditions = new List<UnlockCondition>() { new UnlockCondition() { Type = UnlockCondition.ConditionType.Technology, TechnologyID = "dedicated-db" }, new UnlockCondition() { Type = UnlockCondition.ConditionType.Technology, TechnologyID = "binary-storage" } },
                TutorialStepId = TutorialStepId.Infra_LoadBalancer_Tip
                // ??? Make it to day y?
            },
            new Technology()
            {
                TechnologyID = "read-replicas",
                DisplayName = "Read Replicas",
                Description = "",
                ResearchTime = 60,
                UnlockConditions = new List<UnlockCondition>() { new UnlockCondition() { Type = UnlockCondition.ConditionType.Technology, TechnologyID = "dedicated-db" } }
                // Scale up your dedicated-db to level 2
            },
            new Technology()
            {
                TechnologyID = "water-cooler",
                DisplayName = "Water Cooler",
                Description = "",
                ResearchTime = 5,
                UnlockConditions = new List<UnlockCondition>() { new UnlockCondition() { Type = UnlockCondition.ConditionType.Technology, TechnologyID = "kanban-board" } },
                Direction = Technology.TechTreeDirection.Right,
                TutorialStepId = TutorialStepId.Infra_WaterCooler_Tip
            },
            new Technology()
            {
                TechnologyID = "waf",
                DisplayName = "Web Application Firewall(WAF)",
                Description = "",
                ResearchTime = 25,
                UnlockConditions = new List<UnlockCondition>() { new UnlockCondition() { Type = UnlockCondition.ConditionType.Technology, TechnologyID = "cloud-watch-metrics" } },
                CurrentState = Technology.State.Locked,
                Direction = Technology.TechTreeDirection.Down,
                TutorialStepId = TutorialStepId.Infra_WAF_Tip
                // Survive X malicious packets
            },
            new Technology()
            {
                TechnologyID = "secret-manager",
                DisplayName = "Secret Manager",
                Description = "",
                ResearchTime = 25,
                UnlockConditions = new List<UnlockCondition>() { new UnlockCondition() { Type = UnlockCondition.ConditionType.Technology, TechnologyID = "application-server" } },
                Direction = Technology.TechTreeDirection.Down,
                TutorialStepId = TutorialStepId.Infra_SecretManager_Tip
                // Cycle credentials Y times
            },
            new Technology()
            {
                TechnologyID = "sqs",
                DisplayName = "Simple Queue Service",
                Description = "",
                ResearchTime = 25,
                UnlockConditions = new List<UnlockCondition>() { new UnlockCondition() { Type = UnlockCondition.ConditionType.Technology, TechnologyID = "load-balancer" } },
                TutorialStepId = TutorialStepId.Infra_SQS_Tip
                // survive Y packets in a single run.
            },
            new Technology()
            {
                TechnologyID = "cognito",
                DisplayName = "Cognito User Pools",
                Description = "",
                ResearchTime = 25,
                UnlockConditions = new List<UnlockCondition>()
                {
                    new UnlockCondition()
                    {
                        Type = UnlockCondition.ConditionType.Technology, 
                        TechnologyID = "application-server"
                    },
                    new  UnlockCondition()
                    {
                        Type = UnlockCondition.ConditionType.TutorialStepState,
                        TutorialStepId = TutorialStepId.Technology_DedicatedDB_Unlocked
                    },
                    new  UnlockCondition()
                    {
                        Type = UnlockCondition.ConditionType.TutorialStepState,
                        TutorialStepId = TutorialStepId.Technology_Whiteboard_Unlocked
                    }
                },
                CurrentState = Technology.State.Locked,
                Direction = Technology.TechTreeDirection.Down,
                TutorialStepId = TutorialStepId.Infra_Cognito_Tip
                // Survive Y user info leaked
            },
            new Technology()
            {
                TechnologyID = "codepipeline",
                DisplayName = "Code Pipeline",
                Description = "",
                ResearchTime = 25,
                UnlockConditions = new List<UnlockCondition>()
                {
                    new UnlockCondition()
                    {
                        Type = UnlockCondition.ConditionType.Technology, 
                        TechnologyID = "application-server"
                    },
                    new  UnlockCondition()
                    {
                        Type = UnlockCondition.ConditionType.TutorialStepState,
                        TutorialStepId = TutorialStepId.Technology_DedicatedDB_Unlocked
                    },
                    new  UnlockCondition()
                    {
                        Type = UnlockCondition.ConditionType.TutorialStepState,
                        TutorialStepId = TutorialStepId.Technology_Whiteboard_Unlocked
                    }
                },
                CurrentState = Technology.State.Locked,
                Direction = Technology.TechTreeDirection.Down,
                TutorialStepId = TutorialStepId.Infra_CodePipeline_Tip
                // Push out Y deployments
            },
            new Technology()
            {
                TechnologyID = "email-service",
                DisplayName = "Email Service",
                Description = "",
                ResearchTime = 200,
                CurrentState = Technology.State.Locked,
                UnlockConditions = new List<UnlockCondition>()
                {
                    new UnlockCondition()
                    {
                        Type = UnlockCondition.ConditionType.Technology, 
                        TechnologyID = "application-server"
                    },
                    new UnlockCondition()
                    {
                        Type = UnlockCondition.ConditionType.SprintGreaterOrEqual, 
                        SprintNumber = 1
                    }
                },
                Direction = Technology.TechTreeDirection.Left,
                TutorialStepId = TutorialStepId.Infra_EmailService_Tip
                // Finish round 1
            },
            new Technology()
            {
                TechnologyID = "cloud-watch-metrics",
                DisplayName = "Cloud Watch Metrics",
                Description = "",
                Direction = Technology.TechTreeDirection.Down,
                ResearchTime = 200,
                UnlockConditions = new List<UnlockCondition>()
                {
                    new UnlockCondition()
                    {
                        Type = UnlockCondition.ConditionType.Technology, 
                        TechnologyID = "application-server"
                    },
                    new  UnlockCondition()
                    {
                        Type = UnlockCondition.ConditionType.TutorialStepState,
                        TutorialStepId = TutorialStepId.Technology_DedicatedDB_Unlocked
                    },
                    new  UnlockCondition()
                    {
                        Type = UnlockCondition.ConditionType.TutorialStepState,
                        TutorialStepId = TutorialStepId.Technology_Whiteboard_Unlocked
                    }
                },
                TutorialStepId = TutorialStepId.Infra_CloudWatchMetrics_Tip
                // ???
            },
            new Technology()
            {
                TechnologyID = "sns",
                DisplayName = "Mobile Notifications",
                Description = "",
                Direction = Technology.TechTreeDirection.Left,
                ResearchTime = 200,
                CurrentState = Technology.State.Locked,
                UnlockConditions = new List<UnlockCondition>()
                {
                    new UnlockCondition()
                    {
                        Type = UnlockCondition.ConditionType.Technology, 
                        TechnologyID = "application-server"
                    },
                    new UnlockCondition()
                    {
                        Type = UnlockCondition.ConditionType.SprintGreaterOrEqual, 
                        SprintNumber = 1
                    }
                },
                TutorialStepId = TutorialStepId.Infra_SNS_Tip
                // Finish round 1
            }
        };
        foreach (Technology technology in technologies)
        {
            technology.OriginalState = technology.CurrentState;
        }
        return technologies;
    }


    public static List<MetaChallengeBase> GetAllChallenges()
    {
        List<MetaChallengeBase> challenges = new List<MetaChallengeBase>()
        {
            
            new MetaChallengeBase()
            {
                ChallengeID = "application-server",
                DisplayName = "My First Server",
                Description = "Unlock and build the Application Server Once",
                metaStat = MetaStat.Infra_Built,
                WorldObjectTypeId = "application-server",
                RequirementType = MetaChallengeBase.MetaChallengeRequirementType.Cumulative,
                RequiredValue = 1,
                Rewards =  new List<RewardBase>()
                {
                    /*new RewardBase()
                    {
                        RewardId = "server1",
                        Type = RewardBase.RewardType.WorldObject_StartsOperational,
                    },*/
                    new TechnologyStartStateReward()
                    {
                        TechnologyId = "application-server",
                        StartState = Technology.State.Unlocked
                    }
                }
            },
            new MetaChallengeBase()
            {
                ChallengeID = "white-board",
                DisplayName = "Junior SWE",
                Description = "Unlock and build the White Board 3 Times",
                metaStat = MetaStat.Infra_Built,
                WorldObjectTypeId = "white-board",
                RequirementType = MetaChallengeBase.MetaChallengeRequirementType.Cumulative,
                RequiredValue = 3,
                Rewards =  new List<RewardBase>()
                {
                    new WorldObjectTypeStartsOperationalReward()
                    {
                        WorldObjectTypeId = "white-board",
                    },
                    new TechnologyStartStateReward()
                    {
                        TechnologyId = "white-board",
                        StartState = Technology.State.Unlocked
                    }
                }
            },
            new MetaChallengeBase()
            {
                ChallengeID = "kanban-board",
                DisplayName = "Beginner Project Management",
                Description = "Unlock and build the Kanban Board 5 Times",
                metaStat = MetaStat.Infra_Built,
                WorldObjectTypeId = "kanban-board",
                RequirementType = MetaChallengeBase.MetaChallengeRequirementType.Cumulative,
                RequiredValue = 5,
                Rewards =  new List<RewardBase>()
                {
                    new WorldObjectTypeStartsOperationalReward()
                    {
                        WorldObjectTypeId = "kanban-board",
                    },
                    new TechnologyStartStateReward()
                    {
                        TechnologyId = "kanban-board",
                        StartState = Technology.State.Unlocked
                    }
                }
            },
            new MetaChallengeBase()
            {
                ChallengeID = "product-road-map",
                DisplayName = "Product Road Map",
                Description = "Unlock and build the Product Road Map 1 Times",
                metaStat = MetaStat.Infra_Built,
                WorldObjectTypeId = "product-road-map",
                RequirementType = MetaChallengeBase.MetaChallengeRequirementType.Cumulative,
                RequiredValue = 1,
                Rewards =  new List<RewardBase>()
                {
                    new WorldObjectTypeStartsOperationalReward()
                    {
                        WorldObjectTypeId = "product-road-map",
                    },
                    new TechnologyStartStateReward()
                    {
                        TechnologyId = "product-road-map",
                        StartState = Technology.State.Unlocked
                    }
                }
            },
            new MetaChallengeBase()
            {
                ChallengeID = "org-chart",
                DisplayName = "Org Chart",
                Description = "Unlock and build the Org Chart 10 Times",
                metaStat = MetaStat.Infra_Built,
                WorldObjectTypeId = "org-chart",
                RequirementType = MetaChallengeBase.MetaChallengeRequirementType.Cumulative,
                RequiredValue = 10,
                Rewards =  new List<RewardBase>()
                {
                    new WorldObjectTypeStartsOperationalReward()
                    {
                        WorldObjectTypeId = "org-chart",
                    },
                    new TechnologyStartStateReward()
                    {
                        TechnologyId = "org-chart",
                        StartState = Technology.State.Unlocked
                    }
                }
            },
            
            new MetaChallengeBase()
            {
                ChallengeID = "binary-storage",
                DisplayName = "Binary Storage",
                Description = "Successfully handle 100 images",
                metaStat = MetaStat.Infra_HandleNetworkPacket,
                WorldObjectTypeId = "application-server",
        
                RequiredValue = 100,
                Rewards =  new List<RewardBase>()
                {
                    new TechnologyStartStateReward()
                    {
                        TechnologyId = "binary-storage",
                    }
                }
            },
            new MetaChallengeBase()
            {
                ChallengeID = "redis",
                DisplayName = "Redis Cache",
                Description = "Have your dedicated database handle 100 text packets",
                metaStat = MetaStat.Infra_HandleNetworkPacket,
                WorldObjectTypeId = "dedicated-db",
                RequiredValue = 100,
                Rewards =  new List<RewardBase>()
                {
                    new TechnologyStartStateReward()
                    {
                        TechnologyId = "redis",
                    }
                }
            },
            /*new MetaChallengeBase()
            {
                ChallengeID = "cash1",
                DisplayName = "Extra Starting Cash 1",
                Description = "Make it to day 5",
                metaStat = MetaStat.Day,
                RewardId = StatType.Money.ToString(),
                RewardValue = 1.5f,
                RequiredValue = 5,
                RewardType = MetaChallengeBase.MetaChallengeRewardType.StartingStatValue,
                RequirementType = MetaChallengeBase.MetaChallengeRequirementType.Highest
            },*/
            new MetaChallengeBase()
            {
                ChallengeID = "cdn",
                DisplayName = "Content Delivery Network(CDN)",
                Description = "Send 200 images to S3 successfully",
                metaStat = MetaStat.Infra_HandleNetworkPacket,
                WorldObjectTypeId = "binary-storage",
      
                RequiredValue = 50,
                Rewards =  new List<RewardBase>()
                {
                    new TechnologyStartStateReward()
                    {
                        TechnologyId = "cdn",
                    }
                }
            },
            new MetaChallengeBase()
            {
                ChallengeID = "load-balancer",
                DisplayName = "Load Balancer",
                Description = "Make it to day 10",
                metaStat = MetaStat.Day,
                RequiredValue = 10,
                RequirementType = MetaChallengeBase.MetaChallengeRequirementType.Highest,
                Rewards =  new List<RewardBase>()
                {
                    new TechnologyStartStateReward()
                    {
                        TechnologyId = "load-balancer",
                    }
                }
            },
            new MetaChallengeBase()
            {
                ChallengeID = "read-replicas",
                DisplayName = "Read Replicas",
                Description = "Upsize your database to level 2",
                metaStat = MetaStat.Infra_MaxSize,
                WorldObjectTypeId = "dedicated-db",
                RequiredValue = 2,
                Rewards =  new List<RewardBase>()
                {
                    new TechnologyStartStateReward()
                        {
                        TechnologyId = "read-replicas",
                    }
                }
            },
            new MetaChallengeBase()
            {
                ChallengeID = "codepipeline",
                DisplayName = "Code Pipeline",
                Description = "Successfully deploy 10 releases in one run",
                metaStat = MetaStat.Deployments,
                RequiredValue = 10,
                RequirementType = MetaChallengeBase.MetaChallengeRequirementType.Highest,
                Rewards =  new List<RewardBase>()
                {
                    new TechnologyStartStateReward()
                    {
                        TechnologyId = "codepipeline",
                    }
                }
            },
            new MetaChallengeBase()
            {
                ChallengeID = "sqs",
                DisplayName = "Simple Queue Service",
                Description = "Successfully handle 200 packets with the load balancer",
                metaStat = MetaStat.Infra_HandleNetworkPacket,
                WorldObjectTypeId = "load-balancer",
            
                RequiredValue = 200,
                RequirementType = MetaChallengeBase.MetaChallengeRequirementType.Cumulative,
                Rewards =  new List<RewardBase>()
                {
                    new TechnologyStartStateReward()
                    {
                        TechnologyId = "sqs",
                    }
                }
            },
            new MetaChallengeBase()
            {
                ChallengeID = "cloud-watch-metrics",
                DisplayName = "Cloud Watch Metrics",
                Description = "Successfully make it to Sprint 2",
                metaStat = MetaStat.Sprint,
                WorldObjectTypeId = "sns",
                RequiredValue = 2,
                RequirementType = MetaChallengeBase.MetaChallengeRequirementType.Highest,
                Rewards =  new List<RewardBase>()
                {
                    new TechnologyStartStateReward()
                    {
                        TechnologyId = "sns",
                    }
                }
            }
        };
        
        return challenges;
    }

    public static RewardBase GetRandomModifier(RewardBase.RewardGroup group)
    {
        List<RewardBase> modifiers = GetModifierByGroup(group);
 
        int i = Random.Range(0, modifiers.Count);
        return modifiers[i];
    }  
    public static List<RewardBase> GetModifierByGroup(RewardBase.RewardGroup group)
    {
        List<RewardBase> modifiers = GetAllModifiers();
        List<RewardBase> foundModifiers = new List<RewardBase>();
        foreach (RewardBase modifier in modifiers)
        {
            if (modifier.Group == group)
            {
                //TODO: Check if Meta Unlocked
                foundModifiers.Add(modifier);
            }
        }

        return foundModifiers;
    }  
    public static List<RewardBase> GetAllModifiers()
    {
        return new List<RewardBase>()
        {
           
            new NPCStatModifierReward()
            {
                Id = "fast_worker",
                Name = "Fast Worker",

                StatType = StatType.NPC_DevOpsSpeed,
                IconSpriteId = "IconHand",
                BaseValue = 1.1f
            },
            new NPCStatModifierReward()
            {
                
                Id = "fast_researcher",
                Name = "Fast Researcher",
    
                StatType = StatType.NPC_ResearchSpeed,
                IconSpriteId = "IconResearch",
                BaseValue = 1.1f
            },
            new NPCStatModifierReward()
            {
                Id = "fast_xp",
                Name = "Fast Learner",
                StatType = StatType.NPC_XPSpeed,
                IconSpriteId = "IconTest",
                BaseValue = 1.1f
            },
            new NPCStatModifierReward()
            {
                Id = "move_speed",
                Name = "Fast Move Speed",
                StatType = StatType.NPC_MovmentSpeed,
                IconSpriteId = "IconMovementSpeed",
                BaseValue = 1.1f
            },
            new NPCStatModifierReward()
            {
                Id = "fast_coder",
                Name = "Fast Coder",
                StatType = StatType.NPC_CodeSpeed,
                IconSpriteId = "IconCode",
                BaseValue = 1.1f
            },
            new NPCStatModifierReward()
            {
                Id = "better_coder",
                Name = "Better Coder",
                StatType = StatType.NPC_CodeQuality,
                BaseValue = 1.1f,
                IconSpriteId = "IconCode",
            },
            new NPCStatModifierReward()
            {
                Id = "better_debugger",
                Name = "Better Debugger",
                StatType = StatType.NPC_AttackDamage,
                BaseValue = 1.5f,
                IconSpriteId = "IconDebug"
            },
            /*
             *
             * MATTS NOTES:
             * Release EFFECTS
             * - Decreased Image Load Costs
             * - Security bonuses
             * - Network Packet Load Costs
             * - A/B testing, Sales Page updates, Shopping cart enhancement, Daily Income Bonus
             * - Disk Space Bonus
             * - Documentation - Makes it easier for new NPCS to learn infra.
             * - Cross Training - Requires 2 NPCS or more - Enhances NPCs knowledge, prevents knowledge silo events.
             */
             new WorldObjectTypeNetworkPacketStatModifierReward()
            {
                Group = RewardBase.RewardGroup.Release,
                Id = "image_optimization",
                Name = "Image Optimization",
                Description = "Decreases CPU Load Of Images",
                StatType = StatType.Infra_LoadPerPacket,
                NetworkPacketType = NetworkPacketData.PType.Image,
                BaseValue = 0.95f,
                WorldObjectType = WorldObjectType.Type.ApplicationServer,
                IconSpriteId = "IconImageOptimization",
                ScaleDirection =  ScaleDirection.Down
            },
            new WorldObjectTypeNetworkPacketStatModifierReward()
            {
                Group = RewardBase.RewardGroup.Release,
                Id = "db_optimization",
                Name = "Relational Database Design",
                Description = "Decreases CPU Load Of Packets On The Dedicated DB",
                StatType = StatType.Infra_LoadPerPacket,
                NetworkPacketType = NetworkPacketData.PType.Text,
                BaseValue = 0.9f,
                WorldObjectType = WorldObjectType.Type.DedicatedDB,
                IconSpriteId = "IconRelationalDBDesign",
                ScaleDirection =  ScaleDirection.Down
            },
            new GlobalNetworkPacketStatModifierReward()
            {
                Group = RewardBase.RewardGroup.Release,
                Id = "sale_page_optimization",
                Name = "Sales Page Optimization",
                Description = "Increases Purchase Probability",
                StatType = StatType.NetworkPacket_Probibility,
                NetworkPacketType = NetworkPacketData.PType.Purchase,
                BaseValue = 1.1f,
                IconSpriteId = "IconCart"
            },
            new GlobalStatModifierReward()
            {
                Group = RewardBase.RewardGroup.Release,
                Id = "input_validation",
                Name = "Input Validation",
                Description = "Decreases Chance Of XSS Attacks",
                StatType = StatType.Infra_InputValidation,
                BaseValue = 0.95f,
                IconSpriteId = "IconCode"
            },
            new GlobalStatBaseValueReward()
            {
                Group = RewardBase.RewardGroup.Release,
                Id = "tech_debt",
                Name = "Tech Debt",
                Description = "Decreases Current Tech Debt",
                StatType = StatType.TechDebt,
                BaseValue = -2.0f,
                // ScaleDirection = ScaleDirection.Down,
                IconSpriteId = "IconTechDebt"
            },
            new GlobalStatBaseValueReward()
            {
                Group = RewardBase.RewardGroup.Release,
                Id = "contract_work",
                Name = "Contract Work",
                Description = "Good for some quick cash",
                StatType = StatType.Money,
                BaseValue = 50f,
                // ScaleDirection = ScaleDirection.Down,
                IconSpriteId = "IconDollar"
            },
            new WorldObjectTypeStatModifierReward()
            {
                Group = RewardBase.RewardGroup.Release,
                Id = "db_indexes",
                Name = "Database Indexes",
                Description = "Increases the CPU Load Where Latency Starts Accumulating On The DB",
                StatType = StatType.Infra_LatencyStartsAtLoad,
                BaseValue =  1.25f,
                WorldObjectType = WorldObjectType.Type.DedicatedDB,
                IconSpriteId = "IconRelationalDBDesign",
                ScaleDirection =  ScaleDirection.Up
            },
            new WorldObjectTypeStatModifierReward()
            {
                Group = RewardBase.RewardGroup.Release,
                Id = "multi_threaded",
                Name = "Multi Threaded",
                Description = "Increases the CPU Load Where Latency Starts Accumulating On The Application Layer",
                StatType = StatType.Infra_LatencyStartsAtLoad,
                BaseValue =  1.25f,
                WorldObjectType = WorldObjectType.Type.ApplicationServer,
                IconSpriteId = "IconCode",
                ScaleDirection =  ScaleDirection.Up
            }
            
            /**
             * Sprint Modifiers
             *
             * Difficulty
                Daily Income
                Research Speed
                Coding Speed
                Distractions? Dumb ass questions that get asked by NPCS like the sales guy.
                Increase tech debt rate, requires more frequent deployments
             */
            
        };
    }
}