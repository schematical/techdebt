
using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;


public abstract class MapLevelVictoryConditionBase
{
    public abstract VictoryConditionState GetState();

    public abstract string GetDescription();
    public bool isGlobal = false;

    public void SetGlobal()
    {
        isGlobal = true;
    }

    public bool IsGlobal()
    {
        return isGlobal;
    }
}