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

            GameManager.OnInfrastructureBuilt += HandleInfrastructureBuilt;
            GameManager.OnTechnologyUnlocked += HandleTechnologyUnlocked;
            GameManager.OnDayEnd += OnDayEnd;
            Next();
           
       
        }

        private void OnDayEnd()
        {
            Debug.Log("OnDayEnd Called: " + GameManager.Instance.GameLoopManager.currentDay + " - " + currentStep);
            if (GameManager.Instance.GameLoopManager.currentDay == 1)
            {
                InfrastructureInstance infrastructureInstance =
                    GameManager.Instance.GetInfrastructureInstanceByID("door");
                GameManager.Instance.cameraController.ZoomTo(infrastructureInstance.transform);
                GameManager.Instance.UIManager.ShowNPCDialog(
                    bossSprite,
                    "That's the end of your first day. The team will exit via the door portal at the end of each day and re-enter at the beginning of the next day.",
                    options
                );
            }
        }

        private void Next()
        {
            Debug.Log("Next step: " + currentStep);
            InfrastructureInstance infrastructureInstance;
            switch (currentStep)
            {
                case 0:
                    GameManager.Instance.UIManager.ShowNPCDialog(
                        bossSprite,
                        "Hello! Welcome to the team. Your job is to keep the servers up and running fast our startup can grow and make a profit.",
                        options
                    );
                    break;
                case 1:
                    GameManager.Instance.UIManager.ShowNPCDialog(
                        bossSprite,
                        "If you need help we hired a consultant to guide you. Allow me to introduce you to...",
                        options
                    );
                    break;
                case 2:
                    GameManager.Instance.UIManager.ShowNPCDialog(
                        botSprite,
                        "Hi I am the 'Schematica-bot' from Schematical and I am here to help guide you as you setup your cloud infrastructure.",
                        options
                    );
                    break;
                case 3: 
                    infrastructureInstance =
                        GameManager.Instance.GetInfrastructureInstanceByID("server1");
                    GameManager.Instance.cameraController.ZoomTo(infrastructureInstance.transform);
                    GameManager.Instance.UIManager.ShowNPCDialog(
                        botSprite,
                        "Let's start by building a server so you can start handling some internet traffic. Do this by clicking on the server then selecting 'Plan Build'. One of your DevOps Engineers will start building it shortly.",
                        options
                    );
                    break;
                case 4:

                    break;
                case 5:
                     infrastructureInstance =
                        GameManager.Instance.GetInfrastructureInstanceByID("internetPipe");
                    GameManager.Instance.cameraController.ZoomTo(infrastructureInstance.transform);
                    GameManager.Instance.UIManager.ShowNPCDialog(
                        botSprite,
                        "Great work! Notice Network Packets will start flowing in from the Internet to your server.",
                        options
                    );
                    break;
                case 6:
                     infrastructureInstance =
                        GameManager.Instance.GetInfrastructureInstanceByID("desk");
                     GameManager.Instance.cameraController.ZoomTo(infrastructureInstance.transform);
                    GameManager.Instance.UIManager.ShowNPCDialog(
                        botSprite,
                        "Running everything on one server isn't going to work for long so lets assign your team to start researching technology to help you on your journey. Click on the Desk or the 'Tech' button in the left hand sidebar",
                        options
                    );
                    break;
                case 7:

                    break;
               
            }

            currentStep++;
        }

        private void HandleInfrastructureBuilt(InfrastructureInstance instance)
        {
            Debug.Log("HandleInfrastructureBuilt Called: " + instance.data.ID + " - " + currentStep);
            if (currentStep != 5)
            {
                return;
            }
            Next();
        }
        private void HandleTechnologyUnlocked(Technology tech)
        {
            Debug.Log("HandleTechnologyUnlocked Called: " + tech.TechnologyID + " - " + firstTechnologyResearched);
            if (firstTechnologyResearched)
            {
                return;
            }

            InfrastructureInstance infrastructureInstance =
                GameManager.Instance.ActiveInfrastructure.Find((instance =>
                    instance.data.CurrentState == InfrastructureData.State.Unlocked));
                
            GameManager.Instance.cameraController.ZoomTo(infrastructureInstance.transform);
            firstTechnologyResearched = true;
            GameManager.Instance.UIManager.ShowNPCDialog(
                botSprite,
                "Congrats! You researched your first Technology. Notice new Infrastructure is available to be built. You will want to assign your team to build it.",
                options
            );
        }

        public override bool IsPossible()
        {
            return true;
        }
        
        public virtual void End()
        {
            GameManager.OnTechnologyUnlocked -= HandleTechnologyUnlocked;
        }
/*
        public virtual bool IsOver()
        {
            return true;
        }*/
    }

  
}