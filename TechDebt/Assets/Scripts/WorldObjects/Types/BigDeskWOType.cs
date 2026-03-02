

using Infrastructure;
using UnityEngine;


public class BigDeskWOType: WorldObjectType
{
    public BigDeskWOType()
    {
        type = WorldObjectType.Type.BigDesk;
        DisplayName = "Big Desk";
        interactionPositionOffset = new Vector3(0.0f, 1.0f, 0.0f);

    }
}
