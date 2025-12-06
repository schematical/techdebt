// NPCDevOpsData.cs
using System;

[Serializable]
public class NPCDevOpsData
{
    public string ID = Guid.NewGuid().ToString();
    public string Name = "DevOps " + new Random().Next(100, 999);
    public float DailyCost; // Salary
}
