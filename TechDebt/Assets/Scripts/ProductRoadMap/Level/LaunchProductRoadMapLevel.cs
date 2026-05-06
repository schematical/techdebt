
using System.Collections.Generic;
using DefaultNamespace;
using NPCs;
using NUnit.Framework;
using Stats;
using Tutorial;
using UI;
using UnityEngine;

public class LaunchMapLevel: MapLevel
{

    public LaunchMapLevel() : base()
    {
        Name = "Launch Sprint";
        SpriteId = "IconFlag";
        SprintDuration = 2;
        
        AddCashReward(-1, 150);
        AddPrestigePointsReward();
        
        
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
        modifier.networkPacketType = NetworkPacketData.PType.Purchase;
        modifier.Direction = MapLevelModifier.ModifierDirection.Positive;
        modifier.Duration = MapLevelModifier.ModifierDuration.LaunchDay;
        modifier.SetOverrideValue(2f);
        LevelModifiers.Add(modifier);
        MetaProgressData metaProgressData = MetaGameManager.GetProgress();
        if (metaProgressData.gameStage != GameStage.Tutorial)
        {
            VictoryConditions.Add(new InfraActiveVictoryCondition()
            {
                TargetId = "product-road-map"
            });
        }
    }

    public override void Randomize(int modifierCount)
    {
        // base.Randomize(); // Dont randomize as it will remove modifiers.
       
    }

    public override void OnStartDayPlan()
    {
        GameManager.Instance.Map.AddGlobalVictoryCondition(new HasMoneyVictoryCondition());
       
        NPCStakeholder npc =
            GameManager.Instance.GetNPCById<NPCStakeholder>("ceo");
        npc.ShowDialogBubble().SimpleDisplay(
            $"Welcome to the team! This sprint we need to get up and running. At the end of the sprint we will run a bit launch campaign that will drive a lot more traffic. Make sure our infrastructure can handle it."
        );
        npc.ZoomToAndFollow();
     

        base.OnStartDayPlan();
   
    }
    public override void OnLaunchDayPlan()
    {
        GameManager.Instance.Map.AddGlobalVictoryCondition(new UpTimeVictoryCondition());
        GameManager.Instance.UIManager.toastHolderPanel.Add("Launch Day Start!");
      
        NPCStakeholder npc =
            GameManager.Instance.GetNPCById<NPCStakeholder>("ceo");
        npc.ShowDialogBubble().SimpleDisplay(
            "Today is launch day! \n Expect extra traffic."
        );
        npc.ZoomToAndFollow();
        base.OnLaunchDayPlan();
    }

}
