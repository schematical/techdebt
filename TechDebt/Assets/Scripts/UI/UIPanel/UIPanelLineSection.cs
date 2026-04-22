using UnityEngine;
using UnityEngine.Events;

namespace UI
{
    public abstract class UIPanelLineSection: MonoBehaviour
    {
        public RectTransform rectTransform;
        protected string Id;
        protected UnityAction<UIPanelLineSection> onFixedUpdate;
        public virtual void Initialize()
        {
            Id = null;
            transform.localScale = Vector3.one;
            onFixedUpdate = null;
       
        }
        protected void FixedUpdate()
        {
            if (onFixedUpdate != null)
            {
                onFixedUpdate.Invoke(this);
            }
        }

        public void OnFixedUpdate(UnityAction<UIPanelLineSection> _onFixedUpdate)
        {
            onFixedUpdate = _onFixedUpdate;
        }

        public void SetId(string id)
        {
            Id = id;
        }

        public string GetId()
        {
            return Id;
        }


        public void RefreshLayout()
        {
            
            UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
        }
    }
}