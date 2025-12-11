// NetworkPacketData.cs
using System;
using UnityEngine;
[Serializable]
public class NetworkPacketData
{
    public enum PType
    {
        Text,
        Image,
        Video,
        MaliciousText
    }
    public PType Type { get; private set; }
    public GameObject prefab;
    public float baseLoad = 20f;
    public float probilitly = 1f;
}