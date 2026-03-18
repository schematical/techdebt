using System.Collections.Generic;
using System.IO;
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
                    spriteId = "Suit1NPC"
                },
                new TutorialStep(
                    TutorialStepId.Infra_Door,
                    "Door",
                    "The team will enter via this door at the beginning of the day and exit at the end of the day. Click 'Start Day' to start your day"
                )
                {
                    spriteId = "portal-door"
                },
                new TutorialStep(
                    TutorialStepId.Infra_Desk,
                    "Desk",
                    "This is where your team members will do research tasks. Click on it to begin researching."
                )
                {
                    spriteId = "Desk"
                },
                new TutorialStep(
                    TutorialStepId.Infra_ApplicationServer,
                    "Application Server",
                    "This is the Application Server. " + 
                        "It will receive Network Packets coming from the internet, process them, and send back a response to whoever sent the request on the internet."
                )
                {
                    spriteId = "server1"
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
            step.Trigger();
        }
        public void End()
        {
            // ???
        }

        public void Next(TutorialStepId nextStepId)
        {
            if (nextStepId == null)
            {
                return;
            }
            TutorialStep tutorialStep = GetStep(nextStepId);
            tutorialStep.Trigger();
        }
    }
}