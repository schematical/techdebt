using System.Collections.Generic;
using System.IO;
using DefaultNamespace;
using Infrastructure;
using NPCs;
using Tutorial;
using Tutorial.Steps;
using UnityEngine;

namespace Tutorial
{
    public class TutorialManager
    {
        public enum TutorialManagerState
        {
            Active,
            Inactive,
        }

        public TutorialManagerState State { get; protected set; } = TutorialManagerState.Active;
        protected Dictionary<TutorialStepId, TutorialStep> Steps = new Dictionary<TutorialStepId, TutorialStep>();

        public TutorialManager()
        {
            List<TutorialStep> steps = new List<TutorialStep>()
            {
                new FirstTutorialStep(
                    TutorialStepId.NPC_Boss,
                    "CEO",
                    "Hello! Welcome to the team. Your job is to keep the servers up and running fast so our startup can grow and make a profit. "
                )
                {
                    getTargetTranform = () =>
                    {
                        NPCBase npc = GameManager.Instance.AllNpcs.Find((npc) => npc.GetComponent<BossNPC>() != null);
                        return npc.transform;
                    },
                    spriteId = "Suit1NPC",
                    NextStepId = TutorialStepId.Infra_Door
                },
                new TutorialStep(
                    TutorialStepId.Infra_Door,
                    "Door",
                    "The team will enter via this door at the beginning of the day and exit at the end of the day. Click 'Start Day' to start your day"
                )
                {
                    getTargetTranform = () =>
                    {
                        WorldObjectBase door =
                            GameManager.Instance.GetInfrastructureInstanceByID("door");
                        return door.transform;
                    },
                    spriteId = "Suit1NPC",
                }, 
                new TutorialStep(
                    TutorialStepId.Day_Start,
                    "Your Team",
                    "You are in charge of a small team of software engineers. Here is one now."
                )
                {
                    getTargetTranform = () =>
                    {
                        NPCBase npc = GameManager.Instance.AllNpcs.Find((npc) => npc.GetComponent<NPCDevOps>() != null);
                        return npc.transform;
                    },
                    spriteId = "Suit1NPC",
                    NextStepId = TutorialStepId.NPC_PreConsultant
                },
                new TutorialStep(
                    TutorialStepId.NPC_PreConsultant,
                    "Help",
                    "If you need help we hired a consultant to guide you. Allow me to introduce you to..."
                )
                {
                    spriteId = "Suit1NPC",
                    NextStepId = TutorialStepId.NPC_Consultant
                },
                new TutorialStep(
                    TutorialStepId.NPC_Consultant,
                    "Schematical Bot",
                    "Hi! I am the consultant from Schematical and I am here to help guide you as you setup your cloud infrastructure."
                )
                {
                    getTargetTranform = () =>
                    {
                        NPCBase npc = GameManager.Instance.AllNpcs.Find((npc) => npc.GetComponent<NPCSchematicalBot>() != null);
                        return npc.transform;
                    },
                    spriteId = "SchematicalBot",
                    NextStepId = TutorialStepId.Infra_Desk
                },
                new TutorialStep(
                    TutorialStepId.Infra_Desk,
                    "Desk",
                    "Click on the Desk to assign your team members to do research tasks."
                )
                {
                    getTargetTranform = () =>
                    {
                        InfrastructureInstance infrastructureInstance =
                            GameManager.Instance.GetInfrastructureInstanceByID("desk");
                        infrastructureInstance.ShowAttentionIcon();
                        return infrastructureInstance.transform;
                    },
                    spriteId = "SchematicalBot",
                    
                },
                new TutorialStep(
                    TutorialStepId.Technology_ApplicationServer_Researching,
                    "Speed Up Time",
                    "To speed things up use the controls in the lower right hand side of the screen to manipulate in game time."
                )
                {
                    spriteId = "SchematicalBot",
                }, 
                new TutorialStep(
                    TutorialStepId.Technology_ApplicationServer_Unlocked,
                    "Technology Unlocked",
                    "This is the Application Server. " + 
                    "Let's start by building a server so you can start handling some internet traffic. " + 
                    "Do this by clicking on the server then selecting 'Build'. " + 
                    "One of your Engineers will start building it shortly."
                )
                {
                    getTargetTranform = () =>
                    {
                        InfrastructureInstance infrastructureInstance =
                            GameManager.Instance.GetInfrastructureInstanceByID("server1");
                        infrastructureInstance.ShowAttentionIcon();
                        return infrastructureInstance.transform;
                    },
                    spriteId = "SchematicalBot",
                    
                },
                new TutorialStep(
                    TutorialStepId.Infra_ApplicationServer_Planned,
                    "Building",
                    "Now one of your team members will get to work spinning up your application server so it can handle incoming traffic."
                )
                {
                    spriteId = "SchematicalBot",
                }, 
                new TutorialStep(
                    TutorialStepId.Infra_ApplicationServer_Operational,
                    "Server Built",
                    "This is the Application Server. " + 
                    "It will receive Network Packets coming from the internet, process them, and send back a response to whoever sent the request on the internet."
                )
                {
                    getTargetTranform = () =>
                    {
                        InfrastructureInstance infrastructureInstance =
                            GameManager.Instance.GetInfrastructureInstanceByID("server1");
                        return infrastructureInstance.transform;
                    },
                    spriteId = "SchematicalBot",
                    NextStepId = TutorialStepId.Infra_InternetPipe
                    
                },
                new TutorialStep(
                    TutorialStepId.Infra_InternetPipe,
                    "The Internetz",
                    "Notice Network Packets flowing in from the Internet to your server."
                )
                {
                    getTargetTranform = () =>
                    {
                        
                        InfrastructureInstance infrastructureInstance =
                            GameManager.Instance.GetInfrastructureInstanceByID("internetPipe");
                        return infrastructureInstance.transform;
                    },
                    spriteId = "SchematicalBot",
                    NextStepId = TutorialStepId.NetworkPacket_Text
                    
                },
                new TutorialStep(
                    TutorialStepId.NetworkPacket_Text,
                    "HTML Packets",
                    "Notice there are different network packet types. One type is just simple text like HTML."
                )
                {   
                    onTrigger = () =>
                    {
                        GameManager.Instance.UIManager.SetTimeScalePlay();
                    },
                    getTargetTranform = () =>
                    {
                        NetworkPacketData data = GameManager.Instance.GetNetworkPacketDataByType(NetworkPacketData.PType.Text);
                        List<InternetPipe> instances = GameManager.Instance.GetInfrastructureInstanceByClass<InternetPipe>();
                        InternetPipe pipe = instances[Random.Range(0, instances.Count)];
               
                        NetworkPacket networkPacket = pipe.SendPacket(data);
                        return networkPacket.transform;
                    },
                    spriteId = "SchematicalBot",
                    NextStepId = TutorialStepId.NetworkPacket_BinaryImage
                },
                new TutorialStep(
                    TutorialStepId.NetworkPacket_BinaryImage,
                    "Binary Packets",
                    "Another type is binary data like images. Different NetworkPacket types will have different server load and effects on the various infrastructure and will take different routes as your cloud architecture evolves."
                )
                {
                    onTrigger = () =>
                    {
                        GameManager.Instance.UIManager.SetTimeScalePlay();
                    },
                    getTargetTranform = () =>
                    {
                        NetworkPacketData data = GameManager.Instance.GetNetworkPacketDataByType(NetworkPacketData.PType.Image);
                        List<InternetPipe> instances = GameManager.Instance.GetInfrastructureInstanceByClass<InternetPipe>();
                        InternetPipe pipe = instances[Random.Range(0, instances.Count)];
               
                        NetworkPacket networkPacket = pipe.SendPacket(data);
                        return networkPacket.transform;
                    },
                    spriteId = "SchematicalBot",
                    NextStepId = TutorialStepId.Infra_ApplicationServer_Frozen
                    
                },
                new TutorialStep(
                    TutorialStepId.Infra_ApplicationServer_Frozen,
                    "Frozen Infrastructure",
                    "If the load gets higher then the server can handle it will freeze. If that happens network requests will start to fail. This is bad."
                )
                {
                    
                   onTrigger = () =>
                   {
                       InfrastructureInstance infrastructureInstance =
                           GameManager.Instance.GetInfrastructureInstanceByID("server1");
                       infrastructureInstance.SetState(InfrastructureData.State.Frozen);
                       infrastructureInstance.CurrentLoad =
                           infrastructureInstance.GetWorldObjectType().Stats.GetStatValue(StatType.Infra_MaxLoad);
                   },
                    getTargetTranform = () =>
                    {
                        InfrastructureInstance infrastructureInstance =
                            GameManager.Instance.GetInfrastructureInstanceByID("server1");
                        return infrastructureInstance.transform;
                    },
                    spriteId = "SchematicalBot",
                    NextStepId = TutorialStepId.Infra_ApplicationServer_Frozen2
                    
                },
                new TutorialStep(
                    TutorialStepId.Infra_ApplicationServer_Frozen2,
                    "Frozen Infrastructure",
                    "Until you research technology that monitors the servers you will need to manually tell your engineers to fix the frozen infrastructure. Click on the server and select 'Fix' to assign your team to bring it back online."
                )
                {
                    
                    spriteId = "SchematicalBot",
                    
                },
                new TutorialStep(
                    TutorialStepId.Infra_ApplicationServer_Fixed,
                    "Server Fixed",
                    "The server is back online. Great work!"
                )
                {
                    
                    spriteId = "SchematicalBot",
                    NextStepId = TutorialStepId.Infra_ApplicationServer_Fixed2
                },
                new TutorialStep(
                    TutorialStepId.Infra_ApplicationServer_Fixed2,
                    "Scaling Server Size",
                    "You can increase the servers stats by increasing the instance size. Click on the server then select \"Upsize\" to do this."
                )
                {
                    getTargetTranform = () =>
                    {
                        InfrastructureInstance infrastructureInstance =
                            GameManager.Instance.GetInfrastructureInstanceByID("server1");
                        return infrastructureInstance.transform;
                    },
                    spriteId = "SchematicalBot",
                },
                new TutorialStep(
                    TutorialStepId.Infra_ApplicationServer_Upsized,
                    "Increase Server Size",
                    "Great work. Just remember increasing the server size also increases its cost."
                )
                {
                    getTargetTranform = () =>
                    {
                        InfrastructureInstance infrastructureInstance =
                            GameManager.Instance.GetInfrastructureInstanceByID("server1");
                        return infrastructureInstance.transform;
                    },
                    spriteId = "SchematicalBot",
                    NextStepId = TutorialStepId.NPC_LevelUp_Pending
                },
                new TutorialStep(
                    TutorialStepId.NPC_LevelUp_Pending,
                    "NPC Level Up",
                    "One of your team members has leveled up. Choose a new trait to give them. Each trait comes with unique bonuses."
                )
                {
                    onTrigger = () =>
                    {
                        NPCBase npc = GameManager.Instance.AllNpcs.Find((npc) => npc.GetComponent<NPCDevOps>() != null);
                        NPCDevOps devOps = npc.GetComponent<NPCDevOps>();
                        devOps.AddXP(91);
                        GameManager.Instance.cameraController.ZoomToAndFollow(npc.transform);
                        GameManager.Instance.UIManager.SetTimeScalePause();
                    },
                    getTargetTranform = () =>
                    {
                        NPCBase npc = GameManager.Instance.AllNpcs.Find((npc) => npc.GetComponent<NPCDevOps>() != null);
                        return npc.transform;
                    },
                    spriteId = "SchematicalBot",
                },
                new TutorialStep(
                    TutorialStepId.NPC_LevelUp_Completed,
                    "Team Members Leveled Up",
                    "Well done. Keep your team members happy and healthy so they level up more often."
                )
                {
                    getTargetTranform = () =>
                    {
                        NPCBase npc = GameManager.Instance.AllNpcs.Find((npc) => npc.GetComponent<NPCDevOps>() != null);
                        return npc.transform;
                    },
                    spriteId = "SchematicalBot",
                    NextStepId = TutorialStepId.ResearchChoice
                },
                new TutorialStep(
                    TutorialStepId.ResearchChoice,
                    "Research Choice",
                    "Choose something else to research to progress forward."
                )
                {
                    getTargetTranform = () =>
                    {
                        InfrastructureInstance infrastructureInstance =
                            GameManager.Instance.GetInfrastructureInstanceByID("desk");
                        return infrastructureInstance.transform;
                    },
                    spriteId = "SchematicalBot",
                },
                
                new TutorialStep(
                    TutorialStepId.Technology_DedicatedDB_Unlocked,
                    "Dedicated DB",
                    "Congrats! You researched more Server Infrastructure that available to be built. You will want to assign your team to build it when you are ready"
                )
                {
                   
                    getTargetTranform = () =>
                    {
                        InfrastructureInstance infrastructureInstance =
                            GameManager.Instance.GetInfrastructureInstanceByID("dedicated-db");
                        return infrastructureInstance.transform;
                    },
                    spriteId = "SchematicalBot",
                },
                
                new TutorialStep(
                    TutorialStepId.Infra_DedicatedDB_Operational,
                    "Dedicated DB",
                    "Well done! Notice the load costs of certain packets have gone down even further on the server you built earlier. \nThis is because some of the load has been transferred to the hardware you just built.\n Research and build more to keep up with demand."
                )
                {
                    onFinish = () =>
                    {
                        if (GetStep(TutorialStepId.Technology_Whiteboard_Unlocked).State !=
                            TutorialStep.TutorialStepState.Completed)
                        {
                            Trigger(TutorialStepId.ResearchChoice);
                        }
                        else
                        {
                            Trigger(TutorialStepId.EconomyBasics);
                        }
                
                    },
                    getTargetTranform = () =>
                    {
                        InfrastructureInstance infrastructureInstance =
                            GameManager.Instance.GetInfrastructureInstanceByID("dedicated-db");
                        return infrastructureInstance.transform;
                    },
                    spriteId = "SchematicalBot",
                },
                new TutorialStep(
                    TutorialStepId.Technology_Whiteboard_Unlocked,
                    "Software Releases",
                    "Great work. Another way you can level up your gameplay is to code and deploy releases." + 
                    "Once you build it you can select a feature to focus on by clicking on the whiteboard."
                )
                {
                    getTargetTranform = () =>
                    {
                        InfrastructureInstance infrastructureInstance =
                            GameManager.Instance.GetInfrastructureInstanceByID("whiteboard");
                        return infrastructureInstance.transform;
                    },
                    spriteId = "SchematicalBot",
                },
                new TutorialStep(
                    TutorialStepId.Infra_Whiteboard_Operational,
                    "Software Releases",
                    "Select a feature to focus on by clicking on the whiteboard."
                )
                {
                    getTargetTranform = () =>
                    {
                        InfrastructureInstance infrastructureInstance =
                            GameManager.Instance.GetInfrastructureInstanceByID("whiteboard");
                        return infrastructureInstance.transform;
                    },
                    spriteId = "SchematicalBot",
                },
                new TutorialStep(
                    TutorialStepId.Release_InDevelopment,
                    "Software Releases",
                    "Your team will now work on coding the next release when higher priority tasks are not available.\n" +
                    "You can research tools that will allow you to set tasks priority later"
                )
                {
                    getTargetTranform = () =>
                    {
                        InfrastructureInstance infrastructureInstance =
                            GameManager.Instance.GetInfrastructureInstanceByID("whiteboard");
                        return infrastructureInstance.transform;
                    },
                    spriteId = "SchematicalBot",
                },
                new TutorialStep(
                    TutorialStepId.Release_DeploymentReady,
                    "Deployment Ready",
                    "Your first release is ready to be deployed. Click on all available Application Servers to trigger up the deployment.\n" +
                    "Use Meta Challenges to unlock technologies that will automate this for you in the future."
                )
                {
                    getTargetTranform = () =>
                    {
                        InfrastructureInstance infrastructureInstance =
                            GameManager.Instance.GetInfrastructureInstanceByID("server1");
                        return infrastructureInstance.transform;
                    },
                    spriteId = "SchematicalBot",
                },
                new TutorialStep(
                    TutorialStepId.Release_DeploymentRewardReady,
                    "Deployment Reward",
                    "Well done. Now you will receive the rewards that release unlocked. \n" +
                    "You can only focus on one release at a time right now."
                )
                {
                    getTargetTranform = () =>
                    {
                        InfrastructureInstance infrastructureInstance =
                            GameManager.Instance.GetInfrastructureInstanceByID("server1");
                        return infrastructureInstance.transform;
                    },
                    spriteId = "SchematicalBot",
                },
                new TutorialStep(
                    TutorialStepId.Release_DeploymentCompleted,
                    "Deployment Completed",
                    "A bug was introduced in the last release \n" +
                    "You can choose to debug it now or leave it for now. \n" +
                    "Be careful though, bugs left in production have consequences."
                )
                {
                    onFinish = () =>
                    {
                        if (GetStep(TutorialStepId.Technology_DedicatedDB_Unlocked).State !=
                            TutorialStep.TutorialStepState.Completed)
                        {
                            Trigger(TutorialStepId.ResearchChoice);
                        }
                        else
                        {
                            Trigger(TutorialStepId.EconomyBasics);
                        }
                
                    },
                    getTargetTranform = () =>
                    {
                        NPCBug bugGO = GameManager.Instance.SpawnNPCBug();
                        return bugGO.transform;
                    },
                    spriteId = "SchematicalBot",
                    // NextStepId = TutorialStepId.EconomyBasics,
                },
                new TutorialStep(
                    TutorialStepId.EconomyBasics,
                    "Economy Basics:",
                    "It looks like you are settling right in. \n" +
                    "Now that you have the basics we are going to start the in game clock. " + 
                    "At the end of each day you will get a summary. \n" + 
                    "Our infrastructure budget is directly related to how many packets make it through. " + 
                    "If we run out of money its Game Over..."
                )
                {
                   onTrigger = () =>
                   {
                       GameManager.Instance.SetStat(StatType.PacketsSent, 0);
                       GameManager.Instance.SetStat(StatType.PacketsSucceeded, 0);
                       GameManager.Instance.SetStat(StatType.PacketsFailed, 0);

                      
                       GameManager.Instance.cameraController.StopFollowing();
                     
                       GameManager.Instance.GameLoopManager.playTimerActive = true;
                       End();
                    },
                    getTargetTranform = () =>
                    {
                        NPCBase npc = GameManager.Instance.AllNpcs.Find((npc) => npc.GetComponent<BossNPC>() != null);
                        return npc.transform;
                    },
                    spriteId = "Suit1NPC",
                }
            };

            foreach (TutorialStep step in steps)
            {
                Steps[step.Id] = step;
            }
        }
        public TutorialStep GetStep(TutorialStepId stepId)
        {
            if (!Steps.ContainsKey(stepId))
            {
                throw new System.Exception($"Step {stepId} not found");
            }
            
            return Steps[stepId];
        }

