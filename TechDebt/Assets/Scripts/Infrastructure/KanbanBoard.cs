using UnityEngine.EventSystems;

namespace Infrastructure
{
    public class KanbanBoard: InfrastructureInstance
    {
        public override void OnLeftClick(PointerEventData eventData)
        {
            GameManager.Instance.UIManager.taskListPanel.Show();
            HideAttentionIcon();
        }
    }
}