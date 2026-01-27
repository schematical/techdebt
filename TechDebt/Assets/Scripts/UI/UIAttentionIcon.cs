using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace UI
{
    public class UIAttentionIcon : MonoBehaviour, IPointerClickHandler
    {


        public SpriteRenderer spriteRenderer;
        protected Transform targetTransform;
        protected UnityAction onClick;


        public void Show(Transform _transform, Color color, UnityAction _onClick)
        {
            targetTransform = _transform;
            spriteRenderer.color = color;
            onClick = _onClick;

        }

        void Update()
        {
            if (targetTransform == null)
            {
                Destroy(gameObject);
                return;
            }

            float padding = 50f;
            Vector3 targetPosition = targetTransform.position + new Vector3(0f, 2f, -1f);
            Vector3 screenPos = Camera.main.WorldToScreenPoint(targetPosition);

            float minX, maxX, minY, maxY;


            Vector3[] corners = new Vector3[4];
            GameManager.Instance.UIManager.attentionIconBoarderPanel.GetWorldCorners(corners);
            Vector3 bottomLeftScreen = RectTransformUtility.WorldToScreenPoint(null, corners[0]);
            Vector3 topRightScreen = RectTransformUtility.WorldToScreenPoint(null, corners[2]);

            minX = bottomLeftScreen.x;
            maxX = topRightScreen.x;
            minY = bottomLeftScreen.y;
            maxY = topRightScreen.y;


            bool isOffScreen = screenPos.x <= minX + padding || screenPos.x >= maxX - padding ||
                               screenPos.y <= minY + padding || screenPos.y >= maxY - padding || screenPos.z < 0;

            if (!isOffScreen)
            {
                transform.position = targetPosition;
                transform.rotation = Quaternion.identity;
            }
            else
            {
                if (screenPos.z < 0)
                {
                    screenPos.x = Screen.width - screenPos.x;
                    screenPos.y = Screen.height - screenPos.y;
                }

                float clampedX = Mathf.Clamp(screenPos.x, minX + padding, maxX - padding);
                float clampedY = Mathf.Clamp(screenPos.y, minY + padding, maxY - padding);

                Vector3 clampedScreenPos = new Vector3(clampedX, clampedY, screenPos.z);
                // Ensure Z is positive for ScreenToWorldPoint
                clampedScreenPos.z = Camera.main.nearClipPlane + 0.1f;

                Vector3 newWorldPos = Camera.main.ScreenToWorldPoint(clampedScreenPos);
                transform.position = newWorldPos;

                Vector3 directionToTarget = (targetTransform.position - transform.position).normalized;
                float angle = Mathf.Atan2(directionToTarget.y, directionToTarget.x) * Mathf.Rad2Deg + 90;
                transform.rotation = Quaternion.Euler(0, 0, angle);
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Debug.Log("OnPointerClick");
            onClick.Invoke();
        }
    }

}