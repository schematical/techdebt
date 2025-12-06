// InfrastructureData.cs
using UnityEngine;
using System;

[Serializable]
public class InfrastructureData
{
    public string ID = Guid.NewGuid().ToString(); // Unique identifier for this instance
    public string DisplayName = "New Infrastructure";
    public string Type = "Generic"; // e.g., "WebServer", "Database", "LoadBalancer"
    public GameObject Prefab;
    public Vector2Int GridPosition;
    public float UnlockCost = 500.0f; // One-time cost to purchase
    public float DailyCost = 10.0f; // Cost in Money per day
    public bool IsInitiallyUnlocked = false; // Whether it's active from the start
    
    [HideInInspector] public bool IsUnlockedInGame = false; // Managed at runtime
    [HideInInspector] public GameObject Instance = null; // Reference to the instantiated GameObject
}
