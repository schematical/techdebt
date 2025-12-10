// NetworkConnection.cs
using System;
using System.Collections.Generic;
[Serializable]
public class NetworkConnection
{
    public string TargetID;
    public int Priority = 5;

    public List<NetworkConnectionBonus> NetworkConnectionBonus = new List<NetworkConnectionBonus>();
}
