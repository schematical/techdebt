
using NPCs;

public class MobileMapLevel: MapLevel
{
    public MobileMapLevel() : base()
    {
        Name = "Mobile Notifications Sprint";
        SpriteId = "IconMobile";
        Direction = MapNodeDirection.Right;
        DependencyIds.Add("UserSignupProductRoadMapLevel");
        VictoryConditions.Add(new InfraActiveVictoryCondition()
        {
            TargetId = "sns"
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
        
        NPCStakeholder npc =
            GameManager.Instance.GetNPCById<NPCStakeholder>("cmo");
        npc.ShowDialogBubble().SimpleDisplay(
            "This sprint we want to get mobile notifications working. Research it and get it up and running."
        );
   
    }
}
