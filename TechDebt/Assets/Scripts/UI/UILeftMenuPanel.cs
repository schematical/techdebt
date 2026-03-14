using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UI
{
    public class UILeftMenuPanel:  UIPanel
    {
        private List< UIPanelButton> _buttons = new List<UIPanelButton>();

        public override void Show()
        {

            base.Show();
            titleText.text = "";
            AddButton("Tasks", () => GameManager.Instance.UIManager.taskListPanel.Toggle());
            AddButton("Tech", () => GameManager.Instance.UIManager.techTreePanel.Toggle());
            AddButton("NPCs", () => GameManager.Instance.UIManager.npcListPanel.Toggle());
            AddButton("Releases", () =>GameManager.Instance.UIManager.releaseHistoryPanel.Toggle());
            AddButton("Stats", () => GameManager.Instance.UIManager.globalStatsPanel.Toggle());
            AddButton("Map", () => GameManager.Instance.UIManager.productRoadMap.Toggle());
            AddButton("Events", () =>GameManager.Instance.UIManager.eventDebugPanel.Toggle());
            
        }
      
    }
}