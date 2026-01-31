using System;
using System.Collections.Generic;
using System.Linq;
using NPCs;
using UnityEngine;

namespace Events
{
    public class TutorialEvent : EventBase
    {
        public enum TutorialCheck
        {
            NPC_AddTrait
        }

        protected int currentStep = 0;
        protected List<DialogButtonOption> options = new List<DialogButtonOption>();

        protected Sprite bossSprite;
        protected Sprite botSprite;
        protected NPCSchematicalBot schematicalBot;

        protected bool firstTechnologyResearched = false;
        protected int nextStep = -1;

        protected InfrastructureInstance firstResearchedInstance = null;
        public bool NPCsCanGetXP { get; private set; } = false;
        public Dictionary<TutorialCheck, bool> States { get; private set; } = new Dictionary<TutorialCheck, bool>();

        public TutorialEvent()
        {
            options.Add(new DialogButtonOption() { Text = "Continue", OnClick = () => Next() });
        }

        public override void Apply()
        {
            if (bossSprite == null)
            {
                NPCBase bossNPC = GameManager.Instance.AllNpcs.Find((npc) => npc.GetComponent<BossNPC>() != null);
                if (bossNPC == null)
                {
                    throw new System.Exception("Boss NPC not found");
                }

                bossSprite = bossNPC.GetComponent<SpriteRenderer>().sprite;
            }

            if (schematicalBot == null)
            {
                InfrastructureInstance server = GameManager.Instance.GetInfrastructureInstanceByID("server1");
                GameObject sGO = GameManager.Instance.prefabManager.Create("SchematicalBot",
                    server.transform.position + new Vector3(4, 0));
                schematicalBot = sGO.GetComponent<NPCSchematicalBot>();
            }

            if (botSprite == null)
            {
                botSprite = schematicalBot.GetComponent<SpriteRenderer>().sprite;
            }

            GameManager.Instance.UIManager.dailyProgressPanel.gameObject.SetActive(false);
            GameManager.Instance.GameLoopManager.playTimerActive = false;
            GameManager.OnInfrastructureStateChange += HandleInfrastructureStateChange;
            GameManager.OnTechnologyUnlocked += HandleTechnologyUnlocked;
            GameManager.OnPhaseChange += HandlePhaseChange;
            GameManager.OnReleaseChanged += HandleReleaseChange;
            nextStep = 0;
            Next();
        }


