using UnityEngine;
using TMPro;

namespace UI
{
    public class UIDialogBubble: UIPanel
    {
        public RectTransform pointer;
        protected NPCBase target;
        protected Vector3 worldOffset = new Vector3(0, 1.5f, 0);

        protected override void Awake()
        {
            runUICloseOnShow = false;
            base.Awake();
        }

        public override void Show()
        {
            if (panelState == UIState.Open) return;
            gameObject.SetActive(true);
            panelState = UIState.Open;
            canvasGroup.alpha = 1;
        }

        public void SetTarget(NPCBase target)
        {
            this.target = target;
        }

        protected virtual void LateUpdate()
        {
            if (target == null || target.transform == null || !target.transform.gameObject.activeInHierarchy)
            {
                if (panelState != UIState.Closed && panelState != UIState.Closing)
                {
                    Close();
                }
                return;
            }

            Camera cam = Camera.main;
            if (cam == null) return;

            Vector3 worldPos = target.transform.position + worldOffset;
            Vector3 viewportPos = cam.WorldToViewportPoint(worldPos);

            bool isOffScreen = viewportPos.z < 0 || 
                               viewportPos.x < 0 || viewportPos.x > 1 || 
                               viewportPos.y < 0 || viewportPos.y > 1;

            if (isOffScreen)
            {
                if (canvasGroup != null) canvasGroup.alpha = 0;
                return;
            }
            
            if (canvasGroup != null) canvasGroup.alpha = 1;

            // Pin the UI element to the target's viewport position
            rectTransform.anchorMin = new Vector2(viewportPos.x, viewportPos.y);
            rectTransform.anchorMax = new Vector2(viewportPos.x, viewportPos.y);
            rectTransform.anchoredPosition = Vector2.zero;
        }
    }
}