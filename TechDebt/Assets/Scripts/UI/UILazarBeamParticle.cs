using UnityEngine;

namespace UI
{
    public class UILazarBeamParticle: MonoBehaviour
    {
        public RectTransform rectTransform;
        void Update()
        {
            rectTransform.position = new Vector2(rectTransform.position.x, rectTransform.position.y + 1f);
            
            Vector3[] corners = new Vector3[4];
            rectTransform.GetWorldCorners(corners);
            
            // Bottom-right corner is corners[2]
            Vector3 bottomRight = corners[2];

            if (bottomRight.x < 0 || bottomRight.x > Screen.width || bottomRight.y < 0 || bottomRight.y > Screen.height)
            {
                gameObject.SetActive(false);
            }
        }
        public void Init(float rotationZ = 0)
        {
            /*if (
                rectTransform  != null &&
                startingAnchorMax != null)
            {
                rectTransform.anchorMax = startingAnchorMax;
            }*/
           
            rectTransform.rotation = Quaternion.Euler(0f, 0f, rotationZ);
            gameObject.SetActive(true);
            
        }
    }
}