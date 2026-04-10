using UnityEngine;

public class SslLevel : MapLevel
{
    public SslLevel() : base()
    {
        Name = "SSL Implementation";
        SpriteId = "IconLock";
        RequiredStakeholderId = "ciso";
        Direction = MapNodeDirection.Left;
        DependencyIds.Add("LaunchMapLevel");
    }

    public override string GetDescription()
    {
        return "Implement SSL to protect against Man-in-the-Middle attacks. Requires a Security Officer.";
    }
}