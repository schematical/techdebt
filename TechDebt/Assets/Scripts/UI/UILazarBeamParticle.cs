using UnityEngine;

namespace UI
{
    public class UILazarBeamParticle: MonoBehaviour
    {
        public RectTransform rectTransform;
        void Update()
        {
            rectTransform.position = new Vector2(rectTransform.position.x, rectTransform.position.y + 1f);
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