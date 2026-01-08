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
            Next();
           
       
        }

        private void Next()
        {
            Debug.Log("Next step: " + currentStep);
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
                    InfrastructureInstance infrastructureInstance =
                        GameManager.Instance.GetInfrastructureInstanceByID("server1");
                    GameManager.Instance.cameraController.ZoomTo(infrastructureInstance.transform);
                    GameManager.Instance.UIManager.ShowNPCDialog(
                        botSprite,
                        "Let's start by building a server so you can start handling some internet traffic. Do this by clicking on the server then selecting 'Plan Build'",
                        options
                    );
                    break;
                case 4:

                    break;
                case 5:
                    GameManager.Instance.UIManager.ShowNPCDialog(
                        botSprite,
                        "Great work! Running everything on one server isn't going to work for long so lets assign your team to start researching technology to help you on your journey. Click on the Desk or the 'Tech' button in the left hand sidebar",
                        options
                    );
                    break;
                case 6:

                    break;
                case 7:
                    GameManager.Instance.UIManager.ShowNPCDialog(
                        botSprite,
                        "Congrats! You researched your first Technology. Notice new Infrastructure is available to be built. You will want to assign your team to build it.",
                        options
                    );
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
            if (currentStep != 7)
            {
                return;
            }
            Next();
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