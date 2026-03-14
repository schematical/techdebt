using System.Collections.Generic;
using NPCs;
using Stats;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class UINPCDetailPanel: UIPanel
    {
        public Button followButton;
        private NPCBase _selectedNPC;
        // public UITextArea textArea; // Removed to use AddLine instead
        private List<UIPanelLineSectionButton> _taskButtons = new List<UIPanelLineSectionButton>();

        void Start()
        {
            followButton.onClick.AddListener(() =>
            {
                GameManager.Instance.UIManager.Close();
                GameManager.Instance.cameraController.StartFollowing(_selectedNPC.transform);
            });
         
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

            // Task
            if (_selectedNPC.CurrentTask != null)
            {
                AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = $"Task: {_selectedNPC.CurrentTask.GetDescription()}";
            }
            else
            {
                AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = "Task: None";
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
        }

        protected override void Update()
        {
            base.Update();
            if (_selectedNPC == null) return;

            // Removed textArea update logic
        }
        public override void Close(bool forceClose = false)
        {
            // _selectedNPC = null;
            base.Close(forceClose);
        }

        public void Preview(RewardBase modifierBase, NPCDevOps npc)
        {
            Show(npc);
            Debug.Log("TODO: Preview");
        }
    }
}
