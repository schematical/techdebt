using System;
using System.Collections.Generic;
using Infrastructure;
using JetBrains.Annotations;
using Stats;
using UnityEngine;

namespace NPCs
{
    public class ModifierBase: iModifierSource
    {
       
        

        public enum ModifierType
        {
            NPC_Stat,
            NPC_InfraStat,
            Infra_NetworkPacketStat,
            Run_Stat
        }
        public enum ModifierTarget
        {
            Run,
            NPC,
            InfraClass, // Applies it across all infra of that type.
        }

        public enum ModifierGroup
        {
            NPC,
            Release
        }
        public ModifierGroup Group { get; set; } = ModifierGroup.NPC;
        public ModifierTarget  Target { get; set; } = ModifierTarget.NPC;
        public StatType StatType { get; set; }
        public string Name { get; set; }
        public string Id { get; set; }

        public int Level { get; set; } = 1;

        public float BaseValue { get; set; } = 1.1f;
        
        public ModifierType Type { get; set; } =  ModifierType.NPC_Stat;
        
        public StatModifier StatModifier { get; private set; }
        public NetworkPacketData.PType NetworkPacketType;
        public Type InfraClassName { get; set; }

        public float GetScaledValue(int offsetLevel = 0)
        {
            return (float)Math.Pow(BaseValue, Level + offsetLevel);
        }

        public void Apply(NPCDevOps npc = null)
        {
            switch (Type)
            {
                case(ModifierType.NPC_Stat):
                    StatModifier = new StatModifier(
                        StatModifier.ModifierType.Multiply,
                        GetScaledValue(),
                        this
                    );
                    npc.Stats.AddModifier(StatType, StatModifier); 
                    break;
                case(ModifierType.NPC_InfraStat):
                    
                    break;
                case(ModifierType.Infra_NetworkPacketStat):
                    StatModifier = new StatModifier(
                        StatModifier.ModifierType.Multiply,
                        GetScaledValue(),
                        this
                    );
                    List<InfrastructureInstance> instances = GameManager.Instance.GetInfrastructureInstancesByType(this.InfraClassName);
                    foreach (InfrastructureInstance inst in instances)
                    {
                        
                        foreach (InfrastructureDataNetworkPacket networkPacketData in inst.data.networkPackets)
                        {
                            if (networkPacketData.PacketType == NetworkPacketType)
                            {
                                Debug.Log($"[DEBUG] {inst.data.ID} Applying modifier to {inst.gameObject.name} for packet type {networkPacketData.PacketType}. Stat count: {networkPacketData.Stats.Stats.Count}");
                                networkPacketData.Stats.AddModifier(this.StatType, StatModifier);
                            }
                        }
                    }
                    break;
                case(ModifierType.Run_Stat):
                    StatModifier = new StatModifier(
                        StatModifier.ModifierType.Multiply,
                        GetScaledValue(),
                        this
                    );
                    GameManager.Instance.Stats.AddModifier(StatType, StatModifier);
                    
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
        public int LevelUp()
        {
            Level++; 
            return Level;
        }
    }
}