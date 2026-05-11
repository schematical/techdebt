
public class SocketChatMapLevel:MapLevel
{
    public SocketChatMapLevel() : base()
    {
        Name = "Socket Chat Sprint";
        SpriteId = "IconChat";
        Direction = MapNodeDirection.Right;
        UnlockConditions.Add(new UnlockCondition()
        {
            Type = UnlockCondition.ConditionType.Stakeholder,
            Level = 2,
            TargetId = "cto"
        });
    }
   
}
