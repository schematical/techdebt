using System;
using System.Collections.Generic;
using UnityEngine;

namespace UI
{
    public class UIMetricsBubble: UIPanel
    {
        public RectTransform pointer;
        protected InfrastructureInstance target;
        public RectTransform dialogBox;
        public UIPanelLineProgressBar cpuLoadBar;
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
            AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().h1("Metrics:");
            cpuLoadBar = AddLine<UIPanelLineProgressBar>();
            cpuLoadBar.SetPreText("CPU Load:");
            
        }

        public virtual void Update()
        {
            base.Update();
            float load = target.CurrentLoad / target.GetMaxLoad();
            Color color = new Color(1, 1- load, 1-load, 1);
            cpuLoadBar.SetProgress(load, color);
            
            Camera cam = Camera.main;

            Vector3 worldPos = target.GetInteractionPosition(InteractionType.MetricsBubble);
            Vector3 viewportPos = cam.WorldToViewportPoint(worldPos);

            bool isOffScreen = viewportPos.z < 0 || 
                               viewportPos.x < 0 || viewportPos.x > 1 || 
                               viewportPos.y < 0 || viewportPos.y > 1;
            
            
            // Pin the UI element to the target's viewport position
            rectTransform.anchorMin = new Vector2(viewportPos.x, viewportPos.y);
            rectTransform.anchorMax = new Vector2(viewportPos.x, viewportPos.y);
            rectTransform.anchoredPosition = Vector2.zero;
        }

        /*public void SimpleDisplay(string description, List<DialogButtonOption> options = null)
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
        }*/

        public void SetTarget(InfrastructureInstance target)
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

        /*protected override void LateUpdate()
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
        }*/
    }
}