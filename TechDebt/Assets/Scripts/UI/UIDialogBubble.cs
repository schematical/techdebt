using UnityEngine;
using TMPro;

namespace UI
{
    public class UIDialogBubble: UIPanel
    {
        public RectTransform pointer;
        protected NPCBase target;
        public Vector3 worldOffset = new Vector3(0, -2f, 0);

        protected override void Awake()
        {
            runUICloseOnShow = false;
            base.Awake();
        }

   
        public void SetTarget(NPCBase target)
        {
            this.target = target;
        }

        protected virtual void LateUpdate()
        {
            if (target.transform == null || !target.transform.gameObject.activeInHierarchy)
            {
                if (panelState != UIState.Closed && panelState != UIState.Closing)
                {
                    Close();
                }
                return;
            }

            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null) return;

            Camera cam = canvas.worldCamera;
            if (cam == null) cam = Camera.main;

            Vector3 worldPos = target.transform.position + worldOffset;
            Vector3 screenPos = cam.WorldToScreenPoint(worldPos);

            // Check if the target is behind the camera or off-screen
            bool isOffScreen = screenPos.z < 0 || 
                               screenPos.x < 0 || screenPos.x > Screen.width || 
                               screenPos.y < 0 || screenPos.y > Screen.height;

            if (isOffScreen)
            {
                if (canvasGroup != null) canvasGroup.alpha = 0;
                return;
            }
            
            if (canvasGroup != null) canvasGroup.alpha = 1;

            RectTransform parentRect = rectTransform.parent as RectTransform;
            if (parentRect == null) return;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, screenPos, cam, out Vector2 localPoint))
            {
                rectTransform.anchoredPosition = localPoint;
            }
        }
    }
}