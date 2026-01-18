using System.Collections.Generic;
using NPCs;

namespace Stats
{
    public class ModifierCollection
    {
        public List<ModifierBase> Modifiers { get; private set; } = new List<ModifierBase>();
    }
}