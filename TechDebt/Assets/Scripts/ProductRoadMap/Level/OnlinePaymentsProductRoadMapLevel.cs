
using System.Collections.Generic;
using DefaultNamespace.Rewards;
using NPCs;
using Stats;
using Tutorial;
using UI;
using UnityEngine;

public class OnlinePaymentsProductRoadMapLevel: MapLevel
{
    protected bool hasReleaseBeedDeployed = false;
    
    public OnlinePaymentsProductRoadMapLevel() : base()
    {
        Name = "Online Payments";
        SpriteId = "IconFlag";
        SprintDuration = 5;
        DependencyIds.Add("UserSignupProductRoadMapLevel");
        
        SpecialReleaseVictoryCondition condition = new SpecialReleaseVictoryCondition(
            "Online Payments",
            "Allows User To Pay Online",
            "IconMoney",
            OnSpecialReleaseComplete
        );
        
        VictoryConditions.Add(condition);
        
        
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
    }


    private void OnSpecialReleaseComplete()
    {
        NetworkPacketData networkPacketData =
            GameManager.Instance.GetNetworkPacketDataByType(NetworkPacketData.PType.PII);
        networkPacketData.Stats.Stats[StatType.NetworkPacket_Probibility].SetBaseValue(5);
        NPCBase npc =
            GameManager.Instance.AllNpcs.Find((npc) => npc.GetComponent<NPCSchematicalBot>() != null);
        npc.ShowDialogBubble().SimpleDisplay(
            "Great work. " + (!IsLaunchDay() ? " Some payments will trickle in for now but on Launch Day you will see a lot more." : " Just in the nick of time.")
        );
    }

    public override void OnLaunchDayPlan()
    {
        NetworkPacketData networkPacketData =
            GameManager.Instance.GetNetworkPacketDataByType(NetworkPacketData.PType.Purchase);
        networkPacketData.Stats.Stats[StatType.NetworkPacket_Probibility].SetBaseValue(5);
        GameManager.Instance.TutorialManager.Trigger(TutorialStepId.NetworkPacket_Purchase);

    }
}
