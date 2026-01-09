using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UI
{
    public class UIMultiSelectOption: MonoBehaviour
    {
        public string id;
        public Image image;
        public TextMeshProUGUI primaryText;
        public TextMeshProUGUI secondaryText;
        public Button button;

        public void OnClick(UnityAction<string> action)
        {
            button.onClick.AddListener((() =>
            {
                action.Invoke(id);
            }));
        }
    }
}