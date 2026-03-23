
using System.Collections.Generic;
using DefaultNamespace.Rewards;
using NPCs;
using Stats;
using UI;
using UnityEngine;

public class UserSignupProductRoadMapLevel: MapLevel
{
    protected bool hasReleaseBeedDeployed = false;
    public UserSignupProductRoadMapLevel() : base()
    {
        Name = "User Signup Sprint";
        SpriteId = "IconFlag";// TODO: Drivers licence icon?
        SprintDuration = 5;
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
        hasReleaseBeedDeployed = true;
        NetworkPacketData networkPacketData =
            GameManager.Instance.GetNetworkPacketDataByType(NetworkPacketData.PType.PII);
        networkPacketData.Stats.Stats[StatType.NetworkPacket_Probibility].SetBaseValue(5);
        NPCBase npc =
            GameManager.Instance.AllNpcs.Find((npc) => npc.GetComponent<NPCSchematicalBot>() != null);
        npc.ShowDialogBubble().SimpleDisplay(
            "Great work. " + (!IsLaunchDay() ? " Some signups will trickle in for now but on Launch Day you will see a lot more." : " Just in the nick of time.")
        );
    }

    public override List<RewardBase> GetSpecialReleaseRewards()
    {
        if (hasReleaseBeedDeployed)
        {
            return new List<RewardBase>();
        }

        return new List<RewardBase>()
        {
            new SpecialCallbackReward(OnSpecialReleaseComplete)
            {
                Name = "User Signup/Login",
                Description = "Allows User To Signup",
                IconSpriteId = "IconPII"
            }
        };
    }

    public override VictoryConditionState GetVictoryConditionState()
    {
        VictoryConditionState state = base.GetVictoryConditionState();
        if (state == VictoryConditionState.Failed)
        {
            return state;
        }

        if (!hasReleaseBeedDeployed)
        {
            return VictoryConditionState.NotMet;
        }

        return state;
    }

    public override void Randomize(int modifierCount)
    {
        // base.Randomize(); // Dont randomize as it will remove modifiers.
       
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

    public override void OnLaunchDaySummary()
    {
        base.OnLaunchDaySummary();
        
        
    }
}
