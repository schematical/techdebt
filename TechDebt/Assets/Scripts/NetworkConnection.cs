// NetworkConnection.cs
using System;
using System.Collections.Generic;
using Infrastructure;
using Stats;
using UnityEngine.Serialization;

[Serializable]
public class NetworkConnection
{
    public WorldObjectType.Type worldObjectType;
    [FormerlySerializedAs("Priority")] public int priority = 5;
    public NetworkPacketData.PType networkPacketType;
    [FormerlySerializedAs("Possiblity")] public int possiblity = 1;
    [FormerlySerializedAs("Cost")] public int cost = 0;
    [FormerlySerializedAs("NetworkConnectionBonus")] public List<NetworkConnectionBonus> networkConnectionBonus = new List<NetworkConnectionBonus>();
  
   
}
