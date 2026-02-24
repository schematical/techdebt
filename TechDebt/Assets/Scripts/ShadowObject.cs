using UnityEngine;

namespace DefaultNamespace
{
    public class ShadowObject: MonoBehaviour
    {
        public GameObject target;
        public Vector2 offset =  Vector2.zero;
        void Update()
        {
            if (target == null || !target.activeInHierarchy)
            {
                gameObject.SetActive(false);
                return;
            }

            transform.position = target.transform.position + new Vector3(offset.x,offset.y, 1);
        }

        public void Initialize(GameObject _target,  Vector2 _offset = default)
        {
            target = _target;
            offset = _offset;
        }
    }
}