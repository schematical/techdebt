using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace Infrastructure
{
    public class WhiteBoard: InfrastructureInstance
    {
        public override void OnPointerClick(PointerEventData eventData)
        {
            List<ReleaseBase> releases = GameManager.Instance.GetOpenReleases();
            if (releases.Count > 0)
            {
                GameManager.Instance.UIManager.ShowAlert("You already have open releases that need to be finished first.");
                return;
            }
            GameManager.Instance.UIManager.MultiSelectPanel.Clear();
            // GameManager.Instance.UIManager.MultiSelectPanel.Clear();
            // MetaGameManager.G
            
        }
    }
}