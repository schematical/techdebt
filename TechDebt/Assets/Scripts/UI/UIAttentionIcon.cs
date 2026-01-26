using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class UIAttentionIcon: MonoBehaviour
    {
        public SpriteRenderer spriteRenderer;
        protected Transform targetTransform;
        public void Show(Transform _transform, Color color)
        {
            
            targetTransform = _transform;
            spriteRenderer.color = color;
        }

        void Update()
        {
            float padding = 50f;
            Vector3 targetPosition = targetTransform.position + new Vector3(0f, 2f, -1f);
            Vector3 screenPos = Camera.main.WorldToScreenPoint(targetPosition);

            if (screenPos.x > padding && screenPos.x < Screen.width - padding && screenPos.y > padding && screenPos.y < Screen.height - padding)
            {
                transform.position = targetPosition;
            }
            else
            {
                Vector3 screenCenter = new Vector3(Screen.width / 2, Screen.height / 2, 0);
                Vector3 screenDirection = (screenPos - screenCenter).normalized;

                float angle = Mathf.Atan2(screenDirection.y, screenDirection.x) * Mathf.Rad2Deg;

                float screenWidth = Screen.width - padding * 2;
                float screenHeight = Screen.height - padding * 2;
                
                float clampedX = Mathf.Clamp(screenPos.x, padding, Screen.width - padding);
                float clampedY = Mathf.Clamp(screenPos.y, padding, Screen.height - padding);

                if (clampedX == padding || clampedX == Screen.width - padding)
                {
                    clampedY = screenCenter.y + screenDirection.y * (screenWidth / 2) / Mathf.Abs(screenDirection.x);
                    clampedY = Mathf.Clamp(clampedY, padding, Screen.height - padding);
                }
                
                if (clampedY == padding || clampedY == Screen.height - padding)
                {
                    clampedX = screenCenter.x + screenDirection.x * (screenHeight / 2) / Mathf.Abs(screenDirection.y);
                    clampedX = Mathf.Clamp(clampedX, padding, Screen.width - padding);
                }
                
                Vector3 newScreenPos = new Vector3(clampedX, clampedY, screenPos.z);
                transform.position = Camera.main.ScreenToWorldPoint(newScreenPos);
            }
        }
    }
}