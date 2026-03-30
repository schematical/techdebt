using System.Collections.Generic;
using NPCs;
using UnityEngine;

namespace Tutorial.Steps
{
    public class FirstTutorialStep: TutorialStep
    {
        public FirstTutorialStep(TutorialStepId id, string name, string description) : base(id, name, description)
        {
            onTrigger = () =>
            {
                GameManager.Instance.HideAllAttentionIcons();
                
            };
        }

        public override List<DialogButtonOption> GetDialogOptions()
        {
            return new List<DialogButtonOption>()
            {
                new DialogButtonOption() { Text = "Start Tutorial", OnClick = () =>
                    {
                        Next();
                    } 
                },
                new DialogButtonOption() { Text = "Just Get Started", OnClick = () =>
                    {
                        
                        
                        GameManager.Instance.HireNPCDevOps(new NPCDevOpsData { DailyCost = 100 });
                        GameManager.Instance.GameLoopManager.BeginPlanPhase();
                        GameManager.Instance.TutorialManager.MarkPassive();
                        
                    }
                },
            };
        }
    }
}