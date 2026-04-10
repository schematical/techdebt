using UnityEngine;

public class SslLevel : MapLevel
{
    public SslLevel() : base()
    {
        Name = "SSL Implementation";
        SpriteId = "IconLock";
        RequiredStakeholderId = "ciso";
    }

    public override string GetDescription()
    {
        return "Implement SSL to protect against Man-in-the-Middle attacks. Requires a Security Officer.";
    }
}