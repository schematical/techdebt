using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UI
{
    public class UILeftMenuPanel:  UIPanel
    {
        private Dictionary<string, UIButton> _buttons = new Dictionary<string, UIButton>();

        protected override void Start()
        {

            base.Start();
            titleText.text = "";
            GameManager.OnInfrastructureStateChange += Refresh;
            AddButton("Tasks", () => GameManager.Instance.UIManager.taskListPanel.Toggle(), "kanbanboard");
            AddButton("Tech", () => GameManager.Instance.UIManager.techTreePanel.Toggle());
            AddButton("NPCs", () => GameManager.Instance.UIManager.npcListPanel.Toggle());
            AddButton("Releases", () =>GameManager.Instance.UIManager.releaseHistoryPanel.Toggle(), "whiteboard");
            AddButton("Stats", () => GameManager.Instance.UIManager.globalStatsPanel.Toggle());
            AddButton("Map", () => GameManager.Instance.UIManager.productRoadMap.Toggle(), "product-road-map");
            AddButton("Events", () =>GameManager.Instance.UIManager.eventDebugPanel.Toggle());
            
            Refresh(null, null);
        }
        public UIButton AddButton(string buttonText, UnityAction onClickAction, [CanBeNull] string infraRequirement = null)
        {
            UIButton _button = base.AddButton(buttonText, onClickAction);
            _button.gameObject.AddComponent<LayoutElement>().preferredHeight = 40;
            if (infraRequirement != null)
            {
                _buttons.Add(infraRequirement, _button);
            }
            return _button;
            
        }
        public void Refresh(InfrastructureInstance x, InfrastructureData.State? state)
        {
        
            foreach (string instanceId in _buttons.Keys)
            {
                InfrastructureInstance infrastructureInstance = GameManager.Instance.GetInfrastructureInstanceByID(instanceId);
                if (infrastructureInstance == null)
                {
                    throw new System.Exception("No infrastructure instances found for " + instanceId);
                }
                _buttons[instanceId].gameObject
                    .SetActive(infrastructureInstance.IsActive());
            }
               
            
        }
    }
}