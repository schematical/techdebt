using System.Linq;

namespace Items
{
    public class NukeItem: ItemBase
    {
        public override string UseVerb()
        {
            return "Detonate";
        }

        public override void Use()
        {
            // Create a copy of the list to iterate over, as DestroyPacket modifies the original list.
            var packetsToDestroy = GameManager.Instance.activePackets.ToList();
            
            foreach (var packet in packetsToDestroy)
            {
                GameManager.Instance.DestroyPacket(packet);
            }
            
            // Deactivate the nuke item after use so it can be pooled.
            gameObject.SetActive(false);
        }
    }
}