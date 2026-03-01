
public class MobileMapLevel: MapLevel
{
    public MobileMapLevel(MapStage stage) : base(stage)
    {
        Name = "Mobile Notifications Sprint";
        SpriteId = "IconMobile";
        VictoryConditions.Add(new InfraActiveVictoryCondition()
        {
            TargetId = "sns"
        });
    }
    public override void OnStartDayPlan()
    {
        base.OnStartDayPlan();
        GameManager.Instance.UIManager.ShowNPCDialog(
            GameManager.Instance.SpriteManager.GetSprite("Suit1NPC"),
            "If you are reading this you basically are at the end up what Matt has wired in currently. Good luck!"
        );
   
    }
}