        private void Next()
        {
            if (nextStep < 0)
            {
                GameManager.Instance.cameraController.StopFollowing();
                return;
            }

            currentStep = nextStep;
            GameManager.Instance.UIManager.Resume();
            InfrastructureInstance infrastructureInstance;
            Transform transform;
            switch (currentStep)
            {
                case 0:
                    NPCBase bossNPC = GameManager.Instance.AllNpcs.Find((npc) => npc.GetComponent<BossNPC>() != null);
                    GameManager.Instance.cameraController.ZoomToAndFollow(bossNPC.transform);
                    GameManager.Instance.UIManager.ShowNPCDialog(
                        bossSprite,
                        "Hello! Welcome to the team. Your job is to keep the servers up and running fast so our startup can grow and make a profit. ",
                        new List<DialogButtonOption>()
                        {
                            new DialogButtonOption() { Text = "Start Tutorial", OnClick = () => Next() },
                            new DialogButtonOption() { Text = "Just Get Started", OnClick = () => End() },
                        }
                    );
                    nextStep = 1;
                    break;
// TODO Make basic screen movement display.
                case 1:
                    GameManager.Instance.HideAllAttentionIcons();
                    infrastructureInstance =
                        GameManager.Instance.GetInfrastructureInstanceByID("door");
                    GameManager.Instance.cameraController.ZoomTo(infrastructureInstance.transform);
                    GameManager.Instance.UIManager.ShowNPCDialog(
                        bossSprite,
                        "The team will enter via this door at the beginning of the day and exit at the end of the day. Click 'Start Day' to start your day",
                        options
                    );
                    nextStep = -1;
                    break;
                case 2:
                    NPCBase npc = GameManager.Instance.AllNpcs.Find((npc) => npc.GetComponent<NPCDevOps>() != null);
                    GameManager.Instance.cameraController.ZoomToAndFollow(npc.transform);
                    GameManager.Instance.UIManager.ShowNPCDialog(
                        bossSprite,
                        "You are in charge of a small team of software developers and DevOps engineers. Here is one now.",
                        options
                    );
                    nextStep = 3;
                    break;
                case 3:
                    GameManager.Instance.UIManager.ShowNPCDialog(
                        bossSprite,
                        "If you need help we hired a consultant to guide you. Allow me to introduce you to...",
                        options
                    );
                    nextStep = 4;
                    break;
                case 4:

                    GameManager.Instance.cameraController.ZoomTo(schematicalBot.transform);
                    GameManager.Instance.UIManager.ShowNPCDialog(
                        botSprite,
                        "Hi I am the consultant from Schematical and I am here to help guide you as you setup your cloud infrastructure.",
                        options
                    );
                    nextStep = 5;
                    break;
                case 5:
                    infrastructureInstance =
                        GameManager.Instance.GetInfrastructureInstanceByID("server1");
                    infrastructureInstance.ShowAttentionIcon();
                    GameManager.Instance.cameraController.ZoomTo(infrastructureInstance.transform);
                    GameManager.Instance.UIManager.ShowNPCDialog(
                        botSprite,
                        "Let's start by building a server so you can start handling some internet traffic. Do this by clicking on the server then selecting 'Build'. One of your DevOps Engineers will start building it shortly.",
                        options
                    );
                    nextStep = -1;
                    break;
                case 6:

                    GameManager.Instance.UIManager.ShowNPCDialog(
                        botSprite,
                        "To speed things up use the controls in the lower right hand side of the screen to manipulate in game time.",
                        options
                    );
                    nextStep = -1;
                    break;
                case 7:
                    infrastructureInstance =
                        GameManager.Instance.GetInfrastructureInstanceByID("internetPipe");
                    GameManager.Instance.cameraController.ZoomTo(infrastructureInstance.transform);
                    GameManager.Instance.UIManager.ShowNPCDialog(
                        botSprite,
                        "Great work! Notice Network Packets will start flowing in from the Internet to your server.",
                        options
                    );
                    nextStep = 8;
                    break;

                case 8:
                    NetworkPacket networkPacket =
                        GameManager.Instance.activePackets.Find((networkPacket) =>
                        {
                            return networkPacket.data.Type == NetworkPacketData.PType.Text;
                        });
                    if (networkPacket != null)
                    {
                        GameManager.Instance.cameraController.ZoomToAndFollow(networkPacket.transform);
                    }

                    GameManager.Instance.UIManager.ShowNPCDialog(
                        botSprite,
                        "Notice there are different network packet types. One type is just simple text like HTML.",
                        options
                    );
                    nextStep = 9;
                    break;

                case 9:
                    NetworkPacket networkPacket2 =
                        GameManager.Instance.activePackets.Find((networkPacket) =>
                        {
                            return networkPacket.data.Type == NetworkPacketData.PType.Image;
                        });
                    if (networkPacket2 != null)
                    {
                        GameManager.Instance.cameraController.ZoomToAndFollow(networkPacket2.transform);
                    }

                    GameManager.Instance.UIManager.ShowNPCDialog(
                        botSprite,
                        "Another type is binary data like images. Different NetworkPacket types will have different server load and effects on the various infrastructure and will take different routes as your cloud architecture evolves.",
                        options
                    );
                    nextStep = 10;
                    break;
                case 10:
                    infrastructureInstance =
                        GameManager.Instance.GetInfrastructureInstanceByID("server1");

                    GameManager.Instance.cameraController.ZoomTo(infrastructureInstance.transform);
                    GameManager.Instance.UIManager.ShowNPCDialog(
                        botSprite,
                        "Notice each Network Packet type has a different load that pops up when they are processed by the server.",
                        options
                    );
                    nextStep = 11;
                    break;
                case 11:
                    infrastructureInstance =
                        GameManager.Instance.GetInfrastructureInstanceByID("server1");
                    infrastructureInstance.SetState(InfrastructureData.State.Frozen);
                    infrastructureInstance.CurrentLoad =
                        infrastructureInstance.data.Stats.GetStatValue(StatType.Infra_MaxLoad);
                    GameManager.Instance.cameraController.ZoomTo(infrastructureInstance.transform);
                    GameManager.Instance.UIManager.ShowNPCDialog(
                        botSprite,
                        "If the load gets higher then the server can handle it will freeze. If that happens network requests will start to fail. This is bad.",
                        options
                    );
                    nextStep = 12;
                    break;
                case 12:
                    infrastructureInstance =
                        GameManager.Instance.GetInfrastructureInstanceByID("server1");
                    GameManager.Instance.cameraController.ZoomTo(infrastructureInstance.transform);
                    GameManager.Instance.UIManager.ShowNPCDialog(
                        botSprite,
                        "Until you research technology that monitors the servers you will need to manually tell your DevOps Engineers to fix the frozen infrastructure. Click on the server and select 'Fix' to assign your team to bring it back online.",
                        options
                    );
                    nextStep = -1;
                    break;

                case 13:

                    GameManager.Instance.UIManager.ShowNPCDialog(
                        botSprite,
                        "The server is back online. Great work! ",
                        options
                    );
                    nextStep = 14;
                    break;

                case 14:
                    infrastructureInstance =
                        GameManager.Instance.GetInfrastructureInstanceByID("server1");
                    GameManager.Instance.cameraController.ZoomTo(infrastructureInstance.transform);
                    GameManager.Instance.UIManager.ShowNPCDialog(
                        botSprite,
                        "You can increase the servers stats by increasing the instance size. Click on the server to do this.",
                        options
                    );
                    nextStep = -1;
                    break;
                case 15:
                    GameManager.Instance.UIManager.ShowNPCDialog(
                        botSprite,
                        "Great work. Just remember increasing the server size also increases its cost.",
                        options
                    );
                    nextStep = 16;
                    break;
                case 16:
                    infrastructureInstance =
                        GameManager.Instance.GetInfrastructureInstanceByID("desk");
                    infrastructureInstance.ShowAttentionIcon();
                    GameManager.Instance.cameraController.ZoomTo(infrastructureInstance.transform);
                    GameManager.Instance.UIManager.ShowNPCDialog(
                        botSprite,
                        "Running everything on one server isn't going to work for long so lets assign your team to start researching technology to help you on your journey. Click on the Desk or the 'Tech' button in the left hand sidebar",
                        options
                    );
                    nextStep = -1;
                    break;
                case 17:
                    firstResearchedInstance =
                        GameManager.Instance.ActiveInfrastructure.Find((instance =>
                            instance.data.CurrentState == InfrastructureData.State.Unlocked));
                    firstResearchedInstance.ShowAttentionIcon();
                    GameManager.Instance.cameraController.ZoomTo(firstResearchedInstance.transform);
                    firstTechnologyResearched = true;
                    GameManager.Instance.UIManager.ShowNPCDialog(
                        botSprite,
                        "Congrats! You researched your first Technology. Notice new Infrastructure is available to be built. You will want to assign your team to build it.",
                        options
                    );
                    nextStep = -1;
                    break;
                case 18:
                    infrastructureInstance =
                        GameManager.Instance.GetInfrastructureInstanceByID("server1");
                    GameManager.Instance.cameraController.ZoomTo(infrastructureInstance.transform);
                    GameManager.Instance.UIManager.ShowNPCDialog(
                        botSprite,
                        "Well done! Notice the load costs of certain packets have gone down even further on the server you built earlier. \nThis is because some of the load has been transferred to the hardware you just built.\n Research and build more to keep up with demand.",
                        options
                    );
                    nextStep = 19;
                    break;
                case 19:
                    NPCsCanGetXP = true;
                    npc = GameManager.Instance.AllNpcs.Find((npc) => npc.GetComponent<NPCDevOps>() != null);
                    NPCDevOps devOps = npc.GetComponent<NPCDevOps>();
                    devOps.AddXP(91);
                    GameManager.Instance.cameraController.ZoomToAndFollow(npc.transform);
                    GameManager.Instance.UIManager.SetTimeScalePause();
                    GameManager.Instance.UIManager.ShowNPCDialog(
                        botSprite,
                        "One of your team members has leveled up. Choose a new trait to give them. Each trait comes with unique bonuses.",
                        options
                    );
                    nextStep = -1;
                    break;
                case 20:
                    infrastructureInstance =
                        GameManager.Instance.GetInfrastructureInstanceByID("whiteboard");
                    infrastructureInstance.ShowAttentionIcon();
                    GameManager.Instance.cameraController.ZoomTo(infrastructureInstance.transform);
                    GameManager.Instance.UIManager.ShowNPCDialog(
                        botSprite,
                        "Great work. Another way you can level up your gameplay is to do releases. Select a release focus by clicking on the whiteboard.",
                        options
                    );
                    nextStep = -1;
                    break;
                case 21:
                    infrastructureInstance =
                        GameManager.Instance.GetInfrastructureInstanceByID("kanbanboard");
                    infrastructureInstance.ShowAttentionIcon();
                    GameManager.Instance.cameraController.ZoomTo(infrastructureInstance.transform);
                    GameManager.Instance.UIManager.ShowNPCDialog(
                        botSprite,
                        "Your team will now work on coding the next release when higher priority tasks are not available.\n" +
                        "You can adjust the priority of tasks by clicking on the Kanban Board.",
                        options
                    );
                    nextStep = -1;
                    break;
                case 22:
                    GameManager.Instance.GetInfrastructureInstanceByID("kanbanboard").HideAttentionIcon();
                    infrastructureInstance =
                        GameManager.Instance.GetInfrastructureInstanceByID("server1");
                    GameManager.Instance.cameraController.ZoomTo(infrastructureInstance.transform);
                    GameManager.Instance.UIManager.ShowNPCDialog(
                        botSprite,
                        "Your first release is ready to be deployed. Click on all available Application Servers to trigger up the deployment.\n" +
                        "Use Meta Challenges to unlock technologies that will automate this for you in the future.",
                        options
                    );
                    nextStep = -1;
                    break;
                case 23:
                    GameManager.Instance.UIManager.ShowNPCDialog(
                        botSprite,
                        "Well done. Now you will receive the rewards that release unlocked. \n" +
                        "You can only focus on one release at a time right now.",
                        options
                    );
                    nextStep = 24;
                    break;
                case 24:

                    NPCBug bugGO = GameObject.FindObjectsOfType<NPCBug>().First();
                    GameManager.Instance.cameraController.ZoomToAndFollow(bugGO.transform);
                    GameManager.Instance.UIManager.ShowNPCDialog(
                        botSprite,
                        "A bug was introduced in the last release \n" +
                        "You can choose to debug it now or leave it for now. \n" +
                        "Be careful though, bugs left in production have consequences.",
                        options
                    );
                    nextStep = 25;
                    break;
                case 25:
                    bossNPC = GameManager.Instance.AllNpcs.Find((npc) => npc.GetComponent<BossNPC>() != null);
                    GameManager.Instance.cameraController.ZoomTo(bossNPC.transform);
                    GameManager.Instance.UIManager.ShowNPCDialog(
                        bossSprite,
                        "It looks like you are settling right in. \n Now that you have the basics we are going to start the in game clock. At the end of each day you will get a summary. \n Our infrastructure budget is directly related to how many packets make it through. If we run out of money its Game Over...",
                        options
                    );
                    nextStep = 26;
                    break;
                case 26:
                    End();
                    nextStep = -1;
                    break;
                //TODO: Explain the economy.
            }
        }

