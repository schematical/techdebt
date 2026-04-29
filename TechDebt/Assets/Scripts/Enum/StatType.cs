// StatType.cs

public enum StatType
{
    Money,
    TechDebt,
    Traffic,
    PacketsSent,
    PacketsSucceeded,
    PacketsFailed,
    Difficulty,
    
    // Infrastructure Stats
    Infra_MaxLoad,
    Infra_LoadRecoveryRate,
    Infra_BuildTime,
    Infra_LoadPerPacket,
    Infra_DailyCost,
    
    //NPC
    NPC_DailyCost,
    NPC_FinOps,
    NPC_InfoSec,
    NPC_DevOpsQuality,
    NPC_Energy,
    NPC_MaxEnergy,
    
    Infra_PacketCost,
    
    
    ItemDropChance,
    EventCheckEverySeconds,
    NPC_MovmentSpeed,
    NPC_XPSpeed,
    NPC_DevOpsSpeed,
    NPC_ResearchSpeed,
    NPC_ModifierSlots,
    
    Infra_MaxSize,
    NPC_CodeQuality,
    NPC_CodeSpeed,
    NPC_HP,
    NPC_CoolDown,
    Infra_InputValidation,
    AttackPossibility,
    NPC_AttackDamage,
    TechDebt_AccumulationRate,
    
    NetworkPacket_Probibility,
    NetworkPacket_ValueMin,
    NetworkPacket_ValueMax,
    TotalNetworkPacketLatency,
    VictoryCondition_NetworkPacketLatency,
    NetworkPacket_LoadLatencyMultiplier,
    Infra_LatencyStartsAtLoad,
    AttackPossibilityAccumulationRate,
    NPC_FixSpeed,
    Release_Quality_Multiplier,
    NPC_Release_TechDebt,
    Global_CodeSpeed,
    Global_PIILossCost,
    Global_DeploymentSpeed,

    Global_ReRolls,
    Global_Banish,
    NPC_StartLevel,
    NPC_LevelUpRarity,
    NPC_ContractWorkMoneyMultiplier,
    NPC_BugAttackTechDebtMultiplier
}
