using UnityEngine;

namespace UI
{
    public class UIGameObject: MonoBehaviour
    {
        public enum SlideDirection { Left, Right, Up, Down }
        public SlideDirection slideDirection;
        public RectTransform rectTransform;
        [SerializeField] private Vector2 initialAnchorMin;
        [SerializeField] private Vector2 initialAnchorMax;

        void Start()
        {
            initialAnchorMin = rectTransform.anchorMin;
            initialAnchorMax = rectTransform.anchorMax;
        }

        public void SlideIn()
        {
            switch(slideDirection) {
                case SlideDirection.Left:
                // Start pos will be of the right side of the screen
                break;
            }
        }
        public void SlideOut()
        {
            
        }
    }
}