using System.Collections.Generic;
using NPCs;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Infrastructure
{
    public class WorldObjectBase: MonoBehaviour, iModifierSource,   IPointerClickHandler
    {
        public virtual void OnPointerClick(PointerEventData eventData)
        {


            // This is where the tooltip logic should be handled.
            // We find the UIManager and tell it to show the tooltip for this specific instance.
            UIManager uiManager = GameManager.Instance.UIManager;
            if (uiManager != null)
            {
                uiManager.worldObjectDetailPanel.ShowWorldObjectDetail(this);
            }
        }

        public virtual List<NPCTask> GetAvailableTasks()
        {
            return new List<NPCTask>();
        }
        public virtual string GetDetailText()
             {
                 return gameObject.name;
             }
    }
    
}