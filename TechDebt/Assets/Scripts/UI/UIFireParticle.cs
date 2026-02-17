using UnityEngine;

namespace UI
{
    public class UIFireParticle: MonoBehaviour
    {
        public void MarkDone()
        {
            gameObject.SetActive(false);
        }
    }
}