
public class MobileMapLevel: MapLevel
{
    public MobileMapLevel(MapStage stage) : base(stage)
    {
        Name = "Mobile Notifications Sprint";
        SpriteId = "IconMobile";
        VictoryConditions.Add(new MapLevelVictoryCondition()
        {
            TargetId = "sns"
        });
    }
    public override void OnStartDayPlan()
    {
        GameManager.Instance.UIManager.ShowNPCDialog(
            GameManager.Instance.SpriteManager.GetSprite("Suit1NPC"),
            "This sprint we want to get mobile notifications working. Research it and get it up and running."
        );
   
    }
}
