using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace UI
{
    public class UILazerBeam: MonoBehaviour
    {


        private float speed = 0.01f;
        private Vector2 startingAnchorMax;
        public RectTransform rectTransform;
        public float particleCounter = 0f;
      
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

            float y = rectTransform.anchorMax.y;
            float nextY = y + speed;
            rectTransform.anchorMax = new Vector2(rectTransform.anchorMax.x, nextY);
            particleCounter += 0.01f;
            if (particleCounter >= 1f)
            {
                float halfRange = rectTransform.rect.width / 2;
                Vector2 nextPosition = new Vector2(
                    rectTransform.position.x + Random.Range(-1 * halfRange, halfRange), rectTransform.position.y);
                GameObject particleGO = GameManager.Instance.prefabManager.CreateRandomParticle(nextPosition, transform);
                UILazarBeamParticle particle = particleGO.GetComponent<UILazarBeamParticle>();
                particle.Init(rectTransform.rotation.z);
                particleCounter = Random.Range(0f, 10f)/10;
            }


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