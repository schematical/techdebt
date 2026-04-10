using System.Collections.Generic;
using NPCs;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Infrastructure
{
    public class ProductRoadMap: InfrastructureInstance
    {
        public override void Initialize()
        {
            base.Initialize();
            attentionIconColor = Color.aquamarine;
        }

        public override void OnLeftClick(PointerEventData eventData)
        {
            if (!IsActive())
            {
                base.OnLeftClick(eventData);
                return;
            }

            UIProductRoadMap.State state = UIProductRoadMap.State.Display;
            if (GameManager.Instance.Map.GetCurrentLevel().State == MapLevel.MapLevelState.Completed)
            {
                state = UIProductRoadMap.State.Select;
            }
            GameManager.Instance.UIManager.productRoadMap.Show(state);
            HideAttentionIcon();
            
        }
    }
}