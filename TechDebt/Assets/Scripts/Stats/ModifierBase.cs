using System;
using System.Collections.Generic;
using Stats;

namespace NPCs
{
    public class ModifierBase: iModifierSource
    {
       
        

        public enum ModifierType
        {
            NPCStat,
            InfraStat
        }
        public StatType StatType { get; set; }
        public string Name { get; set; }
        public string Id { get; set; }

        public int Level { get; set; } = 1;

        public float BaseValue { get; set; } = 1.1f;
        
        public ModifierType Type { get; set; } =  ModifierType.NPCStat;
        
        public StatModifier StatModifier { get; private set; }

        public float GetScaledValue(int offsetLevel = 0)
        {
            return (float)Math.Pow(BaseValue, Level + offsetLevel);
        }

        public void Apply(NPCDevOps npc)
        {
            switch (Type)
            {
                case(ModifierType.NPCStat):
                    StatModifier = new StatModifier(
                        StatModifier.ModifierType.Multiply,
                        GetScaledValue(),
                        this
                    );
                    npc.Stats.AddModifier(StatType, StatModifier); 
                    break;
                case(ModifierType.InfraStat):
                    
                    break;
            }
        }
        public void OnInfrastructureBuild(InfrastructureInstance infrastructure)
        {
            
            StatModifier statModifier = new StatModifier(StatModifier.ModifierType.Multiply, GetScaledValue(), this);
            infrastructure.data.Stats.Get(StatType).ReplaceOrAdd(statModifier);


        }

        public string GetDisplayText(int offsetLevel = 0)
        {
            return $"{Name} Level: {Level + offsetLevel} - {StatType}  { Math.Round(GetScaledValue(offsetLevel) * 100) - 100}%";
        }
    }
}