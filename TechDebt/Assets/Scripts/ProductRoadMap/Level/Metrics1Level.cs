using UnityEngine;

public class Metrics1Level : MapLevel
{
    public Metrics1Level() : base()
    {
        Name = "Server Metrics";
        SpriteId = "IconDisk";
        RequiredStakeholderId = "cto";
        Direction = MapNodeDirection.Down;
        DependencyIds.Add("LaunchMapLevel");
        VictoryConditions.Add(new InfraActiveVictoryCondition()
        {
            TargetId = "cloud-watch-metrics"
        });
        AddCashReward(100, 250);
        AddPrestigePointsReward(2);
    }

    public override string GetDescription()
    {
        return "Unlock and deploy Server Metrics to give your team better visibility into the status of your infrastructure.";
    }
}