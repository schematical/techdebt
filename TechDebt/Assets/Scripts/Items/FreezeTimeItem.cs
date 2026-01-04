using System.Linq;
using Effects;

namespace Items
{
    public class FreezeTimeItem: ItemBase
    {
        public override string UseVerb()
        {
            return "Freeze Time";
        }

        public override void Use(NPCBase npc)
        {
            GameManager.Instance.NetworkPacketState = GameManager.GlobalNetworkPacketState.Frozen;
            GameManager.Instance.AddEffect(new FreezeTimeEffect());
            
            // Deactivate the nuke item after use so it can be pooled.
            gameObject.SetActive(false);
        }
    }
}