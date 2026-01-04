using Stats;

namespace Effects
{
    public class EnergyDrinkEffect: EffectBase
    {
        private NPCBase npc;
        StatModifier statModifier;

        public EnergyDrinkEffect(NPCBase _npc, StatModifier _statModifier)
        {
            npc = _npc;
            statModifier = _statModifier;
        }
        public override void OnFinish()
        {
            npc.Stats.RemoveModifier(StatType.NPC_MovmentSpeed, statModifier);
        }
    }
}