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
       
        }

        public void SetId(string id)
        {
            Id = id;
        }

        public string GetId()
        {
            return Id;
        }


    }
}