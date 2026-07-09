using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace UI
{
    public abstract class UIPanelLineSection: MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public RectTransform rectTransform;
        protected string Id;
        protected UnityAction<UIPanelLineSection> onFixedUpdate;
        private Action<UIToolTip> onToolTip;

        public virtual void Initialize()
        {
            Id = null;
            transform.localScale = Vector3.one;
            onFixedUpdate = null;
            onToolTip = null;

        }
        protected void FixedUpdate()
        {
            if (onFixedUpdate != null)
            {
                onFixedUpdate.Invoke(this);
            }
        }

        public void OnFixedUpdate(UnityAction<UIPanelLineSection> _onFixedUpdate)
        {
            onFixedUpdate = _onFixedUpdate;
        }

        public void SetId(string id)
        {
            Id = id;
        }

        public string GetId()
        {
            return Id;
        }


        public void RefreshLayout()
        {
            
            UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
        }

        public void OnToolTip(Action<UIToolTip> _onToolTip)
        {
            onToolTip = _onToolTip;
        }
        public void SetToolTip(string _toolTip)
        {
            OnToolTip((toolTip) =>
            {
                toolTip.AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = _toolTip;
            });
        }
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (onToolTip != null)
            {
                GameManager.Instance.UIManager.ShowTooltip(eventData, onToolTip);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (onToolTip != null)
            {
                GameManager.Instance.UIManager.HideTooltip();
            }
        }
    }
}