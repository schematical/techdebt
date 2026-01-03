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
    public int Possiblity = 1;
    public int Cost = 0;
    public List<NetworkConnectionBonus> NetworkConnectionBonus = new List<NetworkConnectionBonus>();
  
    public NetworkConnection(string targetID, NetworkPacketData.PType _networkPacketType)
    {
        TargetID = targetID;
        networkPacketType = _networkPacketType;

    }
}
