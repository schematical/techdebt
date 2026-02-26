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

            AddButton("Tasks", () => GameManager.Instance.UIManager.taskListPanel.Toggle()).gameObject.AddComponent<LayoutElement>().preferredHeight = 40;
            AddButton("Tech", () => GameManager.Instance.UIManager.techTreePanel.Toggle() ).gameObject.AddComponent<LayoutElement>().preferredHeight = 40;
            AddButton("NPCs", () => GameManager.Instance.UIManager.npcListPanel.Toggle()).gameObject.AddComponent<LayoutElement>().preferredHeight = 40;
            AddButton("Releases", () =>GameManager.Instance.UIManager.releaseHistoryPanel.Toggle()).gameObject.AddComponent<LayoutElement>().preferredHeight = 40;
            AddButton("Stats", () =>GameManager.Instance.UIManager.globalStatsPanel.Toggle()).gameObject.AddComponent<LayoutElement>().preferredHeight = 40;
            AddButton("Map", () => GameManager.Instance.UIManager.productRoadMap.Toggle()).gameObject.AddComponent<LayoutElement>().preferredHeight = 40;
            AddButton("Events", () =>GameManager.Instance.UIManager.eventDebugPanel.Toggle()).gameObject.AddComponent<LayoutElement>().preferredHeight = 40;
        }
    }
}