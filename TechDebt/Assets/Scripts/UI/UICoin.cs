using UnityEngine;

namespace UI
{
    public class UICoin: MonoBehaviour
    {
        void Update()
        {
            transform.position = new Vector3(transform.position.x, transform.position.y - Time.unscaledDeltaTime * 100, transform.position.z);
        }
    }
}