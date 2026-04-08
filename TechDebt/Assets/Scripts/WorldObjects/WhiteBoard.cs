using System.Collections.Generic;
using DefaultNamespace.Rewards;
using NPCs;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Infrastructure
{
    public class WhiteBoard : InfrastructureInstance
    {
        public override void Initialize()
        {
            base.Initialize();
            attentionIconColor = Color.green;
            if (IsActive())
            {
                ShowAttentionIcon();
            }
        }

        public override void SetState(InfrastructureData.State newState)
        {
            if (data.CurrentState == newState) return;
            base.SetState(newState);
            switch (newState)
            {
                case(InfrastructureData.State.Operational):
                    ShowAttentionIcon();
                    break;
            }
            
        }
        public override void OnLeftClick(PointerEventData eventData)
        {
            if (!IsActive())
            {
                base.OnLeftClick(eventData);
                return;
            }
            GameManager.Instance.UIManager.releaseHistoryPanel.Show();
        }
        public override Vector3 GetInteractionPosition(InteractionType interactionType = InteractionType.Basic)
        {
            switch (interactionType)
            {
                case(InteractionType.Basic):
                    return transform.position + new Vector3(1, 0, 0);
                default:
                    return base.GetInteractionPosition(interactionType);
            }
           
        }
        public override UIMetricsBubble ShowMetricsBubble()
        {
            return null;
        }
    }
}