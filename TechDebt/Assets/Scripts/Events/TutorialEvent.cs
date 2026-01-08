using System.Collections.Generic;
using UnityEngine;

namespace Events
{
    public class TutorialEvent: EventBase
    {
        protected int currentStep = 0;
        protected List<DialogButtonOption> options = new List<DialogButtonOption>();
        
        protected Sprite bossSprite;
        protected Sprite botSprite;

        protected bool firstTechnologyResearched = false;
        protected int nextStep = -1;
        public TutorialEvent()
        {
            
            options.Add(new DialogButtonOption() { Text = "Continue", OnClick = ()=> Next() });
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

            if (botSprite == null)
            {
                botSprite = GameManager.Instance.prefabManager.GetPrefab("SchematicaBot").GetComponent<SpriteRenderer>()
                    .sprite;
            }

            GameManager.Instance.GameLoopManager.playTimerActive = false;
            GameManager.OnInfrastructureBuilt += HandleInfrastructureBuilt;
            GameManager.OnTechnologyUnlocked += HandleTechnologyUnlocked;
            GameManager.OnPhaseChange += HandlePhaseChange;
            nextStep = 0;
            Next();
           
       
        }

       
        private void Next()
        {
            Debug.Log("Next step: " + currentStep + " nextStep: " + nextStep);
            if (nextStep < 0)
            {
                return;
            }
            currentStep =  nextStep;
            GameManager.Instance.UIManager.SetTimeScalePlay();
            InfrastructureInstance infrastructureInstance;
            Transform transform;
            switch (currentStep)
            {
                case 0:
                    NPCBase bossNPC = GameManager.Instance.AllNpcs.Find((npc) => npc.GetComponent<BossNPC>() != null);
                    GameManager.Instance.cameraController.ZoomToAndFollow(bossNPC.transform);
                    GameManager.Instance.UIManager.ShowNPCDialog(
                        bossSprite,
                        "Hello! Welcome to the team. Your job is to keep the servers up and running fast so our startup can grow and make a profit.",
                        options
                    );
                    nextStep = 1;
                    break;
                
                case 1:
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
                    infrastructureInstance =
                        GameManager.Instance.GetInfrastructureInstanceByID("server1");
                    GameManager.Instance.cameraController.ZoomTo(infrastructureInstance.transform);
                    GameManager.Instance.UIManager.ShowNPCDialog(
                        botSprite,
                        "Hi I am the 'Schematica-bot' from Schematical and I am here to help guide you as you setup your cloud infrastructure.",
                        options
                    );
                    nextStep = 5;
                    break;
                case 5: 
               
                    GameManager.Instance.UIManager.ShowNPCDialog(
                        botSprite,
                        "Let's start by building a server so you can start handling some internet traffic. Do this by clicking on the server then selecting 'Plan Build'. One of your DevOps Engineers will start building it shortly.",
                        options
                    );
                    nextStep = -1;
                    break;
                case 6:

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
                     infrastructureInstance =
                        GameManager.Instance.GetInfrastructureInstanceByID("desk");
                     GameManager.Instance.cameraController.ZoomTo(infrastructureInstance.transform);
                    GameManager.Instance.UIManager.ShowNPCDialog(
                        botSprite,
                        "Running everything on one server isn't going to work for long so lets assign your team to start researching technology to help you on your journey. Click on the Desk or the 'Tech' button in the left hand sidebar",
                        options
                    );
                    nextStep = -1;
                    break;
                case 9:
                     infrastructureInstance =
                        GameManager.Instance.ActiveInfrastructure.Find((instance =>
                            instance.data.CurrentState == InfrastructureData.State.Unlocked));
                
                    GameManager.Instance.cameraController.ZoomTo(infrastructureInstance.transform);
                    firstTechnologyResearched = true;
                    GameManager.Instance.UIManager.ShowNPCDialog(
                        botSprite,
                        "Congrats! You researched your first Technology. Notice new Infrastructure is available to be built. You will want to assign your team to build it.",
                        options
                    );
                    nextStep = 10;
                    break;
                case 10:
                    transform =
                        GameManager.Instance.activePackets.Find((networkPacket) =>
                        {
                            return networkPacket.data.Type == NetworkPacketData.PType.Text;
                            
                        }).transform;
                
                    GameManager.Instance.cameraController.ZoomToAndFollow(transform);
                    firstTechnologyResearched = true;
                    GameManager.Instance.UIManager.ShowNPCDialog(
                        botSprite,
                        "Notice there are different network packet types. One type is just simple text like HTML.",
                        options
                    );
                    nextStep = 11;
                    break;
                
                case 11:
                     transform =
                        GameManager.Instance.activePackets.Find((networkPacket) =>
                        {
                            return networkPacket.data.Type == NetworkPacketData.PType.Image;
                            
                        }).transform;
                
                    GameManager.Instance.cameraController.ZoomToAndFollow(transform);
                    firstTechnologyResearched = true;
                    GameManager.Instance.UIManager.ShowNPCDialog(
                        botSprite,
                        "Another type is binary data like images. Different NetworkPacket types will have different server load and effects on the various infrastructure and will take different routes as your cloud architecture evolves.",
                        options
                    );
                    nextStep = 12;
                    break;
                case 12:
                    infrastructureInstance =
                        GameManager.Instance.GetInfrastructureInstanceByID("server1");
                
                    GameManager.Instance.cameraController.ZoomToAndFollow(infrastructureInstance.transform);
                    firstTechnologyResearched = true;
                    GameManager.Instance.UIManager.ShowNPCDialog(
                        botSprite,
                        "Notice each Network Packet type has a different load that pops up when they are processed by the server.",
                        options
                    );
                    nextStep = -1;
                    break;
               
            }

   
        }

        private void HandleInfrastructureBuilt(InfrastructureInstance instance)
        {
            Debug.Log("HandleInfrastructureBuilt Called: " + instance.data.ID + " - " + currentStep);
            if (currentStep != 5)
            {
                return;
            }

            nextStep = 7;
            Next();
        }
        private void HandleTechnologyUnlocked(Technology tech)
        {
            Debug.Log("HandleTechnologyUnlocked Called: " + tech.TechnologyID + " - " + firstTechnologyResearched);
            if (firstTechnologyResearched)
            {
                return;
            }

            nextStep = 9;
            Next();

        }

        public override bool IsPossible()
        {
            return true;
        }
        
        public virtual void End()
        {
            GameManager.OnTechnologyUnlocked -= HandleTechnologyUnlocked;
            GameManager.OnInfrastructureBuilt -= HandleInfrastructureBuilt;
            GameManager.OnPhaseChange -= HandlePhaseChange;
        }

        public void HandlePhaseChange(GameLoopManager.GameState state)
        {
            if (currentStep == 1)
            {
                nextStep = 2;
                Next();
            }
        }

        public override string GetEventDescription()
        {
            return $"{base.GetEventDescription()} - Step: {currentStep} - Next: {nextStep}";
        }
/*
        public virtual bool IsOver()
        {
            return true;
        }*/
    }

  
}