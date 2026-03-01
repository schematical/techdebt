
public class SecurityAuditMapLevel: MapLevel
{
    public SecurityAuditMapLevel(MapStage stage) : base(stage)
    {
        Name = "Security Audit Sprint";
        SpriteId = "IconLock";
        
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
