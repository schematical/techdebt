using System.Collections.Generic;
using NPCs;
using UI;

namespace Stats
{
    public class ModifierCollection
    {
        public List<ModifierBase> Modifiers { get; private set; } = new List<ModifierBase>();

        public void Render(UIPanelLine line)
        {
            foreach (ModifierBase modifier in GameManager.Instance.Modifiers.Modifiers)
            {
                modifier.Render(line);
            }
        }
    }
}