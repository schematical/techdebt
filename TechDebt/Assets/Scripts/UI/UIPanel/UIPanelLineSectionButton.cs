using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class UIPanelLineSectionButton: UIPanelLineSectionText
    {
        public Button button;
        // public Image image;
        public override void Initialize()
        {
            base.Initialize();
            button.onClick.RemoveAllListeners();
        }
    }
}