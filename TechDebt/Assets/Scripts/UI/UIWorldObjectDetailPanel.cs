using System.Collections.Generic;
using System.Linq;
using Infrastructure;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class UIWorldObjectDetailPanel: UIPanel
    {
        private WorldObjectBase _selectedWorldObject;
        public UITextArea textArea;
        private List<Button> _taskButtons = new List<Button>();
    

        /*void Start()
        {

        
        }*/
        void Update()
        {
            textArea.textArea.text = _selectedWorldObject.GetDetailText();
        }
        public void ShowWorldObjectDetail(WorldObjectBase worldObject)
        {
            _selectedWorldObject = worldObject;
            gameObject.SetActive(true);
    

            // Cleanup previous task buttons
            foreach (var button in _taskButtons)
            {
                Destroy(button.gameObject);
            }
            _taskButtons.Clear();

            List<NPCTask> tasks = _selectedWorldObject.GetAvailableTasks();
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
            
        }

        public void Close()
        {
            _selectedWorldObject = null;
            gameObject.SetActive(false);
        }
    }
}