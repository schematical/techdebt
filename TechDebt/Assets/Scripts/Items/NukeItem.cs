using System.Collections.Generic;
using System.Linq;
using NPCs;

namespace Items
{
    public class NukeItem: ItemBase
    {
        public override string UseVerb()
        {
            return "Detonate";
        }

        public override void Use(NPCBase npc)
        {
            // Create a copy of the list to iterate over, as DestroyPacket modifies the original list.
            List<NPCBase> npcBugs  = GameManager.Instance.AllNpcs.FindAll((npc => npc.GetComponent<NPCBug>()));
            
            foreach (NPCBase npcBug  in npcBugs)
            {
                npcBug.GetComponent<NPCBug>().ReceiveDamage(100);
            }
            
            // Deactivate the nuke item after use so it can be pooled.
            gameObject.SetActive(false);
        }
    }
}