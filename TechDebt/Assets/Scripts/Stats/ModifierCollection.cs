using System.Collections.Generic;
using NPCs;

namespace Stats
{
    public class ModifierCollection
    {
        public List<ModifierBase> Modifiers { get; private set; } = new List<ModifierBase>();

        public void Render(UIPanel uiPanel)
        {
            foreach (ModifierBase modifier in GameManager.Instance.Modifiers.Modifiers)
            {
                modifier.Render(uiPanel);
            }
        }
    }
}