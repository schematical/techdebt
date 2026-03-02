using Infrastructure;
using UnityEngine;

public class WhiteBoardWOType : WorldObjectType
{
    public WhiteBoardWOType()
    {
        type = WorldObjectType.Type.WhiteBoard;
        DisplayName = "White Board";
        BuildTime = 30;
    }
}
