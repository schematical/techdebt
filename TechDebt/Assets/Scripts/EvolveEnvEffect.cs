using UnityEngine;
using UnityEngine.Events;

namespace DefaultNamespace
{
    public class EvolveEnvEffect:MonoBehaviour
    {
        protected UnityAction onConcealed;
        protected UnityAction onEnd;

        public void Initialize(UnityAction onConcealed, UnityAction onEnd)
        {
            this.onConcealed = onConcealed;
            this.onEnd = onEnd;
        }
        public void InvokeConcealed()
        {
            onConcealed.Invoke();
        }

        public void End()
        {
            onEnd.Invoke();
            gameObject.SetActive(false);
        }
    }
}