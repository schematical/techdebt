using System.Collections.Generic;
using NPCs;
using UI;

namespace Stats
{
    public class ModifierCollection
    {
        public List<RewardBase> Rewards { get; private set; } = new List<RewardBase>();


        public void Render(UIPanelLine line)
        {
            foreach (RewardBase rewardBase in Rewards)
            {
                rewardBase.Render(line);
            }
        }
    }
}