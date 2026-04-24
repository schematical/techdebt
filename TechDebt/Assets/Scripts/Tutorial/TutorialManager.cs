using System.Collections.Generic;
using System.IO;
using System.Linq;
using DefaultNamespace;
using Infrastructure;
using NPCs;
using Stats;
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
            Passive,
            Inactive,
        }

        public TutorialManagerState State { get; protected set; } = TutorialManagerState.Active;
        protected Dictionary<TutorialStepId, TutorialStep> Steps = new Dictionary<TutorialStepId, TutorialStep>();
        protected TutorialStepId CurrentTutorialStepId = TutorialStepId.None;
        
        public TutorialManager()
        {
            List<TutorialStep> steps = new List<TutorialStep>()
            {
                new TutorialStep(
                    TutorialStepId.NPC_Schematical,
                    "Welcome",
                    "Hello! Welcome to the team. Your job is to keep the servers up and running fast so our startup can grow and make a profit."
                )
                {
                    // spriteId = "file-cat",
                    NextStepId = TutorialStepId.NPC_Consultant,
                    forcePause = false,
                    onTrigger = () =>
                    {
                        GameManager.Instance.GameLoopManager.SetPlayTimerActive(false);
                        GameManager.Instance.UIManager.planPhaseMenuPanel.Close();
                    }
                },
                new TutorialStep(
                    TutorialStepId.NPC_Consultant,
                    "Help",
                    "I am here to guide you as you spin up your server infrastructure so you can run your company."
                )
                {
                    getTarget = () =>
                    {
                        WorldObjectBase door =
                            GameManager.Instance.GetInfrastructureInstanceByID("door");
                        return door;
                    },
                    forcePause = false,
                    NextStepId = TutorialStepId.Infra_Door
                },
                new TutorialStep(
                    TutorialStepId.Infra_Door,
                    "Door",
                    "Your team will enter via this door at the beginning of the day and exit at the end of the day. \n" + 
                    "Don't worry, for this part of the tutorial the clock is not ticking yet. \n" + 
                    "Click 'Start Day' to start your day. "
                )
                {
                    onTrigger = () =>
                    {
                        GameManager.Instance.HireNPCDevOps(new NPCDevOpsData { DailyCost = 100 });
                        GameManager.Instance.GameLoopManager.BeginPlanPhase();
                        
                    },
                    onPreCheck = (TutorialStep step) =>
                    {
                        if (step.State == TutorialStep.TutorialStepState.Completed)
                        {
                            GameManager.Instance.HireNPCDevOps(new NPCDevOpsData { DailyCost = 100 });
                            GameManager.Instance.GameLoopManager.BeginPlanPhase();
                            GameManager.Instance.SetStat(StatType.PacketsSent, 0);
                            GameManager.Instance.SetStat(StatType.PacketsSucceeded, 0);
                            GameManager.Instance.SetStat(StatType.PacketsFailed, 0);


                            GameManager.Instance.cameraController.StopFollowing();

                            GameManager.Instance.GameLoopManager.SetPlayTimerActive(true);
                            MarkPassive();
                        }

                    },
                    showContinue = false,
                    getTarget = () =>
                    {
                        WorldObjectBase door =
                            GameManager.Instance.GetInfrastructureInstanceByID("door");
                        return door;
                    },
                },
                new TutorialStep(
                    TutorialStepId.Day_Start,
                    "Your Team",
                    "Here is one of your engineers now. They will wander around until you assign them tasks to do."
                )
                {
                    getTarget = () =>
                    {
                        WorldObjectBase door =
                            GameManager.Instance.GetInfrastructureInstanceByID("door");
                        return door;
                    },
                    NextStepId = TutorialStepId.Infra_Desk,
                    forcePause = false
                },
                
                new TutorialStep(
                    TutorialStepId.Infra_Desk,
                    "Desk",
                    "Click on the Desk to assign your team members to do research tasks."
                )
                {
                    showContinue = false,
                    getTarget = () =>
                    {
                        InfrastructureInstance infrastructureInstance =
                            GameManager.Instance.GetInfrastructureInstanceByID("desk");
                        infrastructureInstance.ShowAttentionIcon();
                        return infrastructureInstance;
                    },

                },
                new TutorialStep(
                    TutorialStepId.Technology_ApplicationServer_Researching,
                    "Speed Up Time",
                    "To speed things up, use the controls in the lower right-hand side of the screen to manipulate in-game time. \n" +
                    "Once this part of the tutorial is over, it will increase the speed of the game clock as well."
                )
                {
           // TODO: Make this non blocking. Triggered but increasing the speed.
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
                    showContinue = false,
                    getTarget = () =>
                    {
                        InfrastructureInstance infrastructureInstance =
                            GameManager.Instance.GetInfrastructureInstanceByID("server1");
                        infrastructureInstance.ShowAttentionIcon();
                        return infrastructureInstance;
                    },

                },
                new TutorialStep(
                    TutorialStepId.Infra_ApplicationServer_Planned,
                    "Building",
                    "Now one of your team members will get to work spinning up your application server so it can handle incoming traffic."
                )
                {
                    getTarget = () =>
                    {
                        InfrastructureInstance infrastructureInstance =
                            GameManager.Instance.GetInfrastructureInstanceByID("server1");
                        infrastructureInstance.ShowAttentionIcon();
                        return infrastructureInstance;
                    },
                },
                new TutorialStep(
                    TutorialStepId.Infra_ApplicationServer_Operational,
                    "Server Built",
                    "This is the Application Server. " +
                    "It will receive Network Packets coming from the Internet, process them, and send back a response to whoever sent the request on the Internet."
                )
                {
                    getTarget = () =>
                    {
                        InfrastructureInstance infrastructureInstance =
                            GameManager.Instance.GetInfrastructureInstanceByID("server1");
                        return infrastructureInstance;
                    },
                    NextStepId = TutorialStepId.Infra_InternetPipe

                },
                new TutorialStep(
                    TutorialStepId.Infra_InternetPipe,
                    "The Internetz",
                    "Notice Network Packets flowing in from the Internet to your server.\n" //  +
                    // "When a packet finishes it's journey you can see how many milliseconds it took for the trip to take. \n" +
                    // "This is known as \"Latency\" and it will be important later."
                )
                {
                    getTarget = () =>
                    {

                        InfrastructureInstance infrastructureInstance =
                            GameManager.Instance.GetInfrastructureInstanceByID("internetPipe");
                        return infrastructureInstance;
                    },
                    NextStepId = TutorialStepId.NetworkPacket_Text

                },
                new TutorialStep(
                    TutorialStepId.NetworkPacket_Text,
                    "HTML Packets",
                    "Notice there are different network packet types. One type is just simple text like HTML."
                )
                {
                    getTarget = () =>
                    {
                        InternetPipe pipe =
                            GameManager.Instance.GetInfrastructureInstanceByID("internetPipe").GetComponent<InternetPipe>();
                        /*NetworkPacketData data =
                            GameManager.Instance.GetNetworkPacketDataByType(NetworkPacketData.PType.Text);
                        NetworkPacket networkPacket = pipe.SendPacket(data);*/
                        return pipe;
                    },
                 
                    spriteId = "file",
                    NextStepId = TutorialStepId.NetworkPacket_BinaryImage
                },
                new TutorialStep(
                    TutorialStepId.NetworkPacket_BinaryImage,
                    "Binary Packets",
                    "Another type is binary data like images. Different NetworkPacket types will have different server load and effects on the various infrastructure and will take different routes as your cloud architecture evolves."
                )
                {
                    getTarget = () =>
                    {
                        InternetPipe pipe =
                            GameManager.Instance.GetInfrastructureInstanceByID("internetPipe").GetComponent<InternetPipe>();
                        /*NetworkPacketData data =
                            GameManager.Instance.GetNetworkPacketDataByType(NetworkPacketData.PType.Image);
                        NetworkPacket networkPacket = pipe.SendPacket(data);*/
                        return pipe;
                    },
                    NextStepId = TutorialStepId.Basics_Economy,
                    spriteId = "file-cat"
                },
              
                new TutorialStep(
                    TutorialStepId.Technology_DedicatedDB_Unlocked,
                    "Dedicated DB",
                    "Congrats! You researched more Server Infrastructure that is available to be built. You will want to assign your team to build it when you are ready."
                )
                {

                    getTarget = () =>
                    {
                        InfrastructureInstance infrastructureInstance =
                            GameManager.Instance.GetInfrastructureInstanceByID("dedicated-db");
                        return infrastructureInstance;
                    },
                },

                new TutorialStep(
                    TutorialStepId.Infra_DedicatedDB_Operational,
                    "Dedicated DB",
                    "Well done! Notice the load costs of certain packets have gone down even further on the server you built earlier. \n" + 
                    "This is because some of the load has been transferred to the hardware you just built.\n " + 
                    "Research and build more to keep up with demand."
                )
                {
                   
                    getTarget = () =>
                    {
                        InfrastructureInstance infrastructureInstance =
                            GameManager.Instance.GetInfrastructureInstanceByID("dedicated-db");
                        return infrastructureInstance;
                    },
                },
                new TutorialStep(
                    TutorialStepId.Technology_Whiteboard_Unlocked,
                    "Software Releases",
                    "Great work. Another way you can level up your gameplay is to code and deploy releases. " +
                    "Once you build it, you can select a feature to focus on by clicking on the whiteboard."
                )
                {
                    
                    getTarget = () =>
                    {
                        InfrastructureInstance infrastructureInstance =
                            GameManager.Instance.GetInfrastructureInstanceByID("whiteboard");
                        return infrastructureInstance;
                    },
                },
                new TutorialStep(
                    TutorialStepId.Infra_Whiteboard_Operational,
                    "Software Releases",
                    "Select a feature to focus on by clicking on the whiteboard then selecting 'Plan Next Release'."
                )
                {
                    getTarget = () =>
                    {
                        InfrastructureInstance infrastructureInstance =
                            GameManager.Instance.GetInfrastructureInstanceByID("whiteboard");
                        return infrastructureInstance;
                    },
                },
                new TutorialStep(
                    TutorialStepId.Release_InDevelopment,
                    "Software Releases",
                    "Your team will now work on coding the next release when higher priority tasks are not available.\n" +
                    "You can research tools that will allow you to set task priority later."
                )
                {
                    getTarget = () =>
                    {
                        InfrastructureInstance infrastructureInstance =
                            GameManager.Instance.GetInfrastructureInstanceByID("whiteboard");
                        return infrastructureInstance;
                    },
                },
                new TutorialStep(
                    TutorialStepId.Release_DeploymentReady,
                    "Deployment Ready",
                    "Your first release is ready to be deployed. Click on all available Application Servers to trigger the deployment.\n" +
                    "Use Meta Challenges to unlock technologies that will automate this for you in the future."
                )
                {
                    getTarget = () =>
                    {
                        InfrastructureInstance infrastructureInstance =
                            GameManager.Instance.GetInfrastructureInstanceByID("server1");
                        return infrastructureInstance;
                    },
                },
                new TutorialStep(
                    TutorialStepId.Release_DeploymentRewardReady,
                    "Deployment Reward",
                    "Well done. Depending on the skill level of the engineers that worked on the release, your reward will be assigned a higher rarity.  \n" +
                    "Click 'Deployment Complete' to find out the rarity of your reward."
                )
                {
                    getTarget = () =>
                    {
                        InfrastructureInstance infrastructureInstance =
                            GameManager.Instance.GetInfrastructureInstanceByID("server1");
                        return infrastructureInstance;
                    },
                    NextStepId = TutorialStepId.Basics_Economy
                },
                new TutorialStep(
                    TutorialStepId.Release_DeploymentCompleted,
                    "Deployment Completed",
                    "That reward will now be applied to improve your server infrastructure."
                )
                {
                    getTarget = () =>
                    {
                        InfrastructureInstance infrastructureInstance =
                            GameManager.Instance.GetInfrastructureInstanceByID("server1");
                        return infrastructureInstance;
                    },
                },
                new TutorialStep(
                    TutorialStepId.NPC_Bug_Spawn,
                    "A Bug!",
                    "A bug was introduced in the last release. Try to find it. \n" +
                    "You can choose to `Debug` it now by clicking on it or leave it for now. \n" +
                    "Be careful though, bugs left in production have consequences."
                )
                {
                    /*getTarget = () =>
                    {
                        NPCBase npc =
                            GameManager.Instance.AllNpcs.Find((npc) => npc.GetComponent<NPCBug>() != null);
                        return npc;
                    },*/
                },
                new TutorialStep(
                    TutorialStepId.NPC_Bug_Dead,
                    "Where bugs come from.",
                    "You debugged it! " +
                    "The quality of your latest Software Release and the amount of Tech Debt you have will influence the likelihood of bugs spawning and their severity. \n"
                )
                {
                    getTarget = () =>
                    {
                        NPCBase npc =
                            GameManager.Instance.AllNpcs.Find((npc) => npc.GetComponent<NPCBug>() != null);
                        return npc;
                    },
                },
                new TutorialStep(
                    TutorialStepId.NetworkPacket_Failed,
                    "Network Packet Failed",
                    "NetworkPackets are failing! " +
                    "There are many reasons NetworkPackets can fail. Bugs can get them, or your servers can become overloaded and freeze.\n" +
                    "Try to figure out the cause and fix it."
                )
                {
                   
                    
                },
               

                new TutorialStep(
                    TutorialStepId.Basics_Economy,
                    "Economy Basics",
                    "Booting up these servers isn't free. At the end of each day, you will get charged for the infrastructure you use. \n" + 
                    "We have a limited budget, so if you exceed the budget, it is game over. \n" + 
                    "Keep an eye on it in the UI."
                )
                {
      
                    NextStepId = TutorialStepId.Basics_LaunchDay
                },
                new TutorialStep(
                    TutorialStepId.Basics_LaunchDay,
                    "Launch Day",
                    "We have enough cash to make it until the end of the week.\n" +
                    "That will be `Launch Day` when we can finally start making money. \n" + 
                    "Expect a lot more traffic and a new type of `NetworkPacket` to indicate incoming purchases."
                )
                {
 
                    NextStepId = TutorialStepId.Basics_Day
                },
                new TutorialStep(
                    TutorialStepId.Basics_Day,
                    "Day Cycle",
                    "The clock is now ticking! At the end of the day traffic will increase.\n" +
                    "Keep researching and building to progress the run."
                )
                {
                    onTrigger = () =>
                    {
                        GameManager.Instance.SetStat(StatType.PacketsSent, 0);
                        GameManager.Instance.SetStat(StatType.PacketsSucceeded, 0);
                        GameManager.Instance.SetStat(StatType.PacketsFailed, 0);


                        GameManager.Instance.cameraController.StopFollowing();

                        GameManager.Instance.GameLoopManager.SetPlayTimerActive(true);
                        MarkPassive();
                    },
                    onPreCheck = (TutorialStep step) =>
                    {
                        if (step.State == TutorialStep.TutorialStepState.Incomplete)
                        {
                            return;
                        }
                        GameManager.Instance.SetStat(StatType.PacketsSent, 0);
                        GameManager.Instance.SetStat(StatType.PacketsSucceeded, 0);
                        GameManager.Instance.SetStat(StatType.PacketsFailed, 0);


                        GameManager.Instance.cameraController.StopFollowing();

                        GameManager.Instance.GameLoopManager.SetPlayTimerActive(true);
                        MarkPassive();
                    },
                    getTarget = () =>
                    {
                        InfrastructureInstance infrastructureInstance =
                            GameManager.Instance.GetInfrastructureInstanceByID("door");
                        return infrastructureInstance;
                    },
                },

                new TutorialStep(
                    TutorialStepId.Basics_Sprint,
                    "Sprints Basics",
                    "TODO \n" +
                    ""
                )
                {
                    getTarget = () =>
                    {
                        NPCBase npc =
                            GameManager.Instance.AllNpcs.Find((npc) => npc.GetComponent<NPCSchematicalBot>() != null);
                        return npc;
                    },
                },/*
                new TutorialStep(
                    TutorialStepId.Item_View,
                    "Items",
                    "Every once in a while you will receive consumable items that can be used to help you. \n" +
                    "While most of this game uses real Cloud Architecture these items are not... but they are fun and I am planning on adding a lot more variety soon."
                )
                {
                    getTarget = () =>
                    {
                        
                    }
                },*/
                new TutorialStep(
                    TutorialStepId.NetworkPacket_Purchase,
                    "Items",
                    "Since it is launch day, we can now expect a new `NetworkPacket` type called `Purchase`. \n" +
                    "This packet type increases the amount of money allocated for our server infrastructure. \n" +
                    "You can unlock upgrades to increase the chances of this type of packet arriving and the amount you make on each packet."
                ){
                    getTarget = () =>
                    {
                        NetworkPacketData data =
                            GameManager.Instance.GetNetworkPacketDataByType(NetworkPacketData.PType.Purchase);
                        List<InternetPipe> instances =
                            GameManager.Instance.GetInfrastructureInstanceByClass<InternetPipe>();
                        InternetPipe pipe = instances[Random.Range(0, instances.Count)];

                        NetworkPacket networkPacket = pipe.SendPacket(data);
                        return pipe;
                    },
                },
                new TutorialStep(
                    TutorialStepId.Infra_ApplicationServer_Frozen,
                    "Frozen Infrastructure",
                    "If the load gets higher than the server can handle, it will freeze. If that happens, network requests will start to fail. This is bad."
                )
                {

                    /*unlockConditions = new List<UnlockCondition>()
                    {
                        new UnlockCondition()
                        {
                            Type = UnlockCondition.ConditionType.TutorialStepState,
                            TutorialStepId =  TutorialStepId.NetworkPacket_Failed
                        }
                    },*/
                    /*onTrigger = () =>
                    {
                        InfrastructureInstance infrastructureInstance =
                            GameManager.Instance.GetInfrastructureInstanceByID("server1");
                        infrastructureInstance.SetState(InfrastructureData.State.Frozen);
                        infrastructureInstance.CurrentLoad =
                            infrastructureInstance.GetWorldObjectType().Stats.GetStatValue(StatType.Infra_MaxLoad);
                    },*/
                    getTarget = () =>
                    {
                        InfrastructureInstance infrastructureInstance =
                            GameManager.Instance.GetInfrastructureInstanceByID("server1");
                        return infrastructureInstance;
                    },
                    NextStepId = TutorialStepId.Infra_ApplicationServer_Frozen2

                },
                new TutorialStep(
                    TutorialStepId.Infra_ApplicationServer_Frozen2,
                    "Frozen Infrastructure",
                    "Until you research technology that monitors the servers, you will need to manually tell your engineers to fix the frozen infrastructure. Click on the server and select 'Fix' to assign your team to bring it back online."
                )
                {
                    
                    getTarget = () =>
                    {
                        InfrastructureInstance infrastructureInstance =
                            GameManager.Instance.GetInfrastructureInstanceByID("server1");
                        return infrastructureInstance;
                    },

                },
                /*new TutorialStep(
                    TutorialStepId.Task_FixFrozen_Queued,
                    "Task: Fix Queued",
                    "One of your engineers will get to work fixing this server shortly."
                )
                {
                    
                    getTarget = () =>
                    {
                        InfrastructureInstance infrastructureInstance =
                            GameManager.Instance.GetInfrastructureInstanceByID("server1");
                        return infrastructureInstance;
                    },

                },*/
                new TutorialStep(
                    TutorialStepId.Infra_ApplicationServer_Fixed,
                    "Server Fixed",
                    "The server is back online. Great work!"
                )
                {
                    getTarget = () =>
                    {
                        InfrastructureInstance infrastructureInstance =
                            GameManager.Instance.GetInfrastructureInstanceByID("server1");
                        return infrastructureInstance;
                    },
                    NextStepId = TutorialStepId.Infra_ApplicationServer_Fixed2,
                },
                new TutorialStep(
                    TutorialStepId.Infra_ApplicationServer_Fixed2,
                    "Scaling Server Size",
                    "You can increase the load the server can take by increasing the server size. \n" + 
                    "First you will need to unlock the a bigger server size by researching it."
                )
                {
                    getTarget = () =>
                    {
                        InfrastructureInstance infrastructureInstance =
                            GameManager.Instance.GetInfrastructureInstanceByID("desk");
                        return infrastructureInstance;
                    },
                },
                new TutorialStep(
                    TutorialStepId.Technology_ApplicationServerSizeMedium_Unlocked,
                    "Technology Application Server Size Medium",
                    "Great work. To resize a server click on it and select 'Upsize' in the menu."
                )
                {
                    
                    getTarget = () =>
                    {
                        InfrastructureInstance infrastructureInstance =
                            GameManager.Instance.GetInfrastructureInstanceByID("server1");
                        return infrastructureInstance;
                    },

                },
                new TutorialStep(
                    TutorialStepId.Task_Resize_Queued,
                    "Task: Resize Queued",
                    "One of your engineers will get to work resizing this server shortly."
                )
                {
                    
                    getTarget = () =>
                    {
                        InfrastructureInstance infrastructureInstance =
                            GameManager.Instance.GetInfrastructureInstanceByID("server1");
                        return infrastructureInstance;
                    },

                },
                new TutorialStep(
                    TutorialStepId.Infra_ApplicationServer_Upsized,
                    "Increase Server Size",
                    "Great work. Just remember that increasing the server size also increases its cost."
                )
                {
                    getTarget = () =>
                    {
                        InfrastructureInstance infrastructureInstance =
                            GameManager.Instance.GetInfrastructureInstanceByID("server1");
                        return infrastructureInstance;
                    },
                },
                new TutorialStep(
                    TutorialStepId.NPC_LevelUp_Pending,
                    "NPC Level Up",
                    "One of your team members has leveled up. Choose a new trait to give them. Each trait comes with unique bonuses."
                )
                {
                    getTarget = () =>
                    {
                        NPCBase npc = GameManager.Instance.AllNpcs.Find((npc) => npc.GetComponent<NPCDevOps>() != null);
                        return npc;
                    },
                },
                new TutorialStep(
                    TutorialStepId.NPC_LevelUp_Completed,
                    "Team Members Leveled Up",
                    "Well done. Keep your team members happy and healthy so they level up more often."
                )
                {
                    getTarget = () =>
                    {
                        NPCBase npc = GameManager.Instance.AllNpcs.Find((npc) => npc.GetComponent<NPCDevOps>() != null);
                        return npc;
                    },
                }
            };
            InitWorldObjectTips(steps);
            foreach (TutorialStep step in steps)
            {
                Steps[step.Id] = step;
            }
        }

        private void InitWorldObjectTips(List<TutorialStep> steps)
        {
            steps.AddRange(
                new List<TutorialStep>()
                {
                    new TutorialStep(
                        TutorialStepId.Infra_ApplicationServer_Tip,
                        "Application Server",
                        "The Application Server is where the main logic for your application should live.\n" +
                        "You can host everything, including images and your database, on it, but that won't scale. \n" +
                        "So research specialized tech that is optimized for the various network traffic."
                        
                    )
                    {
                        Type = TutorialStep.TutorialStateType.Tip,
                        spriteId = "server1"
                    },
              
                    new TutorialStep(
                        TutorialStepId.Infra_DedicatedDB_Tip,
                        "Dedicated DB",
                        "Having a Dedicated DB will significantly reduce the load on the Application Server.\n" +
                        "It will handle text traffic, but not images, because saving images to a relational DB is strongly NOT recommended at scale. \n"
                        
                    )
                    {
                        Type = TutorialStep.TutorialStateType.Tip,
                        spriteId = "rds-simple-1"
                    },
              
                    new TutorialStep(
                        TutorialStepId.Infra_BinaryStorage_Tip,
                        "Binary Storage",
                        "Moving binary storage to its own hardware will significantly reduce the load on the Application Server.\n" +
                        "It will handle image traffic. This also will make it so you can boot up more application servers once you have unlocked the Load Balancer. \n"
                        
                    )
                    {
                        Type = TutorialStep.TutorialStateType.Tip,
                        spriteId = "s3-bucket-basic"
                    },
                    new TutorialStep(
                        TutorialStepId.Infra_KanbanBoard_Tip,
                        "Kanban Board",
                        "The Kanban Board allows you to prioritize which tasks your team works on first. \n"
                    )
                    {
                        Type = TutorialStep.TutorialStateType.Tip,
                        spriteId = "KanbanBoard"
                    },
                    new TutorialStep(
                        TutorialStepId.Infra_WhiteBoard_Tip,
                        "Software Basics",
                        "This unlocks the White Board, which allows you to choose the focus of your team's next software release.\n" +
                        "Deploying software releases unlocks powerful rewards and bonuses.\n"
                    )
                    {
                        Type = TutorialStep.TutorialStateType.Tip,
                        spriteId = "WhiteBoard"
                    },
                    new TutorialStep(
                        TutorialStepId.Infra_OrgChart_Tip,
                        "Org Chart",
                        "The Org Chart allows you to unlock new team member positions.\n" +
                        "Each team member you have will unlock new game mechanics and quests (Coming soon).\n"
                    )
                    {
                        Type = TutorialStep.TutorialStateType.Tip,
                        spriteId = "OrgChart"
                    },
                    new TutorialStep(
                        TutorialStepId.Infra_ProductRoadMap_Tip,
                        "Product Road Map",
                        "The Product Road Map will give you insight into previous and upcoming sprints.\n" +
                        "This will allow you to better navigate the run (Work in progress).\n"
                    )
                    {
                        Type = TutorialStep.TutorialStateType.Tip,
                        spriteId = "ProductRoadMap"
                    },
                    new TutorialStep(
                        TutorialStepId.Infra_Redis_Tip,
                        "Caching",
                        "Caching provides high-performance in-memory caching to reduce database queries.\n" +
                        "Building this will take a significant amount of load off your Application Servers.\n"
                    )
                    {
                        Type = TutorialStep.TutorialStateType.Tip,
                        spriteId = "redis-firewall-green"
                    },
                    new TutorialStep(
                        TutorialStepId.Infra_CDN_Tip,
                        "Content Delivery Network",
                        "A CDN distributes static assets like images globally to bring them closer to your users.\n" +
                        "It will take all the binary data/image load off your Application Layer, except for uploads.\n"
                    )
                    {
                        Type = TutorialStep.TutorialStateType.Tip,
                        spriteId = "cloudFront 1"
                    },
                    new TutorialStep(
                        TutorialStepId.Infra_LoadBalancer_Tip,
                        "Load Balancer",
                        "The Load Balancer evenly distributes incoming traffic across multiple Application Servers.\n"
                    )
                    {
                        Type = TutorialStep.TutorialStateType.Tip,
                        spriteId = "server-alb-sheet"
                    },
                    new TutorialStep(
                        TutorialStepId.Infra_WaterCooler_Tip,
                        "Water Cooler",
                        "The Water Cooler provides a place for your team to relax and socialize.\n" +
                        "It improves morale but also can cause occasional distractions. (Work in progress)\n"
                    )
                    {
                        Type = TutorialStep.TutorialStateType.Tip,
                        spriteId = "WaterCooler"
                    },
                    new TutorialStep(
                        TutorialStepId.Infra_WAF_Tip,
                        "Firewall",
                        "A Firewall protects your application from common malicious exploits and attacks.\n" +
                        "It monitors network packets and filters out bad actors before they reach your servers.\n"
                    )
                    {
                        Type = TutorialStep.TutorialStateType.Tip,
                        spriteId = "waf"
                    },
                    new TutorialStep(
                        TutorialStepId.Infra_SQS_Tip,
                        "Queue",
                        "Deferring computational tasks to be processed later by a Worker Server will take a lot of load off the Application Servers.\n" +
                        "You can accomplish this by sending a request to the Queue for the Worker Servers to pick up later. \n"
                    )
                    {
                        Type = TutorialStep.TutorialStateType.Tip,
                        spriteId = "sqs"
                    },
                    new TutorialStep(
                        TutorialStepId.Infra_CodePipeline_Tip,
                        "Build Pipeline",
                        "Build Pipeline automates your software deployment process.\n" +
                        "This will free up your team members to stay focused on other tasks.\n"
                    )
                    {
                        Type = TutorialStep.TutorialStateType.Tip,
                        spriteId = "CodePipeline"
                    },
                    new TutorialStep(
                        TutorialStepId.Infra_SecretManager_Tip,
                        "Secret Manager",
                        "Secret Manager securely stores and manages sensitive credentials.\n" +
                        "This will decrease the chances of a `Credentials Leaked` event.\n"
                    )
                    {
                        Type = TutorialStep.TutorialStateType.Tip,
                        spriteId = "secrets-manager"
                    },
                    new TutorialStep(
                        TutorialStepId.Infra_Cognito_Tip,
                        "Cognito User Pools",
                        "Cognito manages user authentication and identity.\n" +
                        "This decreases the cost and likelihood of PII Network Packets getting leaked.\n"
                    )
                    {
                        Type = TutorialStep.TutorialStateType.Tip,
                        spriteId = "aws-cognito-service"
                    },
                    new TutorialStep(
                        TutorialStepId.Infra_EmailService_Tip,
                        "Email Service",
                        "Email Service handles automated emails and transactional messages.\n" +
                        "It enables essential communication with your users.\n"
                    )
                    {
                        Type = TutorialStep.TutorialStateType.Tip,
                        spriteId = "mail-service"
                    },
                    new TutorialStep(
                        TutorialStepId.Infra_CloudWatchMetrics_Tip,
                        "Metrics",
                        "Metrics gives you better visibility into infrastructure load.\n" + 
                        "Just hold `Shift` and it will display all the infrastructure metrics above the instances."
                    )
                    {
                        Type = TutorialStep.TutorialStateType.Tip,
                        spriteId = "CloudWatch"
                    },
                    new TutorialStep(
                        TutorialStepId.Infra_SNS_Tip,
                        "Mobile Notifications",
                        "Mobile Notifications engage users and drive traffic back to your app.\n" +
                        "Use it strategically to increase usage and retention.\n"
                    )
                    {
                        Type = TutorialStep.TutorialStateType.Tip,
                        spriteId = "SNS"
                    },
                new TutorialStep(
                    TutorialStepId.NPC_SQLInjection_View,
                    "SQL Injection Attack",
                    "Looks like you found an 'SQL Injection Attack'. \n" + 
                    "This is where a malicious party trys to hide SQL (AKA 'Structured Query Language' in the data they send you hoping you don't validate your input.\n" + 
                    "If it makes its way to  the Database then they could steal some of your users PII or worse. \n" + 
                    "You may want to consider focusing a few releases on Input Validation to decrease the chance of this attack making it to the Database."
                )
                {
                    Type = TutorialStep.TutorialStateType.Tip,
                    spriteId = "SQLInjectionNetworkPacket"
                    /*getTarget = () =>
                    {
                        NPCBase npc = GameManager.Instance.AllNpcs.Find((npc) => npc.GetComponent<NPCDevOps>() != null);
                        return npc;
                    },*/
                },
                new TutorialStep(
                    TutorialStepId.NPC_XSS_View,
                    "Cross Site Scripting (XSS) Attack",
                    "Looks like you found an 'Cross Site Scripting Attack' (AKA 'XSS Attack'). \n" + 
                    "This is where a malicious party sends a little frontend code, such as JavaScript, in with a request.\n" + 
                    "If you don't have good Input Validation then that code will get served up to your legitimate users. \n" +
                    "This can redirect your users to malicious websites cause PII leaks or worse. \n" +
                    "You may want to consider focusing a few releases on Input Validation to decrease the chance of this attack hurting your users."
                )
                {
                    Type = TutorialStep.TutorialStateType.Tip,
                    spriteId = "NPCXSS"
                    /*getTarget = () =>
                    {
                        NPCBase npc = GameManager.Instance.AllNpcs.Find((npc) => npc.GetComponent<NPCDevOps>() != null);
                        return npc;
                    },*/
                },
                }
            );
        }

        public TutorialStep GetStep(TutorialStepId stepId)
        {
            if (!Steps.ContainsKey(stepId))
            {
                Debug.LogWarning($"Step {stepId} not found");
            }
            
            return Steps[stepId];
        }

        public TutorialStep Trigger(TutorialStepId stepId)
        {
            if (State == TutorialManagerState.Inactive)
            {
                return null;
            }
            TutorialStep step = GetStep(stepId);
            if (!step.CanBeTriggered())
            {
                return null;
            }
            if (CurrentTutorialStepId != TutorialStepId.None)
            {
                TutorialStep currentStep = GetStep(CurrentTutorialStepId);
                if (currentStep.IsBlocking())
                {
                    Debug.LogWarning($"Trigger not firing off {stepId} because {CurrentTutorialStepId} is Blocking: {currentStep.IsBlocking()}");
                    return null;
                }

                if (currentStep.State != TutorialStep.TutorialStepState.Completed)
                {
                    currentStep.MarkCompleted();
                }
            }
            
            step.Trigger();
            CurrentTutorialStepId = stepId;
            return step;
        }

        public string GetSavePath()
        {
            return MetaGameManager.GetSavePath(null, "tutorial_progress.json");
        }

        public TutorialData BuildTutorialData()
        {
            TutorialData tutorialData = new TutorialData();
            tutorialData.state = State;
            foreach (TutorialStep step in Steps.Values)
            {
                tutorialData.steps.Add(step.ToTutorialData());
            }
            
            return tutorialData;
        }
        public void SaveProgress(TutorialData tutorialData = null)
        {
            if (tutorialData == null)
            {
                tutorialData = BuildTutorialData();
            }
            string json = JsonUtility.ToJson(tutorialData, true);
            string path = GetSavePath();
            File.WriteAllText(path, json);
        }

        public TutorialData LoadProgress()
        {
            if (!File.Exists(GetSavePath()))
            {
                return new TutorialData();
            }
            string path = GetSavePath();
            string json = File.ReadAllText(path);
            return JsonUtility.FromJson<TutorialData>(json);
        }

        public TutorialStep ForceRender(TutorialStepId tutorialStepId)
        {
            TutorialStep step = GetStep(tutorialStepId);
            step.Render();
            return step;
        }

        
        public void Start()
        {    
            GameManager.OnInfrastructureStateChange += HandleInfrastructureStateChange;
             GameManager.OnTechnologyStateChange += HandleTechnologyStateChange;
             GameManager.OnPhaseChange += HandlePhaseChange;
             GameManager.OnReleaseChanged += HandleReleaseChange;
            TutorialData tutorialData = LoadProgress();
            if (tutorialData != null)
            {
                State = tutorialData.state;
                foreach (TutorialStepId tutorialStepId in Steps.Keys)
                {
                    TutorialStepData tutorialStepData = tutorialData.steps.Find((data => data.Id == tutorialStepId));
                    if (tutorialStepData != null)
                    {
                        Steps[tutorialStepId].FromData(tutorialStepData);
                    }
                }

                return;
            }


           
        }

        public void StartNewGameCheck()
        {
            Debug.Log($"StartNewGameCheck: {State} - {Steps.Values.Count}");
            foreach (TutorialStep step in Steps.Values)
            {
                step.PreCheck();
            }
            switch (State)
            {
                case TutorialManagerState.Inactive:
                case TutorialManagerState.Passive:
                    break;
                case TutorialManagerState.Active:
                  
                    TutorialStep step = GetStep(TutorialStepId.NPC_Schematical);
                    
                    step.Trigger();
                    
                    break;
                default:
                    throw new System.Exception($"Unknown state {State}");
            }

            
        }
        public void MarkPassive()
        {
            State = TutorialManagerState.Passive;
            GameManager.Instance.GameLoopManager.SetPlayTimerActive(true);
        }
        public void End()
        {
     

            State = TutorialManagerState.Inactive;
            GameManager.Instance.GameLoopManager.SetPlayTimerActive(true);
            GameManager.OnInfrastructureStateChange -= HandleInfrastructureStateChange;
            GameManager.OnTechnologyStateChange -= HandleTechnologyStateChange;
            GameManager.OnPhaseChange -= HandlePhaseChange;
            GameManager.OnReleaseChanged -= HandleReleaseChange;
        }

        private void HandleTechnologyStateChange(Technology technology, Technology.State previousState)
        {
            // Debug.Log($"HandleTechnologyStateChange: {technology.TargetId} - {technology.CurrentState} - previousState: {previousState}");
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
                case("application-server-size-medium"):
                    if (
                        technology.CurrentState == Technology.State.Unlocked &&
                        previousState == Technology.State.Researching
                    )
                    {
                        Trigger(TutorialStepId.Technology_ApplicationServerSizeMedium_Unlocked);
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
                    // Debug.LogError($"Not Implemented: {technology.TargetId}");
                    break;
            }
        }
        private void HandleInfrastructureStateChange(InfrastructureInstance infrastructureInstance, InfrastructureData.State? previousState)
        {
            switch (infrastructureInstance.data.worldObjectType)
            {
                case(WorldObjectType.Type.ApplicationServer):
                    if (infrastructureInstance.data.CurrentState == InfrastructureData.State.Frozen)
                    {
                        Trigger(TutorialStepId.Infra_ApplicationServer_Frozen);
                    } else if (infrastructureInstance.CurrentSize > 0)
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
                CurrentTutorialStepId = TutorialStepId.None;
                SaveProgress();
                return;
            }

            Trigger(nextStepId);
            SaveProgress();
       
        }

        public bool IsActive()
        {
            return State != TutorialManagerState.Active;
        }

        public List<TutorialStep> GetSteps()
        {
            return Steps.Values.ToList();
        }
    }
}