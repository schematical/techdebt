using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class UILeftMenuPanel:  UIPanel
    {

        void Start()
        {
       

            titleText.text = "";
            if (closeButton != null)
                closeButton.gameObject.SetActive(false); // No close button for main menu

            AddButton("Tasks", GameManager.Instance.UIManager.ToggleTaskListPanel).gameObject.AddComponent<LayoutElement>().preferredHeight = 40;
            AddButton("Tech", GameManager.Instance.UIManager.techTreePanel.Show).gameObject.AddComponent<LayoutElement>().preferredHeight = 40;
            AddButton("NPCs", GameManager.Instance.UIManager.npcListPanel.Show).gameObject.AddComponent<LayoutElement>().preferredHeight = 40;
            AddButton("Events", GameManager.Instance.UIManager.ToggleEventLogPanel).gameObject.AddComponent<LayoutElement>().preferredHeight = 40;
            AddButton("Releases", GameManager.Instance.UIManager.ToggleDeploymentHistoryPanel).gameObject.AddComponent<LayoutElement>().preferredHeight = 40;
            AddButton("Stats", GameManager.Instance.UIManager.globalStatsPanel.Show).gameObject.AddComponent<LayoutElement>().preferredHeight = 40;
        }
    }
}