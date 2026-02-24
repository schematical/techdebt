
using Stats;

public class LaunchProductRoadMapLevel:ProductRoadMapLevel
{
    public LaunchProductRoadMapLevel()
    {
        Name = "Launch Sprint";
        SpriteId = "IconFlag";
        
    }
    public void OnStart()
    {
      //TODO: Have the NPCs describe the sprint   
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
        // GameManager.Instance.Stats.AddModifier(StatType.Traffic, new StatModifier(StatModifier.ModifierType.Multiply, 4));
    }
}
