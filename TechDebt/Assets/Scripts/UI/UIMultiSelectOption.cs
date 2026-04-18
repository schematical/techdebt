using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
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
        public Image backgroundImage;
        public TextMeshProUGUI primaryText;
        public TextMeshProUGUI secondaryText;
        public TextMeshProUGUI banishButtonText;
        public Button selectButton;
        
        protected UnityAction<InteractionType, string> onInteract;
        protected UIMultiSelectPanel parentPanel;
        
        public Button banishButton;
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
            /*image.color = Color.white;
            backgroundImage.color = Color.grey;*/
            Reset();
            _banishable = false;

            if (banishButton != null)
            {
                banishButton.gameObject.SetActive(false);
                banishButton.onClick.RemoveAllListeners();
            }

            // Clear any previous listeners and reset the selectButton state.
            selectButton.onClick.RemoveAllListeners();
            selectButton.onClick.AddListener((() =>
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
                banishButtonText.text = $"Banish ({banishCount})";

                banishButton.onClick.RemoveAllListeners();
          
                banishButton.onClick.AddListener(() =>
                {
                    if (onInteract != null)
                    {
                        onInteract.Invoke(InteractionType.Banish, id);
                    }
                });
            }
            else if (banishButton != null)
            {
                banishButton.gameObject.SetActive(false);
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

        public void Reset()
        {              
            backgroundImage.color = Color.grey;
            image.color = Color.white;
            selectButton.gameObject.SetActive(true);
        }
    }
}