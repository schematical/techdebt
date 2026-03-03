using TMPro;
using UnityEngine.UI;

namespace UI
{
    public class UIAlertPanel : UIPanel
    {
        public TextMeshProUGUI bodyText;
        public Button acceptButton;

        void Start()
        {
            acceptButton.onClick.AddListener(() => Close());
        }
    }
}