
public class SocketChatMapLevel:MapLevel
{
    public SocketChatMapLevel() : base()
    {
        Name = "Socket Chat Sprint";
        SpriteId = "IconChat";
        Direction = MapNodeDirection.Right;
        AddCashReward(100, 250);
        AddPrestigePointsReward(2);
        UnlockConditions.Add(new UnlockCondition()
        {
            Type = UnlockCondition.ConditionType.Stakeholder,
            Level = 2,
            TargetId = "cto"
        });
    }
   
}
