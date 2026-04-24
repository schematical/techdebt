using System;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using DefaultNamespace;
using DefaultNamespace.Rewards;
using Infrastructure;
using MetaChallenges;
using NPCs;
using Rewards;
using Stats;
using Tutorial;
using UnityEngine;
using Random = UnityEngine.Random;


public static class MetaGameManager
{
    private static List<MetaChallengeBase> _challenges = new List<MetaChallengeBase>();

    public static int CurrentSlotIndex = 0;
    public static MetaProgressData ProgressData;
    


    public static string GetSavePath(string foldername = null, string filename = "meta_progress.json")
    {
        if (foldername == null)
        {
            foldername = $"techdebt_slot_{CurrentSlotIndex}";
        }
#if !UNITY_EDITOR && UNITY_WEBGL
                var	path = System.IO.Path.Combine("idbfs", foldername);  //	Path: "/idbfs/<foldername>"
#else
        var path = System.IO.Path.Combine(Application.persistentDataPath, foldername);
#endif

        if (!System.IO.Directory.Exists(path))
        {
            //Console.WriteLine("Creating save directory: " + path);
            System.IO.Directory.CreateDirectory(path);
        }

        var result = System.IO.Path.Combine(path, filename); //	File Path: "/idbfs/<foldername>/<filename>"
        return result;
    }

    public static void SaveProgress(MetaProgressData metaProgressData)
    {
        string json = JsonUtility.ToJson(metaProgressData, true);
        File.WriteAllText(GetSavePath(), json);

        // Debug.Log($"Progress saved to {GetSavePath()}");
    }

    public static MetaProgressData GetProgress(bool forceReload = false)
    {
        if (!forceReload && ProgressData != null)
        {
            return ProgressData;
        }
        if (!File.Exists(GetSavePath()))
        {
            ProgressData =  new MetaProgressData();
            return ProgressData;
        }

        string json = File.ReadAllText(GetSavePath());
        ProgressData = JsonUtility.FromJson<MetaProgressData>(json);
        return ProgressData;
    }
    
    public static MetaProgressData LoadProgressFromSaveSlot(int slotIndex)
    {
        string path = GetSavePath(foldername: $"techdebt_slot_{slotIndex}", filename: "meta_progress.json");
        if (!File.Exists(path))
        {
            return null;
        }

        string json = File.ReadAllText(path);
        return JsonUtility.FromJson<MetaProgressData>(json);
    }

    public static void DeleteSlot(int slotIndex)
    {
        string path = GetSavePath(foldername: $"techdebt_slot_{slotIndex}", filename: ""); // Get directory path
        if (Directory.Exists(path))
        {
            Directory.Delete(path, true);
        }
    }


    public static MetaMapLevelData GetLevelDataById(string levelId)
    {
        MetaProgressData data = GetProgress();
        if (data.mapLevelData == null) return null;
        return data.mapLevelData.Find(l => l.levelId == levelId);
    }

    // Optional: Reset progress for testing or new game
    public static void ResetProgress()
    {
        SaveProgress(new MetaProgressData());
    }

    public static MetaProgressData GetUpdatedMetaStats(List<WorldObjectType> worldObjectTypes)
    {
        MetaProgressData progressData = GetProgress();

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
            InfraMetaStatSaveData infraStats =
                progressData.metaStats.infra.Find(i => i.infraId == worldObjectType.GetTypeAsId());
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
            case (MetaChallengeBase.MetaChallengeRequirementType.Cumulative):
                value = statData.cumulativeValue;
                break;
            case (MetaChallengeBase.MetaChallengeRequirementType.Highest):
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
            state = GetProgress();
        }

        List<MetaChallengeBase> completedChallenges = new List<MetaChallengeBase>();
        List<MetaChallengeBase> allChallenges = GetAllChallenges(); // Get challenge definitions

        foreach (var challenge in allChallenges)
        {
            // Calculate the progress from AFTER this run (from the current in-memory data).
            if (IsChallengeCompleted(state, challenge))
            {
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
            MetaChallengeBase existingChallengeBase =
                prevCompletedChallenges.Find((c) => c.ChallengeID == currChallenge.ChallengeID);
            if (existingChallengeBase == null)
            {
                diffChallenges.Add(currChallenge);
            }
        }

        return diffChallenges;
    }

    public static bool IsPrestigePointAllocationLeveledUp(string allocationId, int level = 1)
    {
        MetaProgressData data = GetProgress();
        MetaPrestigePointAllocation allocation = data.prestigePointAllocations.Find(r => r.Id == allocationId);
        if (allocation == null)
        {
            return false;
        }
        return allocation.level >= level;
    }

