using UnityEngine;

namespace UI
{
    public class UILazerBeam: MonoBehaviour
    {


        private float speed = 0.001f;
        private Vector2 startingAnchorMax;
        private RectTransform rectTransform;

        public void Start()
        {
            rectTransform = GetComponent<RectTransform>();
            startingAnchorMax = rectTransform.anchorMax;
        }

        public void Update()
        {

   
            
            float y = rectTransform.anchorMax.y;
            float nextY = y + speed;
            Debug.Log($"Next Y: {nextY} - speed: {speed}");
            rectTransform.anchorMax = new Vector2(rectTransform.anchorMax.x, nextY);
            
           
            
        }

        public void Restart()
        {
            gameObject.SetActive(true);
            
        }
    }
}