using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class UILeftMenuPanel:  UIPanel
    {
        private UIManager uiManager;

        void Start()
        {
            uiManager = FindObjectOfType<UIManager>();
            if (uiManager == null)
            {
                Debug.LogError("UILeftMenuPanel could not find the UIManager in the scene.");
                return;
            }

            titleText.text = "";
            if (closeButton != null)
                closeButton.gameObject.SetActive(false); // No close button for main menu

            AddButton("Tasks", uiManager.ToggleTaskListPanel).gameObject.AddComponent<LayoutElement>().preferredHeight = 40;
            AddButton("Tech", uiManager.ToggleTechTreePanel).gameObject.AddComponent<LayoutElement>().preferredHeight = 40;
            AddButton("NPCs", uiManager.ToggleNPCListPanel).gameObject.AddComponent<LayoutElement>().preferredHeight = 40;
            AddButton("Events", uiManager.ToggleEventLogPanel).gameObject.AddComponent<LayoutElement>().preferredHeight = 40;
            AddButton("Deployments", uiManager.ToggleDeploymentHistoryPanel).gameObject.AddComponent<LayoutElement>().preferredHeight = 40;
        }
    }
}