    public static void UpdatePrestigePointAllocation(string allocationId, int level, int cost)
    {
        MetaProgressData data = GetProgress();
        if (level == 0)
        {
            data.prestigePointAllocations.RemoveAll(r => r.Id == allocationId);
            SaveProgress(data);
            return;
        }
        MetaPrestigePointAllocation allocation = data.prestigePointAllocations.Find(r => r.Id == allocationId);
        if (allocation == null)
        {
            allocation = new MetaPrestigePointAllocation();
            allocation.Id = allocationId;
            data.prestigePointAllocations.Add(allocation);
        }

        allocation.level = level;
        SaveProgress(data);
    }

    public static int GetAllocatedPrestigePointCount()
    {
        int count = 0;
        foreach (MetaPrestigePointAllocatable allocatable in GetPrestigePointAllocatables())
        {
            MetaPrestigePointAllocation allocation = allocatable.GetAllocation();
            if (allocation == null)
            {
                continue;
            }

            for (int i = 0; i < allocation.level; i++)
            {
                count += allocatable.levels[i].cost;
            }
        }

        return count;
    }
    public static void ApplyMetaRewards()
    {
        MetaProgressData data = GetProgress();
        foreach (MetaPrestigePointAllocatable allocatable in GetPrestigePointAllocatables())
        {
            MetaPrestigePointAllocation allocation = allocatable.GetAllocation();
            if (allocation == null)
            {
                continue;
            }
            if (allocatable.reward == null)
            {
                throw new SystemException($"Missing `allocation.reward` for: ${allocatable.Id}");
            }
            if (allocatable.reward is GlobalStatBaseValueReward)
            {
               
                if (allocation == null)
                {
                    throw new SystemException($"Cannot find an allocation for Meta Reward `{allocatable.reward.Id}`");
                }

                (allocatable.reward as GlobalStatBaseValueReward).Level = allocation.level;
            }

           
            allocatable.reward.Apply();
        }
        /*List<RewardBase> rewards = GetModifierByGroup(RewardBase.RewardGroup.Meta).FindAll((reward => reward.IsUnlocked()));
        foreach (RewardBase reward in rewards)
        {
            if (reward is GlobalStatBaseValueReward)
            {
                MetaPrestigePointAllocation allocation = data.prestigePointAllocations.Find((allocation => allocation.Id == reward.Id));
                if (allocation == null)
                {
                    throw new SystemException($"Cannot find an allocation for Meta Reward `{reward.Id}`");
                }

                (reward as GlobalStatBaseValueReward).Level = allocation.level;
            }
           
            reward.Apply();
        }*/
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
                        TargetId = "white-board"
                    },
                    new UnlockCondition()
                    {
                        Type = UnlockCondition.ConditionType.SprintGreaterOrEqual,
                        SprintNumber = 1
                    },
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
                UnlockConditions = new List<UnlockCondition>()
                {
                    new UnlockCondition()
                    {
                        Type = UnlockCondition.ConditionType.Technology,
                        TargetId = "application-server"
                    }
                },
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
                UnlockConditions = new List<UnlockCondition>()
                {
                    new UnlockCondition()
                    {
                        Type = UnlockCondition.ConditionType.Technology,
                        TargetId = "product-road-map"
                    }
                },
                CurrentState = Technology.State.Locked, // TODO: Meta lock this?
                Direction = Technology.TechTreeDirection.Right,
                TutorialStepId = TutorialStepId.Infra_OrgChart_Tip
            },
            new Technology()
            {
                TechnologyID = "product-road-map",
                DisplayName = "Product Road Map",
                Description = "Allows you to plan what you want to work on next sprint.",
                ResearchTime = 30,
                UnlockConditions = new List<UnlockCondition>()
                {
                    new UnlockCondition()
                    {
                        Type = UnlockCondition.ConditionType.Technology,
                        TargetId = "white-board"
                    },
                    new UnlockCondition()
                    {
                        Type = UnlockCondition.ConditionType.TutorialStepState,
                        TutorialStepId = TutorialStepId.Technology_DedicatedDB_Unlocked
                    },
                },
                CurrentState = Technology.State.Locked,
                Direction = Technology.TechTreeDirection.Right,
                TutorialStepId = TutorialStepId.Infra_ProductRoadMap_Tip
            },
            new Technology()
            {
                TechnologyID = "application-server-size-medium",
                DisplayName = "Application Server - Medium",
                Description = "2x your Application Server's CPU/RAM and Costs",
                ResearchTime = 15,
                UnlockConditions = new List<UnlockCondition>()
                {
                    new UnlockCondition()
                    {
                        Type = UnlockCondition.ConditionType.Technology,
                        TargetId = "application-server"
                    },
                    new UnlockCondition()
                    {
                        Type = UnlockCondition.ConditionType.TutorialStepState,
                        TutorialStepId = TutorialStepId.Technology_Whiteboard_Unlocked
                    },
                },
                CurrentState = Technology.State.Locked,
                // UnlockConditions - Get and instance to size 2?
            },
            new Technology()
            {
                TechnologyID = "application-server-size-large",
                DisplayName = "Application Server - Large",
                Description = "4x your Application Server's CPU/RAM and Costs",
                ResearchTime = 30,
                UnlockConditions = new List<UnlockCondition>()
                {
                    new UnlockCondition()
                    {
                        Type = UnlockCondition.ConditionType.Technology,
                        TargetId = "application-server-size-medium"
                    },
                    new UnlockCondition()
                    {
                        Type = UnlockCondition.ConditionType.TutorialStepState,
                        TutorialStepId = TutorialStepId.Technology_DedicatedDB_Unlocked
                    },
                },
                CurrentState = Technology.State.Locked,
                // UnlockConditions - Get and instance to size 2?
            },
            new Technology()
            {
                TechnologyID = "dedicated-db",
                DisplayName = "Dedicated Database",
                Description = "Takes load off the Application Server by moving the database to its own dedicated hardware.",
                ResearchTime = 30,
                UnlockConditions = new List<UnlockCondition>()
                {
                    new UnlockCondition()
                    {
                        Type = UnlockCondition.ConditionType.Technology,
                        TargetId = "application-server"
                    },
                    new UnlockCondition()
                    {
                        Type = UnlockCondition.ConditionType.TutorialStepState,
                        TutorialStepId = TutorialStepId.Technology_ApplicationServerSizeMedium_Unlocked
                    },
                },
                CurrentState = Technology.State.Locked,
                TutorialStepId = TutorialStepId.Infra_DedicatedDB_Tip
                // UnlockConditions - Get and instance to size 2?
            },
            new Technology()
            {
                TechnologyID = "binary-storage",
                DisplayName = "Binary Storage",
                Description = "A data store optimized for large binary objects like images.",
                ResearchTime = 40,
                UnlockConditions = new List<UnlockCondition>()
                {
                    new UnlockCondition()
                        { Type = UnlockCondition.ConditionType.Technology, TargetId = "application-server" },
                    new UnlockCondition()
                    {
                        Type = UnlockCondition.ConditionType.TutorialStepState,
                        TutorialStepId = TutorialStepId.Technology_DedicatedDB_Unlocked
                    },
                },
                TutorialStepId = TutorialStepId.Infra_BinaryStorage_Tip
            },
            new Technology()
            {
                TechnologyID = "redis",
                DisplayName = "Caching",
                Description = "A lightning fast value key store. Allows you to decrease load on the application server by caching complex operations.",
                ResearchTime = 25,
                UnlockConditions = new List<UnlockCondition>()
                {
                    new UnlockCondition()
                        { Type = UnlockCondition.ConditionType.Technology, TargetId = "dedicated-db" }
                },
                TutorialStepId = TutorialStepId.Infra_Redis_Tip
            },
            new Technology()
            {
                TechnologyID = "cdn",
                DisplayName = "Content Delivery Network(CDN)",
                Description = "Serves up binary files fast taking all the load off of the Application Server.",
                ResearchTime = 25,
                UnlockConditions = new List<UnlockCondition>()
                {
                    new UnlockCondition()
                        { Type = UnlockCondition.ConditionType.Technology, TargetId = "binary-storage" }
                },
                TutorialStepId = TutorialStepId.Infra_CDN_Tip
                // serve up X binary packets with the s3 bucket
            },
            new Technology()
            {
                TechnologyID = "load-balancer",
                DisplayName = "Load Balancer",
                Description = "Distributes load among multiple Application Servers.",
                ResearchTime = 30,
                UnlockConditions = new List<UnlockCondition>()
                {
                    new UnlockCondition()
                        { Type = UnlockCondition.ConditionType.Technology, TargetId = "dedicated-db" },
                    new UnlockCondition()
                        { Type = UnlockCondition.ConditionType.Technology, TargetId = "binary-storage" }
                },
                TutorialStepId = TutorialStepId.Infra_LoadBalancer_Tip
                // ??? Make it to day y?
            },
            new Technology()
            {
                TechnologyID = "read-replicas",
                DisplayName = "Read Replicas",
                Description = "Takes load off the main 'Writer' database by distributing it among read replica database servers.",
                ResearchTime = 60,
                UnlockConditions = new List<UnlockCondition>()
                {
                    new UnlockCondition()
                        { Type = UnlockCondition.ConditionType.Technology, TargetId = "dedicated-db" }
                }
                // Scale up your dedicated-db to level 2
            },
            new Technology()
            {
                TechnologyID = "water-cooler",
                DisplayName = "Water Cooler",
                Description = "I don't have anything for this one yet.",
                ResearchTime = 5,
                UnlockConditions = new List<UnlockCondition>()
                {
                    new UnlockCondition()
                    {
                        Type = UnlockCondition.ConditionType.Technology, 
                        TargetId = "kanban-board"
                    }
                },
                Direction = Technology.TechTreeDirection.Right,
                TutorialStepId = TutorialStepId.Infra_WaterCooler_Tip
            },
            new Technology()
            {
                TechnologyID = "waf",
                DisplayName = "Web Application Firewall(WAF)",
                Description = "Filters out bad traffic.",
                ResearchTime = 25,
                UnlockConditions = new List<UnlockCondition>()
                {
                    new UnlockCondition()
                    {
                        Type = UnlockCondition.ConditionType.Technology,
                        TargetId = "cloud-watch-metrics"
                    }
                },
                CurrentState = Technology.State.Locked,
                Direction = Technology.TechTreeDirection.Down,
                TutorialStepId = TutorialStepId.Infra_WAF_Tip
                // Survive X malicious packets
            },
            new Technology()
            {
                TechnologyID = "secret-manager",
                DisplayName = "Secret Manager",
                Description = "Decreases the chance you will leak sensitive credentials to malicious parties.",
                ResearchTime = 25,
                UnlockConditions = new List<UnlockCondition>()
                {
                    new UnlockCondition()
                    {
                        Type = UnlockCondition.ConditionType.Technology, 
                        TargetId = "cloud-watch-metrics"
                    }
                },
                Direction = Technology.TechTreeDirection.Down,
                TutorialStepId = TutorialStepId.Infra_SecretManager_Tip
                // Cycle credentials Y times
            },
            new Technology()
            {
                TechnologyID = "sqs",
                DisplayName = "Simple Queue Service",
                Description = "Takes a lot of CPU load off the application server by queueing up complex operations to be processed by worker servers.",
                ResearchTime = 25,
                UnlockConditions = new List<UnlockCondition>()
                {
                    new UnlockCondition()
                        { Type = UnlockCondition.ConditionType.Technology, TargetId = "load-balancer" }
                },
                TutorialStepId = TutorialStepId.Infra_SQS_Tip
                // survive Y packets in a single run.
            },
            new Technology()
            {
                TechnologyID = "cognito",
                DisplayName = "Authentication Service",
                Description = "Further prevents PII leaks.",
                ResearchTime = 25,
                UnlockConditions = new List<UnlockCondition>()
                {
                    new UnlockCondition()
                    {
                        Type = UnlockCondition.ConditionType.Technology,
                        TargetId = "cloud-watch-metrics"
                    },
                    new UnlockCondition()
                    {
                        Type = UnlockCondition.ConditionType.TutorialStepState,
                        TutorialStepId = TutorialStepId.Technology_DedicatedDB_Unlocked
                    },
                    new UnlockCondition()
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
                Description = "Automated deployments saving you and your team time deploying software releases.",
                ResearchTime = 25,
                UnlockConditions = new List<UnlockCondition>()
                {
                    new UnlockCondition()
                    {
                        Type = UnlockCondition.ConditionType.Technology,
                        TargetId = "application-server"
                    },
                    new UnlockCondition()
                    {
                        Type = UnlockCondition.ConditionType.TutorialStepState,
                        TutorialStepId = TutorialStepId.Technology_DedicatedDB_Unlocked
                    },
                    new UnlockCondition()
                    {
                        Type = UnlockCondition.ConditionType.TutorialStepState,
                        TutorialStepId = TutorialStepId.Technology_Whiteboard_Unlocked
                    }
                },
                CurrentState = Technology.State.Locked,
                Direction = Technology.TechTreeDirection.Down,
                TutorialStepId = TutorialStepId.Infra_CodePipeline_Tip
            },
            new Technology()
            {
                TechnologyID = "email-service",
                DisplayName = "Email Service",
                Description = "Sends emails at scale.",
                ResearchTime = 200,
                CurrentState = Technology.State.Locked,
                UnlockConditions = new List<UnlockCondition>()
                {
                    new UnlockCondition()
                    {
                        Type = UnlockCondition.ConditionType.Technology,
                        TargetId = "application-server"
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
                DisplayName = "Metrics",
                Description = "Gives you faster better insights into the health of your server infrastructure.",
                Direction = Technology.TechTreeDirection.Down,
                ResearchTime = 120,
                CurrentState = Technology.State.Locked,
                UnlockConditions = new List<UnlockCondition>()
                {
                    new UnlockCondition()
                    {
                        Type = UnlockCondition.ConditionType.Technology,
                        TargetId = "application-server"
                    },
                    new UnlockCondition()
                    {
                        Type = UnlockCondition.ConditionType.TutorialStepState,
                        TutorialStepId = TutorialStepId.Technology_DedicatedDB_Unlocked
                    },
                    new UnlockCondition()
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
                Description = "Sends mobile phone notifications at scale.",
                Direction = Technology.TechTreeDirection.Left,
                ResearchTime = 200,
                CurrentState = Technology.State.Locked,
                UnlockConditions = new List<UnlockCondition>()
                {
                    new UnlockCondition()
                    {
                        Type = UnlockCondition.ConditionType.Technology,
                        TargetId = "application-server"
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
            // TODO: Add 'Local Dev Env' and 'Version Control' as researchable tech.
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
            /* new MetaChallengeBase()
             {
                 ChallengeID = "application-server",
                 DisplayName = "My First Server",
                 Description = "Unlock and build the Application Server Once",
                 metaStat = MetaStat.Infra_Built,
                 WorldObjectTypeId = "application-server",
                 RequirementType = MetaChallengeBase.MetaChallengeRequirementType.Cumulative,
                 RequiredValue = 1,
                 Rewards = new List<RewardBase>()
                 {
                     /*new RewardBase()
                     {
                         AllocationId = "server1",
                         Type = RewardBase.RewardType.WorldObject_StartsOperational,
                     },#1#
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
                 Rewards = new List<RewardBase>()
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
                 Rewards = new List<RewardBase>()
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
             },*/
            /*new MetaChallengeBase()
            {
                ChallengeID = "product-road-map",
                DisplayName = "Product Road Map",
                Description = "Unlock and build the Product Road Map 1 Times",
                metaStat = MetaStat.Infra_Built,
                WorldObjectTypeId = "product-road-map",
                RequirementType = MetaChallengeBase.MetaChallengeRequirementType.Cumulative,
                RequiredValue = 1,
                Rewards = new List<RewardBase>()
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
            },*/
            /*new MetaChallengeBase()
            {
                ChallengeID = "org-chart",
                DisplayName = "Org Chart",
                Description = "Unlock and build the Org Chart 10 Times",
                metaStat = MetaStat.Infra_Built,
                WorldObjectTypeId = "org-chart",
                RequirementType = MetaChallengeBase.MetaChallengeRequirementType.Cumulative,
                RequiredValue = 10,
                Rewards = new List<RewardBase>()
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
            },*/

            new MetaChallengeBase()
            {
                ChallengeID = "binary-storage",
                DisplayName = "Binary Storage",
                Description = "Successfully handle 100 images",
                metaStat = MetaStat.Infra_HandleNetworkPacket,
                WorldObjectTypeId = "application-server",

                RequiredValue = 100,
                Rewards = new List<RewardBase>()
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
                Rewards = new List<RewardBase>()
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
                AllocationId = StatType.Money.ToString(),
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
                Rewards = new List<RewardBase>()
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
                Rewards = new List<RewardBase>()
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
                Rewards = new List<RewardBase>()
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
                Rewards = new List<RewardBase>()
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
                Rewards = new List<RewardBase>()
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
                DisplayName = "Metrics",
                Description = "Successfully make it to Sprint 2",
                metaStat = MetaStat.Sprint,
                RequiredValue = 2,
                RequirementType = MetaChallengeBase.MetaChallengeRequirementType.Highest,
                Rewards = new List<RewardBase>()
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
                Description = "Spins up and resizes infrastructure faster",
                StatType = StatType.NPC_DevOpsSpeed,
                IconSpriteId = "IconHand"
            },
            new NPCStatModifierReward()
            {
                Id = "fast_researcher",
                Name = "Fast Researcher",
                Description = "Researches new technology faster",
                StatType = StatType.NPC_ResearchSpeed,
                IconSpriteId = "IconResearch"
            },
            new NPCStatModifierReward()
            {
                Id = "fast_xp",
                Name = "Fast Learner",
                Description = "Levels up faster",
                StatType = StatType.NPC_XPSpeed,
                IconSpriteId = "IconTest"
            },
            new NPCStatModifierReward()
            {
                Id = "move_speed",
                Name = "Fast Move Speed",
                Description = "Moves faster",
                StatType = StatType.NPC_MovmentSpeed,
                IconSpriteId = "IconMovementSpeed"
            },
            new NPCStatModifierReward()
            {
                Id = "fast_coder",
                Name = "Fast Coder",
                Description = "Codes releases faster",
                StatType = StatType.NPC_CodeSpeed,
                IconSpriteId = "IconCode"
            },
            new NPCStatModifierReward()
            {
                Id = "better_coder",
                Name = "Better Coder",
                Description = "Increases the rarity of rewards from software releases",
                StatType = StatType.NPC_CodeQuality,
                IconSpriteId = "IconCode",
            },
            new NPCStatModifierReward()
            {
                Id = "better_debugger",
                Name = "Better Debugger",
                Description = "Debugs bugs and cyber attacks faster",
                StatType = StatType.NPC_AttackDamage,
                IconSpriteId = "IconDebug"
            },
            new NPCStatModifierReward()
            {
                Id = "cool_under_pressure",
                Name = "Cool Under Pressure",
                Description = "Fixes frozen infrastructure faster",
                StatType = StatType.NPC_FixSpeed,
                IconSpriteId = "IconDebug"
            },
            new NPCStatModifierReward()
            {
                Id = "pays_down_debt",
                Name = "Pays Down Debt",
                StatType = StatType.NPC_Release_TechDebt,
                Description = "Deploying releases coded by this team member also decreases Tech Debt slightly.",
                IconSpriteId = "IconTechDebt",
                ScaleDirection = ScaleDirection.Down
            },
            // TODO: Multi Tasker - Reduces Time spent when switching tasks but decreases quality
            // TODO: Headphones: Big progress/speed penalty for switching tasks but increased quality/speed when working 
            // TODO: Heads Down - Cannot switch tasks but quality and speed is increased
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
            //TODO: Add a perk that automatically decreases tech debt on release
            // Proboablly do the same thing with cash
            new GlobalStatModifierReward()
            {
                Group = RewardBase.RewardGroup.Release,
                Id = "code_linter",
                Name = "Code Linter",
                Description = "Increases the code quality of future releases",
                StatType = StatType.Release_Quality_Multiplier,
                IconSpriteId = "IconCode",
                ScaleDirection = ScaleDirection.Up
            },
            new WorldObjectTypeNetworkPacketStatModifierReward()
            {
                Group = RewardBase.RewardGroup.Release,
                Id = "image_optimization",
                Name = "Image Optimization",
                Description = "Decreases CPU Load Of Images",
                StatType = StatType.Infra_LoadPerPacket,
                NetworkPacketType = NetworkPacketData.PType.Image,
                WorldObjectType = WorldObjectType.Type.ApplicationServer,
                IconSpriteId = "IconImageOptimization",
                ScaleDirection = ScaleDirection.Down
            },
            new WorldObjectTypeNetworkPacketStatModifierReward()
            {
                Group = RewardBase.RewardGroup.Release,
                Id = "db_optimization",
                Name = "Relational Database Design",
                Description = "Decreases CPU Load Of Packets On The Dedicated DB",
                StatType = StatType.Infra_LoadPerPacket,
                NetworkPacketType = NetworkPacketData.PType.Text,
                WorldObjectType = WorldObjectType.Type.DedicatedDB,
                IconSpriteId = "IconRelationalDBDesign",
                ScaleDirection = ScaleDirection.Down
            },
            new WorldObjectTypeNetworkPacketStatModifierReward()
            {
                Group = RewardBase.RewardGroup.Release,
                Id = "code_optimization",
                Name = "Code Optimization",
                Description = "Decreases CPU Load Of Text Packets On The Application Server",
                StatType = StatType.Infra_LoadPerPacket,
                NetworkPacketType = NetworkPacketData.PType.Text,
                WorldObjectType = WorldObjectType.Type.ApplicationServer,
                IconSpriteId = "IconRelationalDBDesign",
                ScaleDirection = ScaleDirection.Down
            },
            new GlobalNetworkPacketStatModifierReward()
            {
                Group = RewardBase.RewardGroup.Release,
                Id = "sale_page_optimization",
                Name = "Sales Page Optimization",
                Description = "Increases Purchase Probability",
                StatType = StatType.NetworkPacket_Probibility,
                NetworkPacketType = NetworkPacketData.PType.Purchase,
                IconSpriteId = "IconCart"
            },
            new GlobalStatModifierReward()
            {
                Group = RewardBase.RewardGroup.Release,
                Id = "input_validation",
                Name = "Input Validation",
                Description = "Decreases Chance Of XSS Attacks",
                StatType = StatType.Infra_InputValidation,
                IconSpriteId = "IconCode"
            },
            new GlobalStatBaseValueReward()
            {
                Group = RewardBase.RewardGroup.Release,
                Id = "tech_debt",
                Name = "Tech Debt",
                Description = "Decreases Current Tech Debt",
                StatType = StatType.TechDebt,
                LevelValues = new List<float>() {-2.0f }, 
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
                LevelValues = new List<float>() { 50f }, 
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
                WorldObjectType = WorldObjectType.Type.DedicatedDB,
                IconSpriteId = "IconRelationalDBDesign",
                ScaleDirection = ScaleDirection.Up
            },
            new WorldObjectTypeStatModifierReward()
            {
                Group = RewardBase.RewardGroup.Release,
                Id = "multi_threaded",
                Name = "Multi Threaded",
                Description = "Increases the CPU Load Where Latency Starts Accumulating On The Application Layer",
                StatType = StatType.Infra_LatencyStartsAtLoad,
                WorldObjectType = WorldObjectType.Type.ApplicationServer,
                IconSpriteId = "IconCode",
                ScaleDirection = ScaleDirection.Up
            },
            new GlobalStatModifierReward()
            {
                Group = RewardBase.RewardGroup.Release,
                Id = "local_dev_env",
                Name = "Local Dev Environment",
                Description = "By allowing your team to write code and run it on their laptops you can speed up the time it takes to get a release out",
                StatType = StatType.Global_CodeSpeed,
                IconSpriteId = "IconCode",
                ScaleDirection = ScaleDirection.Up
            },
            new GlobalStatModifierReward()
            {
                Group = RewardBase.RewardGroup.Release,
                Id = "hashed_passwords",
                Name = "Hashed Passwords",
                Description = "Hashing passwords decreases the cost of losing P.I.I. because the passwords cannot be decrypted",
                StatType = StatType.Global_PIILossCost,
                IconSpriteId = "IconCode",
                ScaleDirection = ScaleDirection.Down
            },
            
            new GlobalStatModifierReward()
            {
                Group = RewardBase.RewardGroup.Release,
                Id = "base_container_images",
                Name = "Base Container Images",
                Description = "Maintaining a base container image prevents you from rebuilding images from needing to be rebuilt from scratch with every deploy. This means much faster deployments.",
                StatType = StatType.Global_DeploymentSpeed,
                IconSpriteId = "IconCode",
                ScaleDirection = ScaleDirection.Up
            },
            
          
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

    public static List<MetaPrestigePointAllocatable> GetPrestigePointAllocatables()
    {
        List<MetaPrestigePointAllocatable> list = new List<MetaPrestigePointAllocatable>()
        {
            new MetaPrestigePointAllocatable()
            {
                Id = "banish",
                levels = new List<MetaPrestigePointAllocatableLevel>()
                {
                    new MetaPrestigePointAllocatableLevel() { cost = 1 },
                    new MetaPrestigePointAllocatableLevel() { cost = 2 },
                    new MetaPrestigePointAllocatableLevel() { cost = 3 },
                    new MetaPrestigePointAllocatableLevel() { cost = 4 },
                    new MetaPrestigePointAllocatableLevel() { cost = 5 }
                },
                reward = new GlobalStatBaseValueReward()
                {
                    Group = RewardBase.RewardGroup.Meta,
                    Id = "banish",
                    Name = "Banishes",
                    Description = "Allows you to banish rewards",
                    StatType = StatType.Global_Banish,
                    IconSpriteId = "IconTechDebt",
                    LevelValues = new List<float>() { 1, 2, 3, 4, 5, 6 }
                },
            },
            new MetaPrestigePointAllocatable()
            {
                Id = "reroll",
                levels = new List<MetaPrestigePointAllocatableLevel>()
                {
                    new MetaPrestigePointAllocatableLevel() { cost = 1 },
                    new MetaPrestigePointAllocatableLevel() { cost = 2 },
                    new MetaPrestigePointAllocatableLevel() { cost = 3 },
                    new MetaPrestigePointAllocatableLevel() { cost = 4 },
                    new MetaPrestigePointAllocatableLevel() { cost = 5 }
                },
                reward = new GlobalStatBaseValueReward()
                {
                    Group = RewardBase.RewardGroup.Meta,
                    Id = "rerolls",
                    Name = "Rerolls",
                    Description = "Allows you to reroll rewards",
                    StatType = StatType.Global_ReRolls,
                    IconSpriteId = "IconTechDebt",
                    LevelValues = new List<float>() { 1, 2, 3, 4, 5, 6 }
                },
            },
            new MetaPrestigePointAllocatable()
            {
                Id = "money",
                levels = new List<MetaPrestigePointAllocatableLevel>()
                {
                    new MetaPrestigePointAllocatableLevel() { cost = 1 },
                    new MetaPrestigePointAllocatableLevel() { cost = 2 },
                    new MetaPrestigePointAllocatableLevel() { cost = 3 },
                    new MetaPrestigePointAllocatableLevel() { cost = 4 },
                    new MetaPrestigePointAllocatableLevel() { cost = 5 },
                    new MetaPrestigePointAllocatableLevel() { cost = 6 }
                },
                reward = new GlobalStatBaseValueReward()
                {
                    Group = RewardBase.RewardGroup.Meta,
                    Id = "money",
                    Name = "Budget Bonus",
                    Description = "Start each run with additional budget",
                    StatType = StatType.Money,
                    IconSpriteId = "IconDollar",
                    LevelValues = new List<float>() { 50, 100, 200, 400, 800, 1600 }
                }
            },
            new MetaPrestigePointAllocatable()
            {
                Id = "training-program",
                levels = new List<MetaPrestigePointAllocatableLevel>()
                {
                    new MetaPrestigePointAllocatableLevel() { cost = 1 },
                    new MetaPrestigePointAllocatableLevel() { cost = 2 },
                    new MetaPrestigePointAllocatableLevel() { cost = 3 },
                    new MetaPrestigePointAllocatableLevel() { cost = 4 },
                    new MetaPrestigePointAllocatableLevel() { cost = 5 },
                    new MetaPrestigePointAllocatableLevel() { cost = 6 }
                },
                reward = new GlobalStatBaseValueReward()
                {
                    Group = RewardBase.RewardGroup.Meta,
                    Id = "training-program",
                    Name = "Training Program",
                    Description = "Increases the rarity of your Team Members Level Ups",
                    StatType = StatType.Money,
                    IconSpriteId = "IconDollar",
                    LevelValues = new List<float>() { 1.1f, 1.15f, 1.2f, 1.25f, 1.3f, 1.4f, 1.5f }
                }
            },
            new MetaPrestigePointAllocatable()
            {
                Id = "OrgChart_Marketing",
                levels = new List<MetaPrestigePointAllocatableLevel>()
                {
                    new MetaPrestigePointAllocatableLevel() { cost = 1 },
                    new MetaPrestigePointAllocatableLevel() { cost = 2 },
                    new MetaPrestigePointAllocatableLevel() { cost = 4 }
                }
            },
            new MetaPrestigePointAllocatable()
            {
                Id = "OrgChart_Technology",
                levels = new List<MetaPrestigePointAllocatableLevel>()
                {
                    new MetaPrestigePointAllocatableLevel() { cost = 1 },
                    new MetaPrestigePointAllocatableLevel() { cost = 3 },
                    new MetaPrestigePointAllocatableLevel() { cost = 5 }
                }
            },
            new MetaPrestigePointAllocatable()
            {
                Id = "OrgChart_Finance",
                levels = new List<MetaPrestigePointAllocatableLevel>()
                {
                    new MetaPrestigePointAllocatableLevel() { cost = 1 },
                    new MetaPrestigePointAllocatableLevel() { cost = 2 },
                    new MetaPrestigePointAllocatableLevel() { cost = 4 }
                }
            },
            new MetaPrestigePointAllocatable()
            {
                Id = "OrgChart_Security",
                levels = new List<MetaPrestigePointAllocatableLevel>()
                {
                    new MetaPrestigePointAllocatableLevel() { cost = 1 },
                    new MetaPrestigePointAllocatableLevel() { cost = 3 },
                    new MetaPrestigePointAllocatableLevel() { cost = 5 }
                }
            }
        };

        foreach (Technology tech in GetAllTechnologies())
        {
            list.Add(new MetaPrestigePointAllocatable()
            {
                Id = tech.TechnologyID,
                levels = new List<MetaPrestigePointAllocatableLevel>()
                {
                    new MetaPrestigePointAllocatableLevel() { cost = Mathf.CeilToInt(tech.ResearchTime / 30f) }
                },
                reward = new TechnologyStartStateReward()
                {
                    TechnologyId =  tech.TechnologyID,
                    StartState = Technology.State.Unlocked
                }
            });
        }

        return list;
    }

    public static void SetCurrentSaveSlot(int index)
    {
        CurrentSlotIndex = index;
        ProgressData = null;
    }
}