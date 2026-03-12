
public class MobileMapLevel: MapLevel
{
    public MobileMapLevel() : base()
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
            "This sprint we want to get mobile notifications working. Research it and get it up and running."
        );
   
    }
}
