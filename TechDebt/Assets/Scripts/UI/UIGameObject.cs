using System;
using UnityEngine;

namespace UI
{
    public class UIGameObject: MonoBehaviour
    {
        public enum UIState {Closed, Open, Closing, Opening}
        public UIState state = UIState.Closed;
        public enum SlideDirection { Left, Right, Up, Down }
        public SlideDirection slideDirection = SlideDirection.Right;
        public RectTransform rectTransform;
        public CanvasGroup canvasGroup;
        public float animationTime = 0.5f;
        private Vector2 initialAnchorMin;
        private Vector2 initialAnchorMax;
        
        // Animation state
        private float animationProgress;
        private Vector2 startAnchorMin, startAnchorMax;
        private Vector2 targetAnchorMin, targetAnchorMax;
        private float startAlpha, targetAlpha;

        protected virtual void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
            canvasGroup.alpha = 1;
            rectTransform = GetComponent<RectTransform>();
            if (rectTransform == null)
            {
               throw new System.Exception("UIGameObject has no RectTransform");
            }
            initialAnchorMin = rectTransform.anchorMin;
            initialAnchorMax = rectTransform.anchorMax;
        }
        public virtual void Close(bool forceClose = false)
        {
            if (forceClose)
            {
                gameObject.SetActive(false);
                return;
            }
            SlideOut();
        
        }
        public virtual void Show()
        {
            gameObject.SetActive(true);
            SlideIn();
        
        }
        protected virtual void Start()
        {
    
       
           
        }

        protected virtual void Update()
        {
            /*if (!isInitialized && rectTransform.rect.width > 0)
            {
                isInitialized = true;
                // Recalculate here to get the final, post-layout values
                initialAnchorMin = rectTransform.anchorMin;
                initialAnchorMax = rectTransform.anchorMax;

            }*/

            if (!IsAnimating()) return;
            // Debug.Log($"{gameObject.name} is Animating");
            animationProgress += Time.unscaledDeltaTime / animationTime;

            if (animationProgress >= 1.0f)
            {
                animationProgress = 1.0f;
                switch (state)
                {
                    case(UIState.Closing):
                        state = UIState.Closed;
                        gameObject.SetActive(false);
                        break;
                    case(UIState.Opening):
                        state = UIState.Open;
                        break;
                    default:
                        throw new SystemException($"We are in a weird state {state} while trying to end an animation.");
                }
            }

            float easedProgress = 1 - Mathf.Pow(1 - animationProgress, 3); // Ease-out

            rectTransform.anchorMin = Vector2.Lerp(startAnchorMin, targetAnchorMin, easedProgress);
            rectTransform.anchorMax = Vector2.Lerp(startAnchorMax, targetAnchorMax, easedProgress);
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, easedProgress);
        }

        private void Animate(Vector2 newTargetAnchorMin, Vector2 newTargetAnchorMax, float newTargetAlpha)
        {
            animationProgress = 0f;

            startAnchorMin = rectTransform.anchorMin;
            startAnchorMax = rectTransform.anchorMax;
            startAlpha = canvasGroup.alpha;

            targetAnchorMin = newTargetAnchorMin;
            targetAnchorMax = newTargetAnchorMax;
            targetAlpha = newTargetAlpha;
        }

        private bool isInitialized = false;

        public void SlideIn()
        {
            if (state != UIState.Closed)
            {
                return;
            }
            state = UIState.Opening;

            Vector2 offset = Vector2.zero;
            switch(slideDirection) {
                case SlideDirection.Left: // Comes from Right
                    offset = new Vector2(1, 0);
                    break;
                case SlideDirection.Right: // Comes from Left
                    offset = new Vector2(-1, 0);
                    break;
                case SlideDirection.Up: // Comes from Bottom
                    offset = new Vector2(0, -1);
                    break;
                case SlideDirection.Down: // Comes from Top
                    offset = new Vector2(0, 1);
                    break;
            }
            
            rectTransform.anchorMin = initialAnchorMin + offset;
            rectTransform.anchorMax = initialAnchorMax + offset;
     
            Animate(initialAnchorMin, initialAnchorMax, 1);
        }

        protected bool IsAnimating()
        {
            return state == UIState.Closing || state == UIState.Opening;
        }
        public void SlideOut()
        {
            if (state != UIState.Open)
            {
                return;
            }
            state = UIState.Closing;
            
            Vector2 offset = Vector2.zero;
            switch(slideDirection) {
                case SlideDirection.Left: // Slides out to the Right
                    offset = new Vector2(1, 0);
                    break;
                case SlideDirection.Right: // Slides out to the Left
                    offset = new Vector2(-1, 0);
                    break;
                case SlideDirection.Up: // Slides out to the Bottom
                    offset = new Vector2(0, -1);
                    break;
                case SlideDirection.Down: // Slides out to the Top
                    offset = new Vector2(0, 1);
                    break;
            }
            
            Vector2 outAnchorMin = initialAnchorMin + offset;
            Vector2 outAnchorMax = initialAnchorMax + offset;
            
            Animate(outAnchorMin, outAnchorMax, 0);
        }
    }
}