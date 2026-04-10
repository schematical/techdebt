using UnityEngine;

public class DiskSpaceLevel : MapLevel
{
    public DiskSpaceLevel() : base()
    {
        Name = "Disk Space Optimization";
        SpriteId = "IconDisk";
        RequiredStakeholderId = "cto";
    }

    public override string GetDescription()
    {
        return "Optimize disk usage across the cluster. Requires a Lead Developer.";
    }
}