        public TutorialStep Trigger(TutorialStepId stepId)
        {
            Debug.Log($"Triggering step {stepId}");
            TutorialStep step = GetStep(stepId);
            step.Trigger();
            return step;
        }

        public string GetSavePath()
        {
            return MetaGameManager.GetSavePath("techdebt", "tutorial_progress.json");
        }
        public void SaveProgress(MetaProgressData metaProgressData)
        {
            string json = JsonUtility.ToJson(metaProgressData, true);
            string path = GetSavePath();
            File.WriteAllText(path, json);
        
     
        }

        public MetaProgressData LoadProgress()
        {
            if (!File.Exists(GetSavePath()))
            {
                return new MetaProgressData();
            }
            string path = GetSavePath();
            string json = File.ReadAllText(path);
            return JsonUtility.FromJson<MetaProgressData>(json);
        }

        public TutorialStep ForceRender(TutorialStepId tutorialStepId)
        {
            TutorialStep step = GetStep(tutorialStepId);
            step.Render();
            return step;
        }

        public void Start()
        {
            GameManager.Instance.GameLoopManager.playTimerActive = false;
            TutorialStep step = GetStep(TutorialStepId.NPC_Boss);
            GameManager.OnInfrastructureStateChange += HandleInfrastructureStateChange;
            GameManager.OnTechnologyStateChange += HandleTechnologyStateChange;
            GameManager.OnPhaseChange += HandlePhaseChange;
            GameManager.OnReleaseChanged += HandleReleaseChange;
            step.Trigger();
        }


