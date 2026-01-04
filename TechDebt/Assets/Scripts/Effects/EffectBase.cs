using UnityEngine;

namespace Effects
{
    public class EffectBase
    {
        public float Count = 10;
        public virtual void FixedUpdate()
        {
            Count -= Time.fixedDeltaTime;
            if (Count <= 0)
            {
                OnFinish();
                GameManager.Instance.Effects.Remove(this);
            }
        }
        
        public virtual void OnFinish()
        {
        }
    }
}