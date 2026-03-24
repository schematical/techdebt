using UnityEngine;

namespace UI
{
    public abstract class UIPanelLineSection: MonoBehaviour
    {
        public RectTransform rectTransform;
        protected string Id;
        public virtual void Initialize()
        {
            Id = null;
            transform.localScale = Vector3.one;
       
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