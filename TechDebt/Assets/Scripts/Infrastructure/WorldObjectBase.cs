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
        public PolygonCollider2D polygonCollider2D;

        void Start()
        {
            if (polygonCollider2D == null)
            {
              
                PolygonCollider2D collider = gameObject.AddComponent<PolygonCollider2D>();
                SpriteRenderer spriteRenderer = GetComponentInChildren<SpriteRenderer>();
                var sprite = spriteRenderer.sprite;
                var pivot = new Vector2(
                    sprite.pivot.x / sprite.pixelsPerUnit,
                    sprite.pivot.y / sprite.pixelsPerUnit
                );
                
                /*collider.points = new Vector2[4]{
                    spriteRenderer.bounds.min,
                    new Vector2(spriteRenderer.bounds.max.x, spriteRenderer.bounds.min.y),
                    spriteRenderer.bounds.max,
                    new Vector2(spriteRenderer.bounds.min.x, spriteRenderer.bounds.max.y)
                };*/
                collider.points = new Vector2[4]{
                    Vector2.zero - pivot,
                    new Vector2(0, spriteRenderer.bounds.size.y) - pivot,
                    (Vector2)spriteRenderer.bounds.size - pivot,
                    new Vector2(spriteRenderer.bounds.size.x, 0) - pivot,
                };
            }
        }
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

        public virtual LevelUpEnvGraphic ShowLevelUpGraphic(Rarity rarity, UnityAction<Rarity, bool> _onDone = null)
        {
            LevelUpEnvGraphic levelUpEnvGraphic = GameManager.Instance.prefabManager.Create("LevelUpEnvGraphic",
                transform.position + new Vector3(0, 0, .1f)).GetComponent<LevelUpEnvGraphic>();
            GameManager.Instance.UIManager.SetTimeScalePause();
            levelUpEnvGraphic.Init(rarity, _onDone);
            return levelUpEnvGraphic;
        }

        public void ZoomTo()
        {
            GameManager.Instance.cameraController.ZoomTo(transform);
        }
        
    }
    
}