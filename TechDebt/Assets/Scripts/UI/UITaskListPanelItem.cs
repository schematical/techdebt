using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class UITaskListPanelItem: MonoBehaviour
    {
        public NPCTask npcTask;
        public TextMeshProUGUI primaryText;
        public Button upButton;
        public Button downButton;
        void Start()
        {
            upButton.onClick.AddListener(() =>
            {
                GameManager.Instance.IncreaseTaskPriority(npcTask);
                GameManager.Instance.UIManager.taskListPanel.Refresh();
            });
            downButton.onClick.AddListener(() =>
            {
                GameManager.Instance.DecreaseTaskPriority(npcTask);
                GameManager.Instance.UIManager.taskListPanel.Refresh();
            });
        }
        public void Initialize(NPCTask task, int sortOrder, int totalTaskCount)
        {
            
           npcTask = task;
           upButton.interactable = (sortOrder > 0);

      
           downButton.interactable = (sortOrder < totalTaskCount - 1);

        }

        void Update()
        {
            if (npcTask == null)
            {
                gameObject.SetActive(false);
            }
            string statusColor = npcTask.CurrentState == NPCTask.State.Executing ? "yellow" : "white";
            string assignee = npcTask.AssignedNPC != null ? npcTask.AssignedNPC.name : "Unassigned";
            string taskText = $"<b>{npcTask.GetType().Name}</b> ({npcTask.GetDescription()})\n";
            
            taskText += $"<color={statusColor}>Status: {npcTask.CurrentState}</color> | Assignee: {assignee}";
            primaryText.text = taskText;
   
        }
    }
}