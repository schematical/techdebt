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

        // public int DisplayOffset = 0;

        public enum ModifierGroup
        {
            NPC,
            Release
        }

        public enum ModifierScaleDirection
        {
            Up,
            Down,
        }
        public ModifierGroup Group { get; set; } = ModifierGroup.NPC;
        public ModifierTarget  Target { get; set; } = ModifierTarget.NPC;
        public StatType StatType { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string IconPrefab { get; set; }
        public string Id { get; set; }
        
        public List<Rarity> Levels { get; set; } = new List<Rarity>();

        public float BaseValue { get; set; } = 1f;
        
        public ModifierType Type { get; set; } =  ModifierType.NPC_Stat;
        public ModifierScaleDirection ScaleDirection = ModifierScaleDirection.Up;
        public StatModifier StatModifier { get; private set; }
        public NetworkPacketData.PType NetworkPacketType;
        public Type InfraClassName { get; set; }
       
        public float GetScaledValue()
        {
            float percent = BaseValue;
            foreach (Rarity rarity in Levels)
            {
                percent *= GetScaledAdjustmentValue(rarity);
            }
            return percent;
        }

        public float GetScaledAdjustmentValue(Rarity rarity)
        {
            float scaleValue =  RarityHelper.GetDefaultScaleValue(rarity);
            switch (ScaleDirection)
            {
                case ModifierScaleDirection.Up:
                    break;
                case ModifierScaleDirection.Down:
                    scaleValue = (1 - scaleValue) + 1;
                    break;
                default:
                    throw new SystemException("Unknown scale direction: " + ScaleDirection);
            }

            return scaleValue;
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

        public string GetTitle()
        {
            return $"{Name}";
        }
        public string GetNextLevelUpDisplayText(Rarity nextLevelRarity)
        {
            string text = $"Level: {Levels.Count + 1}\n";
            float percent = GetScaledValue();
            float increasePercent = GetScaledAdjustmentValue(nextLevelRarity);
            if (Levels.Count == 0)
            {
                switch (ScaleDirection)
                {
                    //   text += $"{Math.Round(percent * increasePercent * 100)}
                    case ModifierScaleDirection.Up:
                        text += $"{Math.Round(percent * increasePercent * 100) - 100}%";
                        break;
                    case ModifierScaleDirection.Down:
                        text += $"{100 - Math.Round(percent * increasePercent * 100)}%";
                        break;
                    default:
                        throw new SystemException("Unknown scale direction: " + ScaleDirection);
                }
                return text;
            }
            float nextPercent = percent * increasePercent;
            text += $"{nextPercent} = {percent} * {increasePercent}\n";
            switch (ScaleDirection)
            {
                  
                    // text += $"{Math.Round(percent * 100)}% => {Math.Round(nextPercent * 100)}%";
                case ModifierScaleDirection.Up:
                    text += $"{Math.Round(percent * 100) - 100}% => {Math.Round(nextPercent * 100) - 100}%";
                    break;
                case ModifierScaleDirection.Down:
                    text += $"{100 - Math.Round(percent * 100)}% => {100 -Math.Round(nextPercent * 100)}%";
                    break;
                default:
                    throw new SystemException("Unknown scale direction: " + ScaleDirection);
            }
         
            return text;
        }
        
        public int LevelUp(Rarity rarity)
        {
            /*string debug = "Levels.Count Before: " +  Levels.Count + "\n" +
                           "Rarity: " + rarity + "\n";*/
            
            
            
            
            Levels.Add(rarity);
            
            
            
            /*debug += "Levels.Count After: " + Levels.Count + "\n";
            for(int i = 0; i < Levels.Count; i++){
                    debug += "Level " + i + ": " + Levels[i] + "\n";
            }
            Debug.Log("Level up: \n" + debug);*/
            return Levels.Count;
        }

        public int GetLevel()
        {
            return Levels.Count;
        }

        public string ToFullDetail()
        {
            string text = GetTitle();
            text += "Level " + Levels.Count + "\n";
            text += $"BaseValue: {BaseValue}\n";
            float percent = BaseValue;
            for(int i = 0; i < Levels.Count; i++)
            {
                float scaledValue = GetScaledAdjustmentValue(Levels[i]);
                percent *= scaledValue;
                text += $" - Level {i} - {Levels[i]} -  {ScaleDirection} - Scale: {scaledValue} - Result: {percent}\n";
            }
            text += $"Scaled Value: {GetScaledValue()}\n";
            return text;
        }
    }
}