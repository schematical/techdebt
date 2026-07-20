using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class UIPanelButton: UIPanelLine
    {
        public Button button;
        public TextMeshProUGUI text;
        public override void Initialize(int _depth, UIPanel _rootPanel, UIPanelLine _parentLine)
        {
           base.Initialize(_depth, _rootPanel, _parentLine);
           text.fontSize = 20;
           SetButtonHeight(26);
           button.onClick.RemoveAllListeners();

        }
        public override void Refresh()
        {
            //Do nothing
        }

        private void SetButtonHeight(int height)
        {
            button.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        }
        /*public void H1()
        {
            text.fontSize = 70;
            SetButtonHeight(60);
        }*/
        public void H1()
        {
            text.fontSize = 55;
            SetButtonHeight(50);
        }
        public void H2()
        {
            text.fontSize = 45;
            SetButtonHeight(40);
        }
        public void H3()
        {
            text.fontSize = 30;
            SetButtonHeight(35);
        }
    }
}