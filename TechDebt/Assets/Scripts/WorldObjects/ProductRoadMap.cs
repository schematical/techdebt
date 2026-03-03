using System.Collections.Generic;
using NPCs;
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
            GameManager.Instance.UIManager.productRoadMap.Show();
            HideAttentionIcon();
            
        }
    }
}