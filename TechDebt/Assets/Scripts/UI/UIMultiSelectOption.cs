using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    public class UIMultiSelectOption: MonoBehaviour//, IPointerEnterHandler
    {
        public string id;
        public Image image;
        public TextMeshProUGUI primaryText;
        public TextMeshProUGUI secondaryText;
        public Button button;
        protected UnityAction<string> onPreview;
        protected UnityAction<string> onSelect;
        protected UIMultiSelectPanel parentPanel;
        
        public void OnSelect(UnityAction<string> action)
        {
            onSelect = action;
        }
        
 

        public void OnPreview(UnityAction<string> action)
        {
            onPreview = action;
        }

        public void Initialize(UIMultiSelectPanel _parentPanel, string _id, Sprite sprite, string _primaryText, string _secondaryText)
        {
            parentPanel = _parentPanel;
            id = _id;
            image.sprite = sprite;
            primaryText.text = _primaryText;
            secondaryText.text = _secondaryText;
            onPreview = null;
          
            // Clear any previous listeners and reset the button state.
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener((() =>
            {
                if (onPreview != null)
                {
                    onPreview.Invoke(id);
                }

                _parentPanel.SetPreview(this);
                //action.Invoke(id);
            }));
        }

        public void SetParentBottomText(string text)
        {
            parentPanel.bottomText.text = text;
        }
        public void MarkSelected()
        {
            onSelect.Invoke(id);
        }
    }
}