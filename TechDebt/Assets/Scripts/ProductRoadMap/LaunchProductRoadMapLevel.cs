
using System.Collections.Generic;
using Stats;
using UI;
using UnityEngine;

public class LaunchMapLevel: MapLevel
{

    public LaunchMapLevel() : base()
    {
        Name = "Launch Sprint";
        SpriteId = "IconFlag";
        SprintDuration = 5;

    }

    public override void Randomize()
    {
        // base.Randomize();
        MapLevelModifier modifier = new MapLevelModifier();
        modifier.Type = MapLevelModifier.ModifierType.Stat;
        modifier.statType = StatType.Traffic;
        modifier.Direction = MapLevelModifier.ModifierDirection.Negative;
        modifier.Duration = MapLevelModifier.ModifierDuration.LaunchDay;
        modifier.SetOverrideValue(2);
        LevelModifiers.Add(modifier);
        
        modifier = new MapLevelModifier();
        modifier.Type = MapLevelModifier.ModifierType.Stat;
        modifier.statType = StatType.NetworkPacket_Probibility;
        modifier.Direction = MapLevelModifier.ModifierDirection.Positive;
        modifier.Duration = MapLevelModifier.ModifierDuration.LaunchDay;
        modifier.SetOverrideValue(2);
        LevelModifiers.Add(modifier);
    }

    public override void OnStartDayPlan()
    {
        GameManager.Instance.UIManager.ShowNPCDialog(
            GameManager.Instance.SpriteManager.GetSprite("Suit1NPC"),
            "Hey! This sprint we need to get up and running. At the end of the sprint we will run a bit launch campaign that will drive a lot more traffic. Make sure our infrastructure can handle it."
        );
        base.OnStartDayPlan();
   
    }
    public override void OnLaunchDayPlan()
    {
        GameManager.Instance.UIManager.ShowNPCDialog(
            GameManager.Instance.SpriteManager.GetSprite("Suit1NPC"),
            "Today is launch day! Now we will receive sales packets. \n Expect extra traffic."
        );

        NetworkPacketData networkPacketData =
            GameManager.Instance.GetNetworkPacketDataByType(NetworkPacketData.PType.Purchase);
        networkPacketData.Stats.Stats[StatType.NetworkPacket_Probibility].SetBaseValue(5);
        /*StatModifier launchDayTrafficModifier = new StatModifier("launch_day_traffic", 2f);
        GameManager.Instance.Stats.AddModifier(StatType.Traffic, launchDayTrafficModifier);
        StatModifiers[ModifierType.LaunchDay].Add(launchDayTrafficModifier);

        networkPacketData.Stats.AddModifier(StatType.NetworkPacket_Probibility, launchDayPurchaseModifier);
        StatModifiers[ModifierType.LaunchDay].Add(launchDayPurchaseModifier);
        GameManager.Instance.InfrastructureUpdateNetworkTargets();*/
        base.OnLaunchDayPlan();
    }

    public override void OnLaunchDaySummary()
    {
        base.OnLaunchDaySummary();
        
        
    }
}
