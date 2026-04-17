using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    public class UIMultiSelectOption: MonoBehaviour
    {
        public enum InteractionType
        {
            Select,
            Preview,
            Banish
        }

        public string id;
        public Image image;
        public TextMeshProUGUI primaryText;
        public TextMeshProUGUI secondaryText;
        public Button button;
        
        protected UnityAction<InteractionType, string> onInteract;
        protected UIMultiSelectPanel parentPanel;
        
        public UIButton banishButton;
        private bool _banishable = false;

        public UIMultiSelectOption OnInteract(UnityAction<InteractionType, string> action)
        {
            onInteract = action;
            return this;
        }

        public void Initialize(UIMultiSelectPanel _parentPanel, string _id, Sprite sprite, string _primaryText, string _secondaryText)
        {
            parentPanel = _parentPanel;
            id = _id;
            image.sprite = sprite;
            primaryText.text = _primaryText;
            secondaryText.text = _secondaryText;
            onInteract = null;
            image.color = Color.white;
            _banishable = false;

            if (banishButton != null)
            {
                banishButton.gameObject.SetActive(false);
                banishButton.button.onClick.RemoveAllListeners();
            }

            // Clear any previous listeners and reset the button state.
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener((() =>
            {
                if (onInteract != null)
                {
                    onInteract.Invoke(InteractionType.Preview, id);
                }

                _parentPanel.SetPreview(this);
            }));
        }

        public UIMultiSelectOption MarkBanisable()
        {
            _banishable = true;

            int banishCount = (int)GameManager.Instance.GetStatValue(StatType.Global_Banish);
            if (banishCount > 0 && banishButton != null)
            {
                banishButton.gameObject.SetActive(true);
                banishButton.buttonText.text = $"Banish ({banishCount})";

                banishButton.button.onClick.RemoveAllListeners();
                banishButton.button.onClick.AddListener(() =>
                {
                    if (onInteract != null)
                    {
                        onInteract.Invoke(InteractionType.Banish, id);
                    }
                });
            }
            return this;
        }

        public void SetParentBottomText(string text)
        {
            parentPanel.bottomText.text = text;
        }

        public void MarkSelected()
        {
            if (onInteract != null)
            {
                onInteract.Invoke(InteractionType.Select, id);
            }
        }
    }
}