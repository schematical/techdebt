
public class EmailMapLevel:MapLevel
{
    public EmailMapLevel(MapStage stage) : base(stage)
    {
        Name = "Email Sprint";
        SpriteId = "IconEmail";
        VictoryConditions.Add(new MapLevelVictoryCondition()
        {
            TargetId = "email-service"
        });
    }
    public override void OnStartDayPlan()
    {
        GameManager.Instance.UIManager.ShowNPCDialog(
            GameManager.Instance.SpriteManager.GetSprite("Suit1NPC"),
            "This sprint we want to get a dedicated email sending service. Research it and get it up and running."
        );
   
    }
}
