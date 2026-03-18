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
                    "You are in charge of a small team of software developers and DevOps engineers. Here is one now."
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
                    TutorialStepId.Technology_ApplicationServer,
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
                    TutorialStepId.Infra_ApplicationServer,
                    "Server Built",
                    "This is the Application Server. " + 
                    "It will receive Network Packets coming from the internet, process them, and send back a response to whoever sent the request on the internet."
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
            TutorialStep step = GetStep(TutorialStepId.NPC_Boss);
            GameManager.OnInfrastructureStateChange += HandleInfrastructureStateChange;
            GameManager.OnTechnologyUnlocked += HandleTechnologyUnlocked;
            GameManager.OnPhaseChange += HandlePhaseChange;
            GameManager.OnReleaseChanged += HandleReleaseChange;
            step.Trigger();
        }


        public void End()
        {
            GameManager.OnInfrastructureStateChange -= HandleInfrastructureStateChange;
            GameManager.OnTechnologyUnlocked -= HandleTechnologyUnlocked;
            GameManager.OnPhaseChange -= HandlePhaseChange;
            GameManager.OnReleaseChanged -= HandleReleaseChange;
        }

        private void HandleTechnologyUnlocked(Technology technology)
        {
            switch (technology.TechnologyID)
            {
                case("application-server"):
                    Trigger(TutorialStepId.Technology_ApplicationServer);
                    break;
                default:
                    Debug.LogError("Not Implemented");
                    break;
            }
        }
        private void HandleInfrastructureStateChange(InfrastructureInstance infrastructureInstance, InfrastructureData.State? previousState)
        {
            switch (infrastructureInstance.data.worldObjectType)
            {
                case(WorldObjectType.Type.ApplicationServer):
                    if(
                        previousState == InfrastructureData.State.Planned && 
                        infrastructureInstance.data.CurrentState  == InfrastructureData.State.Operational
                    )
                    {
                        Trigger(TutorialStepId.Infra_ApplicationServer);
                    }
                break;
            }
        }

        private void HandleReleaseChange(ReleaseBase arg1, ReleaseBase.ReleaseState arg2)
        {
            // throw new System.NotImplementedException();
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
    }
}