
using System.Collections.Generic;
using Stats;
using UI;
using UnityEngine;

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
            "Today is launch day! Now we will receive sales packets. \n Expect extra traffic."
        );

        StatModifier launchDayTrafficModifier = new StatModifier("launch_day_traffic", 2f);
        GameManager.Instance.Stats.AddModifier(StatType.Traffic, launchDayTrafficModifier);
        StatModifiers[ModifierType.LaunchDay].Add(launchDayTrafficModifier);
        NetworkPacketData networkPacketData =
            GameManager.Instance.GetNetworkPacketDataByType(NetworkPacketData.PType.Purchase);
        StatModifier launchDayPurchaseModifier = new StatModifier("launch_day_purchase", 2f);
        networkPacketData.Stats.Stats[StatType.NetworkPacket_Probibility].SetBaseValue(10);
        networkPacketData.Stats.AddModifier(StatType.NetworkPacket_Probibility, launchDayPurchaseModifier);
        StatModifiers[ModifierType.LaunchDay].Add(launchDayPurchaseModifier);
        GameManager.Instance.InfrastructureUpdateNetworkTargets();
    }

    public override void OnLaunchDaySummary()
    {
        GameManager.Instance.UIManager.ShowNPCDialog(
            GameManager.Instance.SpriteManager.GetSprite("Suit1NPC"),
            "We survived! Great work. Lets get working on our next sprint.",
            new List<DialogButtonOption>()
            {
                new DialogButtonOption() { Text = "Plan Next Sprint", OnClick = () =>
                    {
                        CleanUpModifiers(ModifierType.LaunchDay);
                        NetworkPacketData networkPacketData =
                            GameManager.Instance.GetNetworkPacketDataByType(NetworkPacketData.PType.Purchase);
                        GameManager.Instance.InfrastructureUpdateNetworkTargets();
                        GameManager.Instance.ProductRoadMap.IncrStage();
                        GameManager.Instance.UIManager.productRoadMap.Show(UIProductRoadMap.State.Select);
                    }
                }/*,
                new DialogButtonOption() { Text = "Main Menu", OnClick = () =>
                    {

                    }
                },*/
            }
        );
        
    }
}
