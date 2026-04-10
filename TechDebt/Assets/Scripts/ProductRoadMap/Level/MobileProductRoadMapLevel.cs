
using NPCs;

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
        
        NPCBase npc =
            GameManager.Instance.AllNpcs.Find((npc) => npc.GetComponent<NPCSchematicalBot>() != null);
        npc.ShowDialogBubble().SimpleDisplay(
            "This sprint we want to get mobile notifications working. Research it and get it up and running."
        );
   
    }
}
