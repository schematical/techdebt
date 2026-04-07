using TMPro;
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
           button.onClick.RemoveAllListeners();

        }
        public override void Refresh()
        {
                //Do nothing
        }
    }
}