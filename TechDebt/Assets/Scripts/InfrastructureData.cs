// InfrastructureData.cs
using UnityEngine;
using System;

[Serializable]
public class InfrastructureData
{
    public enum State { Locked, Unlocked, Planned, Operational }

    public string ID = Guid.NewGuid().ToString(); // Unique identifier for this instance
    public string DisplayName = "New Infrastructure";
    public string Type = "Generic"; // e.g., "WebServer", "Database", "LoadBalancer"
    public GameObject Prefab;
    public Vector2Int GridPosition;
    public UnlockCondition[] UnlockConditions;
    public float DailyCost = 100;
    public float BuildTime = 5f; // In seconds
    public bool IsInitiallyUnlocked = false;
    
    public State CurrentState = State.Locked;
    [HideInInspector] public GameObject Instance = null; // Reference to the instantiated GameObject
}
