using UnityEngine;

namespace Effects.Infrastructure
{
    public class EnvEffectBase : MonoBehaviour
    {
        public int loopFor = -1; // -1 means loop forever
        private int loopCount = 0;

        private void OnEnable()
        {
            // Reset loop count when the object is re-enabled from the pool
            loopCount = 0;
        }

        // This method will be called by an Animation Event
        public void OnAnimationLoopComplete()
        {
            if (loopFor == -1) return; // Loop forever

            loopCount++;
            if (loopCount >= loopFor)
            {
                gameObject.SetActive(false);
            }
        }
    }
}