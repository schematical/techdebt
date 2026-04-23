using System.Collections.Generic;

namespace UI
{
    public class UIMetaUnlockMapNode : iUIMapNode
    {
        public string Id { get; set; }
        public virtual string DisplayName { get; set; }
        public virtual string Description { get; set; }
        public MapNodeDirection Direction { get; set; }
        public List<string> DependencyIds { get; set; }
        public virtual int PrestigeCost { get; set; }
        public string RewardId { get; set; }
        public MapNodeState? CurrentState { get; set; }
        public float GetProgress() => 0;

        public UnityEngine.Tilemaps.TileBase GetTile()
        {
            string tileId = "TechTreeLockedTile";
            switch (CurrentState)
            {
                case MapNodeState.MetaLocked:
                    tileId = "TechTreeLockedTile";
                    break;
                case MapNodeState.Locked:
                    tileId = "TechTreeUnlockedTile";
                    break;
                case MapNodeState.Active:
                    tileId = "TechTreeResearching";
                    break;
                case MapNodeState.Unlocked:
                    tileId = "TechTreeResearched";
                    break;
            }
            return GameManager.Instance.prefabManager.GetTile(tileId);
        }

        public void OnSelected(UIMapPanel panel)
        {
        }

       
    }
}