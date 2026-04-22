using System.Collections.Generic;
using UnityEngine;

namespace UI
{
    public class UIToastHolderPanel: UIPanel
    {
        protected List<UIToastPanel> toastPanels = new();

        public UIToastPanel Add(string text, float duration = 15)
        {
            UIToastPanel toastPanel = GameManager.Instance.prefabManager.Create("UIToastPanel", Vector3.zero, scrollContent).GetComponent<UIToastPanel>();
            toastPanels.Add(toastPanel);
            toastPanel.Init(text, duration);
            return toastPanel;
        }
        protected virtual void FixedUpdate()
        {
            foreach (UIToastPanel toastPanel in toastPanels.ToArray())
            {
                float duration = toastPanel.Tick(Time.fixedUnscaledDeltaTime);
                if (duration <= 0f)
                {
                    toastPanel.gameObject.SetActive(false);
                    toastPanels.Remove(toastPanel);
                }
            }
           
        }
    }
}