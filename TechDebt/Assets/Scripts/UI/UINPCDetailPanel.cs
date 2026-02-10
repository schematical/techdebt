using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class UINPCDetailPanel: UIPanel
    {
        public Button followButton;
        private NPCBase _selectedNPC;
        public UITextArea textArea;
        private List<Button> _taskButtons = new List<Button>();

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
            // Cleanup previous task buttons
            foreach (var button in _taskButtons)
            {
                Destroy(button.gameObject);
            }
            _taskButtons.Clear();

            List<NPCTask> tasks = _selectedNPC.GetAvailableTasks();
            foreach (NPCTask task in tasks)
            {
                NPCTask localTask = task; // Local copy for the closure
                UIButton newButton = AddButton(task.GetAssignButtonText(), () =>
                {
                    GameManager.Instance.AddTask(localTask);
                    Close();
                });
                _taskButtons.Add(newButton.button);
            }
            
            textArea.transform.SetAsLastSibling();
        }

        protected override void Update()
        {
            base.Update();
            if (_selectedNPC == null) return;

            if (textArea == null)
            {
                Debug.LogError("_npcDetailText is not assigned. Cannot update NPC Detail Panel.");
                return;
            }
           
            textArea.textArea.text = _selectedNPC.GetDetailText();
        }
        public override void Close(bool forceClose = false)
        {
            // _selectedNPC = null;
            base.Close(forceClose);
        }
    }
}