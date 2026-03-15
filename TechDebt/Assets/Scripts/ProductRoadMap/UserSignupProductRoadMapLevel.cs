
using System.Collections.Generic;
using DefaultNamespace.Rewards;
using Stats;
using UI;
using UnityEngine;

public class UserSignupProductRoadMapLevel: MapLevel
{
    protected bool hasReleaseBeedDeployed = false;
    protected List<RewardBase> ReleaseRewards = new List<RewardBase>();
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
        ReleaseRewards.Add(new SpecialCallbackReward(OnSpecialReleaseComplete)
        {
            Name = "User Signup/Login",
            Description = "Allows User To Signup"
        });
        
    }

    private void OnSpecialReleaseComplete()
    {
        hasReleaseBeedDeployed = true;
    }

    public override List<RewardBase> GetSpecialReleaseRewards()
    {
        return ReleaseRewards;
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
        GameManager.Instance.UIManager.ShowNPCDialog(
            GameManager.Instance.SpriteManager.GetSprite("Suit1NPC"),
            "Hey! This sprint we to allow users to Sign Up. Be careful handling Personally Identifiable Information(PII). If that gets leaked were in big trouble."
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
            GameManager.Instance.GetNetworkPacketDataByType(NetworkPacketData.PType.PII);
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
