
public class EmailProductRoadMapLevel:ProductRoadMapLevel
{
    public EmailProductRoadMapLevel()
    {
        Name = "Email Sprint";
        SpriteId = "IconEmail";
        VictoryConditions.Add(new ProductRoadMapLevelVictoryCondition()
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
