
using System.Collections.Generic;
using Stats;
using UI;
using UnityEngine;

public class LaunchProductRoadMapLevel: ProductRoadMapLevel
{
    StatModifier LaunchDayStatModifier;
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
        LaunchDayStatModifier = new StatModifier("launch_day_traffic", 2f);
        GameManager.Instance.Stats.AddModifier(StatType.Traffic, LaunchDayStatModifier);
        NetworkPacketData networkPacketData = GameManager.Instance.GetNetworkPacketDatas().Find((data => data.Type == NetworkPacketData.PType.Purchase));
        if (networkPacketData == null)
        {
            foreach (NetworkPacketData networkPacketData2 in GameManager.Instance.GetNetworkPacketDatas())
            {
                Debug.LogError($"NetworkPacketData {networkPacketData2.Type}");
            }
            throw new System.Exception("No network packet found `NetworkPacketData.PType.Purchase`");
        }
        networkPacketData.probilitly = 20;
        GameManager.Instance.InfrastructureUpdateNetworkTargets();
    }

    public override void OnLaunchDaySummary()
    {
        Debug.Log($"OnLaunchDaySummary");
        GameManager.Instance.UIManager.ShowNPCDialog(
            GameManager.Instance.SpriteManager.GetSprite("Suit1NPC"),
            "We survived! Great work. Lets get working on our next sprint.",
            new List<DialogButtonOption>()
            {
                new DialogButtonOption() { Text = "Plan Next Sprint", OnClick = () =>
                    {
                        LaunchDayStatModifier.Remove();
                        NetworkPacketData networkPacketData = GameManager.Instance.GetNetworkPacketDatas().Find((data => data.Type == NetworkPacketData.PType.Purchase));
                        if (networkPacketData == null)
                        {
                            foreach (NetworkPacketData networkPacketData2 in GameManager.Instance.GetNetworkPacketDatas())
                            {
                                Debug.LogError($"NetworkPacketData {networkPacketData2.Type}");
                            }
                            throw new System.Exception("No network packet found `NetworkPacketData.PType.Purchase`");
                        }
                        networkPacketData.probilitly = 10;
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