        private void HandleInfrastructureStateChange(InfrastructureInstance instance,
            InfrastructureData.State? previousState)
        {
            Debug.Log("HandleInfrastructureBuilt Called: " + instance.data.ID + " - " + currentStep + " - Prev: " +
                      previousState + " -> Curr: " + instance.data.CurrentState);
            switch (currentStep)
            {
                case 5:
                    if (instance.data.CurrentState == InfrastructureData.State.Planned)
                    {
                        nextStep = 6;
                    }

                    Next();
                    break;
                case 6:
                    if (instance.IsActive())
                    {
                        nextStep = 7;
                        Next();
                    }

                    break;
                case 12:
                    if (instance.IsActive())
                    {
                        nextStep = 13;
                        Next();
                    }

                    break;
                case 14:
                    if (instance.IsActive() && instance.CurrentSizeLevel > 0)
                    {
                        nextStep = 15;
                        Next();
                    }

                    break;
                case 17:
                    if (
                        instance.data.ID == firstResearchedInstance.data.ID &&
                        instance.IsActive()
                    )
                    {
                        nextStep = 18;
                        Next();
                    }

                    break;
            }
        }

        private void HandleTechnologyUnlocked(Technology tech)
        {
            Debug.Log("HandleTechnologyUnlocked Called: " + tech.TechnologyID + " - " + firstTechnologyResearched);
            if (firstTechnologyResearched)
            {
                return;
            }

            nextStep = 17;
            Next();
        }

