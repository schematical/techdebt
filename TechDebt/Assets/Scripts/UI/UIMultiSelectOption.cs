using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    public class UIMultiSelectOption: MonoBehaviour, IPointerEnterHandler
    {
        public string id;
        public Image image;
        public TextMeshProUGUI primaryText;
        public TextMeshProUGUI secondaryText;
        public Button button;
        protected Action<string> onHover;

        public void OnClick(UnityAction<string> action)
        {
            button.onClick.AddListener((() =>
            {
                action.Invoke(id);
            }));
        }
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            Debug.Log($"OnMouseOver {gameObject.name}");
            onHover.Invoke(id);
        }

        public void OnHover(Action<string> action)
        {
            onHover = action;
        }
    }
}