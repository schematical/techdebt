
using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;


public abstract class MapLevelVictoryConditionBase
{
    public abstract VictoryConditionState GetState();

    public abstract string GetDescription();
}