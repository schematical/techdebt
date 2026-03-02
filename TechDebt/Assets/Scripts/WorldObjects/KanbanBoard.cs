using UnityEngine.EventSystems;

namespace Infrastructure
{
    public class KanbanBoard: InfrastructureInstance
    {
        public override void OnLeftClick(PointerEventData eventData)
        {
            if (!IsActive())
            {
                base.OnLeftClick(eventData);
                return;
            }
            GameManager.Instance.UIManager.taskListPanel.Show();
            HideAttentionIcon();
        }
    }
}