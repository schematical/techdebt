using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI
{
    public class UIToolTipListener: MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
 
            private Action<UIToolTip> onToolTip;
            public void OnToolTip(Action<UIToolTip> _onToolTip)
            {
                onToolTip = _onToolTip;
            }
            public void SetToolTip(string _toolTip)
            {
                if (_toolTip == null)
                {
                    onToolTip = null;
                    return;
                }
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