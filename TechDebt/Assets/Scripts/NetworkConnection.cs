// NetworkConnection.cs
using System;
using System.Collections.Generic;
using Stats;

[Serializable]
public class NetworkConnection
{
    public string TargetID;
    public int Priority = 5;
    public NetworkPacketData.PType networkPacketType;
    public List<NetworkConnectionBonus> NetworkConnectionBonus = new List<NetworkConnectionBonus>();
    public StatsCollection Stats = new StatsCollection();
    public NetworkConnection(string targetID, NetworkPacketData.PType _networkPacketType)
    {
        TargetID = targetID;
        networkPacketType = _networkPacketType;
    }
}
