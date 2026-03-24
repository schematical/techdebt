using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace UI
{
    public class UIDialogBubble: UIPanel
    {
        public RectTransform pointer;
        protected NPCBase target;
        protected Vector3 worldOffset = new Vector3(0, 1.5f, .5f);
        public RectTransform dialogBox;
        protected override void Awake()
        {
            runUICloseOnShow = false;
            base.Awake();
        }

        public override void Show()
        {
            CleanUp();
            gameObject.SetActive(true);
            transform.SetAsFirstSibling();
            panelState = UIState.Open;
        }

        public void SimpleDisplay(string description, List<DialogButtonOption> options = null)
        {
            AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = description;

            if (options == null)
            {
                options = new List<DialogButtonOption>()
                {
                    new DialogButtonOption()
                    {
                        Text = "Continue", 
                        OnClick = () =>
                        {
                            Close();
                        }
                    },
                };
            }
            foreach (DialogButtonOption option in options)
            {
                AddButton(
                    option.Text,
                    () =>
                    {
                        option.OnClick.Invoke();
                    }
                );
            }
        }

        public void SetTarget(NPCBase target)
        {
            this.target = target;
        }

        public override void Close(bool forceClose = false)
        {
            CleanUp();
            gameObject.SetActive(false);
        }

        public override void RefreshLayout()
        {
    
            base.RefreshLayout();
     
            UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(dialogBox);
            
        }

        protected override void LateUpdate()
        {
            base.LateUpdate();
            Camera cam = Camera.main;

            Vector3 worldPos = target.transform.position + worldOffset;
            Vector3 viewportPos = cam.WorldToViewportPoint(worldPos);

            bool isOffScreen = viewportPos.z < 0 || 
                               viewportPos.x < 0 || viewportPos.x > 1 || 
                               viewportPos.y < 0 || viewportPos.y > 1;
            
            
            // Pin the UI element to the target's viewport position
            rectTransform.anchorMin = new Vector2(viewportPos.x, viewportPos.y);
            rectTransform.anchorMax = new Vector2(viewportPos.x, viewportPos.y);
            rectTransform.anchoredPosition = Vector2.zero;
        }
    }
}