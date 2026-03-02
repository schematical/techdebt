using Infrastructure;
using UnityEngine;

public class MiscWOType : WorldObjectType
{
    public MiscWOType()
    {
        type = WorldObjectType.Type.Misc;
        DisplayName = "Misc";
        BuildTime = 30;
    }
}
