// NetworkPacketData.cs
using System;
using UnityEngine;
[Serializable]
public class NetworkPacketData
{
    [Serializable]
    public enum PType
    {
        Text,
        Image,
        BatchJob,
        MaliciousText,
        Purchase
    }

    public PType Type;
    public string prefabId;
    public float baseLoad = 20f;
    public float probilitly = 1f;
    // public int incomePerPacket = 0;
}