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
        public enum State {Rising, /*Falling, Fading*/}

        protected State state;
        public RectTransform rectTransform;
        public List<Sprite> sprites;
        public SpriteRenderer spriteRenderer;
        void Update()
        {
            switch (state)
            {
                case(State.Rising):
                    // First, move the particle
                    rectTransform.position = new Vector3(rectTransform.position.x, rectTransform.position.y + (10 * Time.unscaledDeltaTime), rectTransform.position.z);
                    
                    // Then, get its new world position and convert to screen coordinates to check bounds
                    Vector3[] corners = new Vector3[4];
                    rectTransform.GetWorldCorners(corners);

                    // We use a bottom corner (e.g., bottom-left at index 0) to see if the whole particle is off-screen.
                    Vector3 bottomCornerScreenPos = Camera.main.WorldToScreenPoint(corners[0]);

                    if (bottomCornerScreenPos.y > Screen.height + 100)
                    {
                        if (Random.Range(0, 5) == 0)
                        {
                            CreateUIScreenParticle();
                        }

                        gameObject.SetActive(false);
                    }
                    break;
                /*case(State.Falling):
              
                    rectTransform.position = new Vector3(rectTransform.position.x, rectTransform.position.y - 1f, rectTransform.position.z);
                    if (bottomRight.y < 0)
                    {
                       
                        gameObject.SetActive(false);
                    }
                    break;
             case(State.Fading):
                 
                    rectTransform.position = new Vector3(rectTransform.position.x, rectTransform.position.y - 1f, rectTransform.position.z);
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
                    break;*/
            }
        }

        public UIScreenParticle CreateUIScreenParticle()
        {
      
         
            float newX =  Random.Range(0, Screen.width / 4);
            if (Random.Range(0, 2) > 0)
            {
                newX = Screen.width - newX;
            }
            Vector2 startPosition = new Vector2(newX, Screen.height);
            GameObject particleGO =
                GameManager.Instance.prefabManager.Create("UIScreenParticle", startPosition, GameManager.Instance.UIManager.transform);
            UIScreenParticle particle = particleGO.GetComponent<UIScreenParticle>();
            particle.Init(spriteRenderer.sprite);
            return particle;
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
            rectTransform.position = new Vector3(
                rectTransform.position.x,
                rectTransform.position.y,
                0.1f // rectTransform.position.z - 0.05f
            );
            // rectTransform.rotation = Quaternion.Euler(0f, 0f, rotationZ);
            gameObject.SetActive(true);
            /*// rectTransform.sizeDelta = new Vector2(25, 25);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 25);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 25);*/
            
        }

        /*public void Shutdown()
        {
            // state = State.Fading;
            
            /*switch (state)
            {
                case (State.Fading):
                    break;

            }#1#
        }*/
    }
}