        public override bool IsPossible()
        {
            return true;
        }

        public virtual void End()
        {
           
            GameManager.Instance.UIManager.dailyProgressPanel.gameObject.SetActive(true);
            schematicalBot.gameObject.SetActive(false);
            GameManager.Instance.SetStat(StatType.PacketsSent, 0);
            GameManager.Instance.SetStat(StatType.PacketsServiced, 0);
            GameManager.Instance.SetStat(StatType.PacketsFailed, 0);

            GameManager.OnTechnologyUnlocked -= HandleTechnologyUnlocked;

            GameManager.OnInfrastructureStateChange -= HandleInfrastructureStateChange;

            GameManager.OnPhaseChange -= HandlePhaseChange;
            GameManager.OnReleaseChanged -= HandleReleaseChange;

            GameManager.Instance.cameraController.StopFollowing();
            if (currentStep < 2)
            {
                GameManager.Instance.GetInfrastructureInstanceByID("server1").ZoomTo();
            }
            GameManager.Instance.GameLoopManager.playTimerActive = true;

            base.End();
        }

        public void HandlePhaseChange(GameLoopManager.GameState state)
        {
            if (currentStep == 1)
            {
                nextStep = 2;
                Next();
            }
        }

        public void HandleReleaseChange(ReleaseBase releaseBase, ReleaseBase.ReleaseState prevState)
        {
            switch (currentStep)
            {
                case (20):
                    if (releaseBase.State == ReleaseBase.ReleaseState.InDevelopment)
                    {
                        nextStep = 21;
                        Next();
                    }

                    break;
                case (21):
                    if (releaseBase.State == ReleaseBase.ReleaseState.DeploymentReady)
                    {
                        nextStep = 22;
                        Next();
                    }

                    break;
                /*case(22):
                    if (releaseBase.State == ReleaseBase.ReleaseState.DeploymentCompleted)
                    {
                        nextStep = 23;
                        Next();
                    }
                    break;*/
            }
        }

        public override string GetEventDescription()
        {
            return $"{base.GetEventDescription()} - Step: {currentStep} - Next: {nextStep}";
        }

        public void Check(TutorialCheck check)
        {
            bool checkValue = false;
            if (States.ContainsKey(check))
            {
                checkValue = States[check];
            }

            switch (check)
            {
                case (TutorialCheck.NPC_AddTrait):
                    if (!checkValue)
                    {
                        nextStep = 20;
                        States[check] = true;
                        Next();
                    }

                    break;
                default:
                    throw new SystemException($"No idea what this check is {check}");
            }
        }


/*
        public virtual bool IsOver()
        {
            return true;
        }*/
        public void OnRewardsPanelDone()
        {
            nextStep = 23;
            Next();
        }
    }
}