// StatType.cs

public enum StatType
{
    Money,
    TechDebt,
    Traffic,
    PacketsSent,
    PacketsServiced,
    PRR, // PacketsRatioRequirements,
    PacketsFailed,
    PacketIncome,
    Difficulty,
    
    // Infrastructure Stats
    Infra_MaxLoad,
    Infra_LoadRecoveryRate,
    Infra_BuildTime,
    Infra_LoadPerPacket,
    Infra_DailyCost,
    
    //NPC
    NPC_DailyCost
}
