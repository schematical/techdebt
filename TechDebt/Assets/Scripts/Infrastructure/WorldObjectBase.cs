using System.Collections.Generic;
using NPCs;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Infrastructure
{
    public class WorldObjectBase: MonoBehaviour, iModifierSource,   IPointerClickHandler, iAssignable
    {
        public Color attentionIconColor = Color.white;
        public UIAttentionIcon uiAttentionIcon;
        public virtual void OnPointerClick(PointerEventData eventData)
        {

            if (eventData.button == PointerEventData.InputButton.Left)
            {
                OnLeftClick(eventData);
            }
          
        }

        public virtual void OnLeftClick(PointerEventData eventData)
        {
            
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
        public void ShowAttentionIcon()
        {
            if (uiAttentionIcon != null && uiAttentionIcon.gameObject.activeSelf)
            {
                Debug.LogWarning($"{gameObject.name} - uiAttentionIcon is already active");
                return;
            }
            uiAttentionIcon = GameManager.Instance.UIManager.AddAttentionIcon(
                transform,
                attentionIconColor,
                () =>
                {
                    GameManager.Instance.cameraController.ZoomTo(transform, () =>
                    {
                        OnPointerClick(new PointerEventData(EventSystem.current));
                    });
               
                }
            );
        }

        public void HideAttentionIcon()
        {
            if (uiAttentionIcon == null)
            {
                return;
            }
            uiAttentionIcon.gameObject.SetActive(false);
            uiAttentionIcon = null;
        }

        public virtual void Initialize()
        {
            /*transform.position = new Vector3(
                transform.position.x, 
                transform.position.y, 
                1 - transform.position.y * -0.1f // Setting Z for sorting order
            );*/
        }

        public virtual Vector3 GetInteractionPosition()
        {
            return transform.position;
        }
    }
    
}