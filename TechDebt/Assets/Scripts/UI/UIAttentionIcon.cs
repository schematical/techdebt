using UnityEngine;

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
            if (targetTransform == null)
            {
                Destroy(gameObject);
                return;
            }
            
            float padding = 50f;
            Vector3 targetPosition = targetTransform.position + new Vector3(0f, 2f, -1f);
            Vector3 screenPos = Camera.main.WorldToScreenPoint(targetPosition);

            bool isOffScreen = screenPos.x <= padding || screenPos.x >= Screen.width - padding ||
                               screenPos.y <= padding || screenPos.y >= Screen.height - padding || screenPos.z < 0;

            if (!isOffScreen)
            {
                transform.position = targetPosition;
                transform.rotation = Quaternion.identity; // Reset rotation when on-screen
            }
            else
            {
                // If the target is behind the camera, project it onto the camera plane
                if (screenPos.z < 0)
                {
                    screenPos *= -1;
                }

                Vector3 screenCenter = new Vector3(Screen.width / 2, Screen.height / 2, 0);
                screenPos -= screenCenter;
                
                float angle = Mathf.Atan2(screenPos.y, screenPos.x);
                angle -= 90 * Mathf.Deg2Rad;
                
                float cos = Mathf.Cos(angle);
                float sin = -Mathf.Sin(angle);
                
                screenPos = screenCenter + new Vector3(sin * 150, cos * 150, 0);
                
                float m = cos / sin;
                
                Vector3 screenBounds = screenCenter * 0.9f;

                if (cos > 0)
                {
                    screenPos = new Vector3(screenBounds.y / m, screenBounds.y, 0);
                }
                else
                {
                    screenPos = new Vector3(-screenBounds.y / m, -screenBounds.y, 0);
                }

                if (screenPos.x > screenBounds.x)
                {
                    screenPos = new Vector3(screenBounds.x, screenBounds.x * m, 0);
                }
                else if (screenPos.x < -screenBounds.x)
                {
                    screenPos = new Vector3(-screenBounds.x, -screenBounds.x * m, 0);
                }

                screenPos += screenCenter;
                
                // We have to fudge the Z position to make sure the icon appears in front of the camera
                Vector3 newWorldPos = Camera.main.ScreenToWorldPoint(screenPos + new Vector3(0,0,10));
                transform.position = newWorldPos;
                
                Vector3 directionToTarget = (targetTransform.position - transform.position).normalized;
                float rotationAngle = Mathf.Atan2(directionToTarget.y, directionToTarget.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0, 0, rotationAngle + 90); // Adjust for sprite's default orientation
            }
        }
    }
}