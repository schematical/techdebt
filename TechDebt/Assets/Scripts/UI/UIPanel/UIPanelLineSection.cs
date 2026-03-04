using UnityEngine;

namespace UI
{
    public abstract class UIPanelLineSection: MonoBehaviour
    {
        public RectTransform rectTransform;

        public virtual void Initialize(UIPanelLineSectionOptions options)
        {
            /*if (options.width != null)
            {
                rectTransform.anchorMax = new Vector2(rectTransform.anchorMax.x, options.width);
            }*/
        }
    }
}