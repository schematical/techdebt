using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class UIScreenParticle: MonoBehaviour
    {
        public enum State { Falling, Fading}

        public enum Effects
        {
            Fire,
            Smoke,
            Lightning
        };
        public List<Effects> activeEffects = new List<Effects>();
        public Dictionary<Effects, float> effectCooldowns = new Dictionary<Effects, float>();
        protected State state;
        public RectTransform rectTransform;
        public Image image;
        public UIFireParticle fireParticle;
        void Update()
        {
            Vector3[] corners = new Vector3[4];
            rectTransform.GetWorldCorners(corners);
          
            // Bottom-right corner is corners[2]
            Vector3 bottomRight = corners[2];
            switch (state)
            {
              
                case(State.Falling):
              
                    rectTransform.position = new Vector2(rectTransform.position.x, rectTransform.position.y - Time.unscaledDeltaTime * 400f);
                    if (bottomRight.y < -100)
                    {
                       
                        gameObject.SetActive(false);
                        if (fireParticle != null)
                        {
                            fireParticle.gameObject.SetActive(false);
                        }
                    }
                    break;
             case(State.Fading):
                 
                    rectTransform.position = new Vector2(rectTransform.position.x, rectTransform.position.y - Time.unscaledDeltaTime * 400f);
                    image.color = new Color(
                        image.color.r,
                        image.color.g,
                        image.color.b,
                        image.color.a - ( Time.unscaledDeltaTime)
                    );
                    if (image.color.a <= 0)
                    {
                        gameObject.SetActive(false);
                        if (fireParticle != null)
                        {
                            fireParticle.gameObject.SetActive(false);
                        }
                    }
                    break;
            }

            /*if (activeEffects.Contains(Effects.Fire))
            {
                float cooldown = 0;
                if (effectCooldowns.ContainsKey(Effects.Fire))
                {
                    cooldown = effectCooldowns[Effects.Fire];
                }

                cooldown  -= Time.unscaledDeltaTime;
                if (cooldown < 0)
                {
                
                    Vector3 pos = rectTransform.position + new Vector3(Random.Range(-20, 20), Random.Range(-20, 20), 0f);
                    GameManager.Instance.prefabManager.Create("UIFireParticle", pos, GameManager.Instance.UIManager.transform);
                    cooldown = 0.1f;
                }
                effectCooldowns[Effects.Fire] = cooldown;
            }*/

            
        }
        public void Init(Sprite sprite, float rotationZ = 0, List<Effects> _activeEffects = null)
        {
            image.color = Color.white;
            image.sprite = sprite;
            state = State.Falling;
            rectTransform.rotation = Quaternion.Euler(0f, 0f, rotationZ);
            gameObject.SetActive(true);
            if (_activeEffects != null)
            {
                activeEffects = _activeEffects;
            }
            // rectTransform.sizeDelta = new Vector2(25, 25);
            // rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 25);
            // rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 25);
            
            // rectTransform.sizeDelta = new Vector2(100, 100);
            float ratio = sprite.rect.width / sprite.rect.height;
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 100 * ratio);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 100);
            if (activeEffects.Contains(Effects.Fire))
            {
                fireParticle = GameManager.Instance.prefabManager.Create("UIFireParticle", transform.position, GameManager.Instance.UIManager.transform).GetComponent<UIFireParticle>();
                fireParticle.Initialize(this.transform);
            }
   
                        
          
            
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