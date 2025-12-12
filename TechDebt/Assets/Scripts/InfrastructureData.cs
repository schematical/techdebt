// InfrastructureData.cs
using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class InfrastructureData
{
    public enum State { Locked, Unlocked, Planned, Operational, Frozen }

    public string ID = Guid.NewGuid().ToString(); // Unique identifier for this instance
    public string DisplayName = "New Infrastructure";
    public string Type = "Generic"; // e.g., "WebServer", "Database", "LoadBalancer"
    public GameObject Prefab;
    public Vector2Int GridPosition;
    public UnlockCondition[] UnlockConditions;
    public float DailyCost = 100;
    public float BuildTime = 5f; // In seconds
    public float loadPerPacket = 20f;
    public List<InfrastructureDataNetworkPacket> networkPackets = new List<InfrastructureDataNetworkPacket>();
    public float maxLoad = 100;

    public float loadRecoveryRate = 50f;
    
    public State CurrentState = State.Locked;
    public NetworkConnection[] NetworkConnections; // Array of NetworkConnection objects
}

[Serializable]
public class InfrastructureDataNetworkPacket
{
    public NetworkPacketData.PType PacketType;
    public int loadPerPacket = 20;
}