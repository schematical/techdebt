using System.Collections.Generic;
using DefaultNamespace;
using DefaultNamespace.EnvGraphic;
using NPCs;
using UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Infrastructure
{
    public class WorldObjectBase: MonoBehaviour, iModifierSource,   IPointerClickHandler, iAssignable
    {
        public Vector3Int GridPosition;
        public Color attentionIconColor = Color.white;
        public Vector3 interactionPositionOffset = Vector3.zero;
        private UIAttentionIcon uiAttentionIcon;
        protected List<EnvGraphicBase> envGraphics = new List<EnvGraphicBase>();
        
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

        public virtual void UpdateFootPrint()
        {
            
            foreach (Vector3Int pos in GetFootPrint())
            {
                GameManager.Instance.gridManager.UpdateTileState(Vector3Int.FloorToInt(GridPosition + pos), true);
            }
        }

        public List<Vector3Int> GetFootPrint()
        {
            return new List<Vector3Int>()
            {
                new Vector3Int(0, 0)
            };
        }

        public virtual Vector3 GetInteractionPosition()
        {
            return transform.position + interactionPositionOffset;
        }

        public virtual LevelUpEnvGraphic ShowLevelUpGraphic(Rarity rarity, UnityAction _onDone = null)
        {
            LevelUpEnvGraphic levelUpEnvGraphic = GameManager.Instance.prefabManager.Create("LevelUpEnvGraphic",
                transform.position + new Vector3(0, 0, .1f)).GetComponent<LevelUpEnvGraphic>();
            GameManager.Instance.UIManager.SetTimeScalePause();
            levelUpEnvGraphic.Init(rarity, _onDone);
            return levelUpEnvGraphic;
        }

        public void ZoomTo()
        {
            Debug.Log("ZoomTo");
            GameManager.Instance.cameraController.ZoomTo(transform);
        }
        
    }
    
}