
using System.Collections.Generic;
using DefaultNamespace.Rewards;
using NPCs;
using Stats;
using UI;
using UnityEngine;

public class UserSignupProductRoadMapLevel: MapLevel
{
    public UserSignupProductRoadMapLevel() : base()
    {
        Name = "User Signup Sprint";
        SpriteId = "IconFlag";// TODO: Drivers licence icon?
        SprintDuration = 5;
        DependencyIds.Add("LaunchMapLevel");
        Direction = MapNodeDirection.Right;

        SpecialReleaseVictoryCondition condition = new SpecialReleaseVictoryCondition(
            "User Signup/Login",
            "Allows User To Signup",
            "IconPII",
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
        modifier.networkPacketType = NetworkPacketData.PType.PII;
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
            "Great work. " + (!IsLaunchDay() ? " Some signups will trickle in for now but on Launch Day you will see a lot more." : " Just in the nick of time.")
        );
    }

    

    public override void OnStartDayPlan()
    {
        NPCBase npc =
            GameManager.Instance.AllNpcs.Find((npc) => npc.GetComponent<NPCSchematicalBot>() != null);
        npc.ShowDialogBubble().SimpleDisplay(
            "Hey! This sprint we to allow users to Sign Up. Be careful handling Personally Identifiable Information(PII). If that gets leaked were in big trouble."
        );
        base.OnStartDayPlan();
   
    }
    public override void OnLaunchDayPlan()
    {
        NPCBase npc =
            GameManager.Instance.AllNpcs.Find((npc) => npc.GetComponent<NPCSchematicalBot>() != null);
        npc.ShowDialogBubble().SimpleDisplay(
            "Today is launch day!  \n Expect extra traffic."
        );

 
        base.OnLaunchDayPlan();
    }

 
}
