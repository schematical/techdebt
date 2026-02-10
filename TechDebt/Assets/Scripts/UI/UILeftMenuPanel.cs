using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class UILeftMenuPanel:  UIPanel
    {

        protected override void Start()
        {

            base.Start();
            titleText.text = "";

            AddButton("Tasks", GameManager.Instance.UIManager.taskListPanel.Show).gameObject.AddComponent<LayoutElement>().preferredHeight = 40;
            AddButton("Tech", GameManager.Instance.UIManager.techTreePanel.Show).gameObject.AddComponent<LayoutElement>().preferredHeight = 40;
            AddButton("NPCs", GameManager.Instance.UIManager.npcListPanel.Show).gameObject.AddComponent<LayoutElement>().preferredHeight = 40;
            // AddButton("Events", GameManager.Instance.UIManager.ToggleEventLogPanel).gameObject.AddComponent<LayoutElement>().preferredHeight = 40;
            AddButton("Releases", GameManager.Instance.UIManager.ToggleDeploymentHistoryPanel).gameObject.AddComponent<LayoutElement>().preferredHeight = 40;
            AddButton("Stats", GameManager.Instance.UIManager.globalStatsPanel.Show).gameObject.AddComponent<LayoutElement>().preferredHeight = 40;
        }
    }
}