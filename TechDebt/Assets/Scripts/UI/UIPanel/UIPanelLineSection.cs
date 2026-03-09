using UnityEngine;

namespace UI
{
    public abstract class UIPanelLineSection: MonoBehaviour
    {
        public RectTransform rectTransform;
        protected string Id;
        public virtual void Initialize()
        {
            /*if (options.width != null)
            {
                rectTransform.anchorMax = new Vector2(rectTransform.anchorMax.x, options.width);
            }*/
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