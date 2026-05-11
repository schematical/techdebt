
public class GeoLocationMapLevel:MapLevel
{
    public GeoLocationMapLevel() : base()
    {
        Name = "Geo Location Sprint";
        SpriteId = "IconGeo";
        Direction = MapNodeDirection.Right;
        UnlockConditions.Add(new UnlockCondition()
        {
            Type = UnlockCondition.ConditionType.Stakeholder,
            Level = 3,
            TargetId = "cmo"
        });
    }
  
}
