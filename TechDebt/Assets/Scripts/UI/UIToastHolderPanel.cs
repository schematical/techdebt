using System.Collections.Generic;
using UnityEngine;

namespace UI
{
    public class UIToastHolderPanel: UIPanel
    {
        protected List<UIToastPanel> toastPanels = new();

        public UIToastPanel Add(string text, float duration = 5)
        {
            UIToastPanel toastPanel = GameManager.Instance.prefabManager.Create("UIToastPanel", Vector3.zero, GameManager.Instance.UIManager.transform).GetComponent<UIToastPanel>();
            toastPanels.Add(toastPanel);
            toastPanel.Init(text, duration);
            return toastPanel;
        }
        protected virtual void FixedUpdate()
        {
            foreach (UIToastPanel toastPanel in toastPanels)
            {
                float duration = toastPanel.Tick(Time.fixedDeltaTime);
                if (duration <= 0f)
                {
                    toastPanel.gameObject.SetActive(false);
                    toastPanels.Remove(toastPanel);
                }
            }
           
        }
    }
}