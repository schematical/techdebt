
using NPCs;

public class EmailMapLevel:MapLevel
{
    public EmailMapLevel() : base()
    {
        Name = "Email Sprint";
        SpriteId = "IconEmail";
        Direction = MapNodeDirection.Right;
        DependencyIds.Add("UserSignupProductRoadMapLevel");
        VictoryConditions.Add(new InfraActiveVictoryCondition()
        {
            TargetId = "email-service"
        });
        UnlockConditions.Add(new UnlockCondition()
        {
            Type = UnlockCondition.ConditionType.Stakeholder,
            Level = 0,
            TargetId = "cmo"
        });
    }
    public override void OnStartDayPlan()
    {
        base.OnStartDayPlan();
        NPCBase npc =
            GameManager.Instance.AllNpcs.Find((npc) => npc.GetComponent<NPCSchematicalBot>() != null);
        npc.ShowDialogBubble().SimpleDisplay(
            "This sprint we want to get a dedicated email sending service. Research it and get it up and running."
        );
   
    }
}
