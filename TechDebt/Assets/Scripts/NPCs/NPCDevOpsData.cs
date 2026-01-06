// NPCDevOpsData.cs
using System;
using Stats;
using UnityEngine;

[Serializable]
public class NPCDevOpsData
{
    public string ID = Guid.NewGuid().ToString();
    public string Name = "DevOps " + new System.Random().Next(100, 999);
    public float DailyCost = 100f; // Salary
    public StatsCollection Stats { get; private set; } = new StatsCollection();


    public void InitializeStats()
    {
        
        // Stats.Add(new StatData(StatType.NPC_DailyCost, DailyCost));
    }
}
