using UnityEngine;

public class DiskSpaceLevel : MapLevel
{
    public DiskSpaceLevel() : base()
    {
        Name = "Disk Space Optimization";
        SpriteId = "IconDisk";
        RequiredStakeholderId = "cto";
        Direction = MapNodeDirection.Down;
        DependencyIds.Add("LaunchMapLevel");
        UnlockConditions.Add(new UnlockCondition()
        {
            Type = UnlockCondition.ConditionType.Stakeholder,
            Level = 1,
            TargetId = "cto"
        });
    }

    public override string GetDescription()
    {
        return "Optimize disk usage across the cluster. Requires a Lead Developer.";
    }
}