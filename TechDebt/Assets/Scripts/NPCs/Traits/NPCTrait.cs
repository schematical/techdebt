using System;
using System.Collections.Generic;
using Stats;

namespace NPCs
{
    public class NPCTrait
    {
       
        

        public enum TraitType
        {
            NPCStat,
            InfraStat
        }
        public StatType StatType { get; set; }
        public string Name { get; set; }

        public int Level { get; set; } = 1;

        public float BaseValue { get; set; } = 1.1f;
        
        public TraitType Type { get; set; } =  TraitType.NPCStat;
        
        public StatModifier NPCStatModifier { get; private set; }

        public float GetScaledValue()
        {
            return (float)Math.Pow(BaseValue, Level);
        }

        public void Apply(NPCDevOps npc)
        {
            switch (Type)
            {
                case(TraitType.NPCStat):
                    NPCStatModifier = new StatModifier(
                        StatModifier.ModifierType.Multiply,
                        GetScaledValue(),
                        this
                    );
                    npc.Stats.AddModifier(StatType, NPCStatModifier); 
                    break;
                case(TraitType.InfraStat):
                    
                    break;
            }
        }
        public void OnInfrastructureBuild(InfrastructureInstance infrastructure)
        {
            
            StatModifier statModifier = new StatModifier(StatModifier.ModifierType.Multiply, GetScaledValue(), this);
            infrastructure.data.Stats.Get(StatType).ReplaceOrAdd(statModifier);


        }

        public string GetDisplayText()
        {
            return $"{Name} - {StatType} + { Math.Round(GetScaledValue() * 100) - 100}%";
        }
    }
}