using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace UI
{
    public class ScreenParticle: MonoBehaviour
    {
        public enum State {Rising, Falling, Fading}

        protected State state;
        public RectTransform rectTransform;
        public List<Sprite> sprites;
        public SpriteRenderer spriteRenderer;
        void Update()
        {
            Vector3[] corners = new Vector3[4];
            rectTransform.GetWorldCorners(corners);
            
            // Bottom-right corner is corners[2]
            Vector3 bottomRight = corners[2];
            switch (state)
            {
                case(State.Rising):
                    rectTransform.position = new Vector2(rectTransform.position.x, rectTransform.position.y + 0.1f);
                    if (bottomRight.y > Screen.height + 100)
                    {
                        state = State.Falling;
                        transform.SetParent(transform.parent.parent.parent);
                        rectTransform.rotation = Quaternion.Euler(0f, 0f, 0f);
                        // rectTransform.sizeDelta = new Vector2(100, 100);
                        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 100);
                        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 100);
                        float newX =  Random.Range(0, Screen.width / 4);
                        if (Random.Range(0, 2) > 0)
                        {
                            newX = Screen.width - newX;
                        }
                        
                        rectTransform.position = new Vector2(newX, rectTransform.position.y);
                    }
                    break;
                case(State.Falling):
              
                    rectTransform.position = new Vector2(rectTransform.position.x, rectTransform.position.y - 1f);
                    if (bottomRight.y < 0)
                    {
                       
                        gameObject.SetActive(false);
                    }
                    break;
             case(State.Fading):
                 
                    rectTransform.position = new Vector2(rectTransform.position.x, rectTransform.position.y - 1f);
                    spriteRenderer.color = new Color(
                        spriteRenderer.color.r,
                        spriteRenderer.color.g,
                        spriteRenderer.color.b,
                        spriteRenderer.color.a - ( Time.unscaledDeltaTime)
                    );
                    if (spriteRenderer.color.a <= 0)
                    {
                        gameObject.SetActive(false);
                    }
                    break;
            }

            
        }
        public void Init(float rotationZ = 0)
        {
            if (sprites.Count == 0)
            {
                throw new SystemException("No sprites found");
            }
            spriteRenderer.sprite = sprites[Random.Range(0, sprites.Count)];
            spriteRenderer.color = Color.white;
            state = State.Rising;
            rectTransform.rotation = Quaternion.Euler(0f, 0f, rotationZ);
            gameObject.SetActive(true);
            // rectTransform.sizeDelta = new Vector2(25, 25);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 25);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 25);
            
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