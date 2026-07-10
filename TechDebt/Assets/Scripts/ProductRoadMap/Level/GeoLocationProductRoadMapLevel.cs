
public class GeoLocationMapLevel:MapLevel
{
    public GeoLocationMapLevel() : base()
    {
        Name = "Geo Location Sprint";
        SpriteId = "IconGeo";
        Direction = MapNodeDirection.Right;
        AddCashReward(100, 250);
        AddPrestigePointsReward(2);
        UnlockConditions.Add(new UnlockCondition()
        {
            Type = UnlockCondition.ConditionType.Stakeholder,
            Level = 3,
            TargetId = "cmo"
        });
    }
  
}
