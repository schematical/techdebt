using UnityEngine;

namespace UI
{
    public class UIDialogBubble: UIPanel
    {
        public RectTransform pointer;
        protected NPCBase target;
        public void SetTarget(NPCBase target)
        {
            this.target = target;
        }
        // TODO The target is in world space. We need to make this render above their head and follow them around. 
    }
}