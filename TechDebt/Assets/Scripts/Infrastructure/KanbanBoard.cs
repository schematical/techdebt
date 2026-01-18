using UnityEngine.EventSystems;

namespace Infrastructure
{
    public class KanbanBoard: InfrastructureInstance
    {
        public override void OnPointerClick(PointerEventData eventData)
        {
            GameManager.Instance.UIManager.ToggleTaskListPanel();
        }
    }
}