        public void End()
        {
            State = TutorialManagerState.Inactive;
            GameManager.OnInfrastructureStateChange -= HandleInfrastructureStateChange;
            GameManager.OnTechnologyStateChange -= HandleTechnologyStateChange;
            GameManager.OnPhaseChange -= HandlePhaseChange;
            GameManager.OnReleaseChanged -= HandleReleaseChange;
            
        }

        private void HandleTechnologyStateChange(Technology technology, Technology.State previousState)
        {
            switch (technology.TechnologyID)
            {
                case("application-server"):
                    if (technology.CurrentState == Technology.State.Researching)
                    {
                        Trigger(TutorialStepId.Technology_ApplicationServer_Researching);
                    } else if (
                        technology.CurrentState == Technology.State.Unlocked &&
                        previousState == Technology.State.Researching
                    )
                    {
                        Trigger(TutorialStepId.Technology_ApplicationServer_Unlocked);
                    }
                    
                    break;
                case("dedicated-db"):
                    if (
                        technology.CurrentState == Technology.State.Unlocked &&
                        previousState == Technology.State.Researching
                    )
                    {
                        Trigger(TutorialStepId.Technology_DedicatedDB_Unlocked);
                    }

                    break;
                case("white-board"):
                    if (
                        technology.CurrentState == Technology.State.Unlocked &&
                        previousState == Technology.State.Researching
                    )
                    {
                        Trigger(TutorialStepId.Technology_Whiteboard_Unlocked);
                    }

                    break;
                default:
                    Debug.LogError($"Not Implemented: {technology.TechnologyID}");
                    break;
            }
        }
        private void HandleInfrastructureStateChange(InfrastructureInstance infrastructureInstance, InfrastructureData.State? previousState)
        {
            switch (infrastructureInstance.data.worldObjectType)
            {
                case(WorldObjectType.Type.ApplicationServer):
                    if (infrastructureInstance.CurrentSizeLevel > 0)
                    {
                        Trigger(TutorialStepId.Infra_ApplicationServer_Upsized);
                    } else if(
                        previousState == InfrastructureData.State.Unlocked && 
                        infrastructureInstance.data.CurrentState  == InfrastructureData.State.Planned
                    )
                    {
                        Trigger(TutorialStepId.Infra_ApplicationServer_Planned);
                    } else if(
                        previousState == InfrastructureData.State.Planned && 
                        infrastructureInstance.data.CurrentState  == InfrastructureData.State.Operational
                    )
                    {
                        Trigger(TutorialStepId.Infra_ApplicationServer_Operational);
                    } else if(
                        previousState == InfrastructureData.State.Frozen && 
                        infrastructureInstance.data.CurrentState  == InfrastructureData.State.Operational
                    )
                    {
                        Trigger(TutorialStepId.Infra_ApplicationServer_Fixed);
                    } 
                  
                break;
                case(WorldObjectType.Type.DedicatedDB):
                    if(
                        previousState == InfrastructureData.State.Planned && 
                        infrastructureInstance.data.CurrentState  == InfrastructureData.State.Operational
                    )
                    {
                        Trigger(TutorialStepId.Infra_DedicatedDB_Operational);
                    }

                    break;
                case(WorldObjectType.Type.WhiteBoard):
                    if(
                        previousState == InfrastructureData.State.Planned && 
                        infrastructureInstance.data.CurrentState  == InfrastructureData.State.Operational
                    )
                    {
                        Trigger(TutorialStepId.Infra_Whiteboard_Operational);
                    }

                    break;
                    
            }
        }

        private void HandleReleaseChange(ReleaseBase release, ReleaseBase.ReleaseState state)
        {
            switch (release.State)
            {
                case(ReleaseBase.ReleaseState.InDevelopment):
                    Trigger(TutorialStepId.Release_InDevelopment);
                    break;
                case(ReleaseBase.ReleaseState.DeploymentReady):
                    Trigger(TutorialStepId.Release_DeploymentReady);
                    break;
                case(ReleaseBase.ReleaseState.DeploymentRewardReady):
                    Trigger(TutorialStepId.Release_DeploymentRewardReady);
                    break; 
                case(ReleaseBase.ReleaseState.DeploymentCompleted):
                    Trigger(TutorialStepId.Release_DeploymentCompleted);
                    break;
            }
        }

        private void HandlePhaseChange(GameLoopManager.GameState obj)
        {
            
        }

       

        public void Next(TutorialStepId nextStepId)
        {
            if (nextStepId == TutorialStepId.None)
            {
                return;
            }
            TutorialStep tutorialStep = GetStep(nextStepId);
            tutorialStep.Trigger();
        }

        public bool IsActive()
        {
            return State == TutorialManagerState.Inactive;
        }
    }
}