using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UI
{
    public class UITaskListPanel: UIPanel
    {   // Task List
        private List<UITaskListPanelItem> _taskListPanelItems = new List<UITaskListPanelItem>();

        public override void Show()
        {

            base.Show();



            foreach (UITaskListPanelItem taskListPanelItem in _taskListPanelItems)
            {
                Destroy(taskListPanelItem);
                _taskListPanelItems.Remove(taskListPanelItem);
            }

            List<NPCTask> currentTasks =
                GameManager.Instance.AvailableTasks.FindAll(t => t.CurrentState != NPCTask.State.Completed)
                    .OrderByDescending(t => t.Priority)
                    .ToList();


            for (int i = 0; i < currentTasks.Count; i++)
            {
                NPCTask task = currentTasks[i];
                UITaskListPanelItem item = GameManager.Instance.prefabManager
                    .Create("UITaskListPanelItem", Vector3.zero, scrollContent.transform)
                    .GetComponent<UITaskListPanelItem>();
                item.Initialize(task, i, currentTasks.Count);
                _taskListPanelItems.Add(item);

            }
        }

    }
}