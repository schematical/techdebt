// NetworkConnectionBonus.cs
using System;
using Stats;

[Serializable]
public class NetworkConnectionBonus
{



    public NetworkPacketData.PType PacketType;
    public StatType Stat;
    public StatModifier.ModifierType Type;
    public float value;
}