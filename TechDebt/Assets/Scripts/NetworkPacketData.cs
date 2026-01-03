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
        MaliciousText
    }

    public PType Type;
    public GameObject prefab;
    public float baseLoad = 20f;
    public float probilitly = 1f;
    // public int incomePerPacket = 0;
}