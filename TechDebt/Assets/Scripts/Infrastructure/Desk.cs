// Server.cs

using System;
using UnityEngine;

public class Desk : InfrastructureInstance
{
    public int lastDisplayedProgress = -1;
    public void OnResearchProgress(Vector3 position)
    {
        int progress = (int)Math.Floor(GameManager.Instance.CurrentlyResearchingTechnology.CurrentResearchProgress /
            GameManager.Instance.CurrentlyResearchingTechnology.ResearchPointCost * 100);
        // Emit a "+1" text particle at the given position.
        if (progress % 10 == 0 && lastDisplayedProgress != progress)
        {
            FloatingTextFactory.Instance.ShowText($"{progress}%", position, new Color(0.2f, 0.6f, 1f));//  + new Vector3(0, 1, 3));
            lastDisplayedProgress = progress;
        }
    }
}