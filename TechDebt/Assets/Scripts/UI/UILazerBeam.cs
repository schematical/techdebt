using UnityEngine;

namespace UI
{
    public class UILazerBeam: MonoBehaviour
    {


        private float speed = 0.001f;
        private Vector2 startingAnchorMax;
        public RectTransform rectTransform;

        public void Start()
        {
            if (rectTransform == null)
            {
                rectTransform = GetComponent<RectTransform>();
            }

            startingAnchorMax = rectTransform.anchorMax;
        }

        public void Update()
        {

   
            
            float y = rectTransform.anchorMax.y;
            float nextY = y + speed;
            Debug.Log($"Next Y: {nextY} - speed: {speed}");
            rectTransform.anchorMax = new Vector2(rectTransform.anchorMax.x, nextY);
            
           
            
        }

        public void Init(int rotationZ = 0)
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