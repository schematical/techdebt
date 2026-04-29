using System;
using System.Collections.Generic;
using NPCs;
using Stats;
using TMPro;
using Tutorial;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class UINPCDetailPanel: UIPanel
    {
        private NPCBase _selectedNPC;
        // public UITextArea textArea; // Removed to use AddLine instead
        private List<UIPanelLineSectionButton> _taskButtons = new List<UIPanelLineSectionButton>();
        private UIPanelLineSectionText tasksLineText;
        private UIPanelLineSectionText levelLineText;
        private UIPanelLineSectionText xpLineText;
        void Start()
        {
     
         
        }
        public void Show(NPCBase npc)
        {
   
            base.Show();
            _selectedNPC = npc;
       
            
            CleanUp(); // Clear existing lines

            List<NPCTask> tasks = _selectedNPC.GetAvailableTasks();
            foreach (NPCTask task in tasks)
            {
                NPCTask localTask = task; // Local copy for the closure
                UIPanelButton newButton = AddButton(task.GetAssignButtonText(), () =>
                {
                    GameManager.Instance.AddTask(localTask);
                    Close();
                });
            
            }
            
            // Name
            AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = _selectedNPC.name;

            // State
            AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = $"State: {_selectedNPC.CurrentState}";
            tasksLineText = AddLine<UIPanelLine>().Add<UIPanelLineSectionText>();
            if (_selectedNPC is NPCDevOps)
            {
                NPCDevOps npcDevOps = (NPCDevOps)_selectedNPC;
                UIPanelLine levelLine = AddLine<UIPanelLine>();
                levelLineText = levelLine.Add<UIPanelLineSectionText>();
                UIPanelLine xpLine = AddLine<UIPanelLine>();
                xpLineText = xpLine.Add<UIPanelLineSectionText>();
                
            }
            else
            {
                levelLineText = null;
                xpLineText = null;
            }
         
            // Stats
            UIStatCollectionPanelLine statsLine = AddLine<UIStatCollectionPanelLine>();
            statsLine.SetStatCollection(_selectedNPC.Stats, "Stats");
            statsLine.SetId("Stats");

            // Cooldowns
            UIPanelLine cooldownsLine = AddLine<UIPanelLine>();
            cooldownsLine.Add<UIPanelLineSectionText>().text.text = "Cool Downs:";
            cooldownsLine.SetExpandable((line) =>
            {
                foreach (KeyValuePair<NPCBase.CoolDownType, float> coolDown in _selectedNPC.GetCoolDowns())
                {
                    UIPanelLine detailLine = line.AddLine<UIPanelLine>();
                    detailLine.Add<UIPanelLineSectionText>().text.text = $"{coolDown.Key}: {coolDown.Value:F2}";
                }
            });
            if (_selectedNPC is NPCDevOps)
            {
                NPCDevOps npcDevOps = (NPCDevOps)_selectedNPC;
                UIPanelLine traitsLine = AddLine<UIPanelLine>();
                traitsLine.Add<UIPanelLineSectionText>().text.text = "Traits:";
                traitsLine.SetExpandable((line) =>
                {
                    foreach (RewardBase rewardBase in npcDevOps.Modifiers.Rewards)
                    {
                        UIPanelLine detailLine = line.AddLine<UIPanelLine>();
                        rewardBase.Render(detailLine); // .Add<UIPanelLineSectionText>().text.text = $"{}");
                    }
                });
            }

            TutorialStepId? tutorialStepId = npc.GetTutorialStepId();
            if (tutorialStepId != null && tutorialStepId != TutorialStepId.None)
            {
                AddButton("Learn More", () => GameManager.Instance.TutorialManager.ForceRender(tutorialStepId.Value));
            }
            AddButton("Follow", () =>
            {
                GameManager.Instance.UIManager.Close();
                GameManager.Instance.cameraController.StartFollowing(_selectedNPC.transform);
            });
        }

        protected override void Update()
        {
            base.Update();
            if (_selectedNPC == null) return;
            
            if (_selectedNPC.CurrentTask != null)
            {
                tasksLineText.text.text = $"Task: {_selectedNPC.CurrentTask.GetDescription()}";
            }
            else
            {
                tasksLineText.text.text = "Task: None";
            }
            if (_selectedNPC is NPCDevOps)
            {
                // This is a bit hacky
                NPCDevOps npcDevOps = (NPCDevOps)_selectedNPC;
                levelLineText.text.text = $"Level: {npcDevOps.level} (Leveled Up To: {npcDevOps.leveledUpTo})";
                xpLineText.text.text = $"XP:{Math.Round(npcDevOps.currentXP)} - Next Level At: {npcDevOps.GetNextLevelXP()}";
            }

        }
        
    }
}
