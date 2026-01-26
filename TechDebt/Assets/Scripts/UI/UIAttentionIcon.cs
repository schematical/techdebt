using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class UIAttentionIcon: MonoBehaviour
    {
        public SpriteRenderer spriteRenderer;
        protected Transform targetTransform;
        public void Show(Transform _transform, Color color)
        {
            
            targetTransform = _transform;
            spriteRenderer.color = color;
        }

        void Update()
        {
            transform.position = targetTransform.position + new Vector3(0f, 2f, -1f); // Camera.main.WorldToScreenPoint(targetTransform.position + new Vector3(0f, 1f, -1f));
        }
    }
}