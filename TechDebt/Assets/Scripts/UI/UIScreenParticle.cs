using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class UIScreenParticle: MonoBehaviour
    {
        public enum State { Falling, Fading}

        protected State state;
        public RectTransform rectTransform;
        public Image image;
        void Update()
        {
            Vector3[] corners = new Vector3[4];
            rectTransform.GetWorldCorners(corners);
            
            // Bottom-right corner is corners[2]
            Vector3 bottomRight = corners[2];
            switch (state)
            {
              
                case(State.Falling):
              
                    rectTransform.position = new Vector2(rectTransform.position.x, rectTransform.position.y - 1f);
                    if (bottomRight.y < 0)
                    {
                       
                        gameObject.SetActive(false);
                    }
                    break;
             case(State.Fading):
                 
                    rectTransform.position = new Vector2(rectTransform.position.x, rectTransform.position.y - 1f);
                    image.color = new Color(
                        image.color.r,
                        image.color.g,
                        image.color.b,
                        image.color.a - ( Time.unscaledDeltaTime)
                    );
                    if (image.color.a <= 0)
                    {
                        gameObject.SetActive(false);
                    }
                    break;
            }

            
        }
        public void Init(Sprite sprite, float rotationZ = 0)
        {
            image.color = Color.white;
            image.sprite = sprite;
            state = State.Falling;
            rectTransform.rotation = Quaternion.Euler(0f, 0f, rotationZ);
            gameObject.SetActive(true);
            // rectTransform.sizeDelta = new Vector2(25, 25);
            // rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 25);
            // rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 25);
            
            rectTransform.rotation = Quaternion.Euler(0f, 0f, 0f);
            // rectTransform.sizeDelta = new Vector2(100, 100);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 100);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 100);
            float newX =  Random.Range(0, Screen.width / 4);
            if (Random.Range(0, 2) > 0)
            {
                newX = Screen.width - newX;
            }
                        
            rectTransform.position = new Vector2(newX, 0);
            
        }

        public void Shutdown()
        {
            state = State.Fading;
            
            /*switch (state)
            {
                case (State.Fading):
                    break;

            }*/
        }
    }
}