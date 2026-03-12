
public class EmailMapLevel:MapLevel
{
    public EmailMapLevel() : base()
    {
        Name = "Email Sprint";
        SpriteId = "IconEmail";
        VictoryConditions.Add(new InfraActiveVictoryCondition()
        {
            TargetId = "email-service"
        });
    }
    public override void OnStartDayPlan()
    {
        base.OnStartDayPlan();
        GameManager.Instance.UIManager.ShowNPCDialog(
            GameManager.Instance.SpriteManager.GetSprite("Suit1NPC"),
            "This sprint we want to get a dedicated email sending service. Research it and get it up and running."
        );
   
    }
}
