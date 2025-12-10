// NetworkConnectionBonus.cs
using System;

[Serializable]
public class NetworkConnectionBonus
{
    public enum InfrStat
    {
        LoadPerPacket
    }
    public enum BonusType
    {
        Multiplier
    }

    public InfrStat Stat;
    public BonusType Type;
    public float value;
}