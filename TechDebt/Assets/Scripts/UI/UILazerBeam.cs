using UnityEngine;

namespace UI
{
    public class UILazerBeam: MonoBehaviour
    {


        private float speed = 0.001f;
        private Vector2 startingAnchorMax;
        public RectTransform rectTransform;
        public float particleCounter = 0f;

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
            rectTransform.anchorMax = new Vector2(rectTransform.anchorMax.x, nextY);
            particleCounter += 0.01f;
            if (particleCounter >= 1f)
            {
                GameObject particleGO = GameManager.Instance.prefabManager.CreateRandomParticle(transform.position, transform);
                particleCounter = 0;
            }


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