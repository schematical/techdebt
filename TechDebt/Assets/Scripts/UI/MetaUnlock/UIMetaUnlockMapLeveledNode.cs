
using System.Collections.Generic;
using System.Linq;

namespace UI
{
    public class UIMetaUnlockLevelData
    {
        public string Id;
        public string DisplayName;
        public string Description;
        public int PrestigeCost;
        public StatType StatType;
        public float Value;
    }

    public class UIMetaUnlockMapLeveledNode : UIMetaUnlockMapNode
    {
        public List<UIMetaUnlockLevelData> Levels;

        public int CurrentLevelIndex
        {
            get
            {
                if (Levels == null || Levels.Count == 0) return -1;
                for (int i = Levels.Count - 1; i >= 0; i--)
                {
                    if (MetaGameManager.IsResourceEquipped(ResourceType, Levels[i].Id))
                        return i;
                }
                return -1;
            }
        }
        
    }
}