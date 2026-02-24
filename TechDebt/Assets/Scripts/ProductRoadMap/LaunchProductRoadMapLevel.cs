
using Stats;

public class LaunchProductRoadMapLevel: ProductRoadMapLevel
{
    public LaunchProductRoadMapLevel()
    {
        Name = "Launch Sprint";
        SpriteId = "IconFlag";
        
    }
    public override void OnStartDayPlan()
    {
        GameManager.Instance.UIManager.ShowNPCDialog(
            GameManager.Instance.SpriteManager.GetSprite("Suit1NPC"),
            "Hey! This sprint we need to get up and running. At the end of the sprint we will run a bit launch campaign that will drive a lot more traffic. Make sure our infrastructure can handle it."
        );
   
    }
    public override void OnLaunchDayPlan()
    {
        GameManager.Instance.UIManager.ShowNPCDialog(
            GameManager.Instance.SpriteManager.GetSprite("Suit1NPC"),
            "Today is launch day! \n Expect extra traffic."
        );
        GameManager.Instance.Stats.AddModifier(StatType.Traffic, new StatModifier("launch_day_traffic", 2));
    }

    public override void OnLaunchDaySummary()
    {
        GameManager.Instance.UIManager.ShowNPCDialog(
            GameManager.Instance.SpriteManager.GetSprite("Suit1NPC"),
            "TODO: Check if we hit our goals. TODO: Remove stat modifier"
        );
    }
}
