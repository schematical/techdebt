// NetworkConnectionBonus.cs
using System;
using Stats;

[Serializable]
public class NetworkConnectionBonus
{


    // public string Id;
    // public NetworkPacketData.PType PacketType;
    public StatType Stat = StatType.Infra_LoadPerPacket;
    public StatModifier.ModifierType Type = StatModifier.ModifierType.Multiply;
    public float value;
}