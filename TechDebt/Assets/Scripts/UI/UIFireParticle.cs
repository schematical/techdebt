using UnityEngine;

namespace UI
{
    public class UIFireParticle: MonoBehaviour
    {
        public Transform target;
        void Update()
        {
            transform.position = target.position + new Vector3(0, -0.5f, -0.1f);
        }
        public void MarkDone()
        {
            // gameObject.SetActive(false);
        }

        public void Initialize(Transform transform1)
        {
            target = transform1;
        }
    }
}