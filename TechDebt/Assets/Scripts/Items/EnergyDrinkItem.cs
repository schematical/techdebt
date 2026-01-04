using Effects;
using Stats;

namespace Items
{
    public class EnergyDrinkItem: ItemBase
    {
        public override string UseVerb()
        {
            return "Drink";
        }

        public override void Use(NPCBase npc)
        {
            StatModifier statModifier = new StatModifier(StatModifier.ModifierType.Multiply, 2f);
            GameManager.Instance.AddEffect(new EnergyDrinkEffect(npc, statModifier));
            npc.Stats.AddModifier(StatType.NPC_MovmentSpeed, statModifier);
            // Deactivate the nuke item after use so it can be pooled.
            gameObject.SetActive(false);
        }
    }
}