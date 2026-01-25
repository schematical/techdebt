using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace UI
{
    public class UILazerBeam: MonoBehaviour
    {

        public enum State { Running, ShuttingDown}
        private State state = State.Running;
        private float speed = 0.01f;
        private Vector2 startingAnchorMax;
        public RectTransform rectTransform;
        public Image image;
        protected float particleCounter = 0f;
        protected List<UILazarBeamParticle> particles = new List<UILazarBeamParticle>();
        public void Awake()
        {
            if (rectTransform == null)
            {
                rectTransform = GetComponent<RectTransform>();
            }
 
            startingAnchorMax = rectTransform.anchorMax;
        }

        public void Update()
        {
            switch (state)
            {
                case (State.Running):

                    float y = rectTransform.anchorMax.y;
                    float nextY = y + speed;
                    rectTransform.anchorMax = new Vector2(rectTransform.anchorMax.x, nextY);
                    particleCounter += 0.01f;
                    if (particleCounter >= 1f)
                    {
                        float halfRange = rectTransform.rect.width / 2;
                        Vector2 nextPosition = new Vector2(
                            rectTransform.position.x + Random.Range(-1 * halfRange, halfRange),
                            rectTransform.position.y);
                        GameObject particleGO =
                            GameManager.Instance.prefabManager.CreateRandomParticle(nextPosition, transform);
                        UILazarBeamParticle particle = particleGO.GetComponent<UILazarBeamParticle>();
                        particle.transform.SetParent(transform);
                        particle.Init(rectTransform.rotation.z);
                        particles.Add(particle);
                        particleCounter = Random.Range(0f, 10f) / 10;
                    }

                    break;
                case (State.ShuttingDown):
                    
                    
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

        public void Init(float rotationZ = 0)
        {
            state = State.Running;
            image.color = Color.white;
            /*if (
                rectTransform  != null &&
                startingAnchorMax != null)
            {
                rectTransform.anchorMax = startingAnchorMax;
            }*/
            
            rectTransform.rotation = Quaternion.Euler(0f, 0f, rotationZ);
            gameObject.SetActive(true);
            
        }

        public void Shutdown()
        {
            state = State.ShuttingDown;
            
            foreach (UILazarBeamParticle particle in particles)
            {
                particle.Shutdown();
            }
        }
    }
}