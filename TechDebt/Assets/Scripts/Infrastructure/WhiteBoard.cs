using UnityEngine.EventSystems;

namespace Infrastructure
{
    public class WhiteBoard: InfrastructureInstance
    {
        public override void OnPointerClick(PointerEventData eventData)
        {
            GameManager.Instance.UIManager.deskMenuPanel.gameObject.SetActive(true);
        }
    }
}