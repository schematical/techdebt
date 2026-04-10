
using NPCs;

public class SecurityAuditMapLevel: MapLevel
{
    public SecurityAuditMapLevel() : base()
    {
        Name = "Security Audit Sprint";
        SpriteId = "IconLock";
        Direction = MapNodeDirection.Left;
        
    }
    public override void OnStartDayPlan()
    {
        base.OnStartDayPlan();

        NPCBase npc =
            GameManager.Instance.AllNpcs.Find((npc) => npc.GetComponent<NPCSchematicalBot>() != null);
        npc.ShowDialogBubble().SimpleDisplay(
            "If you are reading this you basically are at the end up what Matt has wired in currently. Good luck!"
        );
   
    }
}
