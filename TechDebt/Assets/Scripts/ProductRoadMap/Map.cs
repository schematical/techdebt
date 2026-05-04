#if !(UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX || UNITY_STANDALONE_OSX || STEAMWORKS_WIN || STEAMWORKS_LIN_OSX)
#define DISABLESTEAMWORKS
#endif

using System;
using System.Collections.Generic;
using System.Linq;
using DefaultNamespace.Rewards;
using MetaChallenges;
using NPCs;
using Stats;
#if !DISABLESTEAMWORKS
using Steamworks;
#endif
using UI;
using UnityEngine;
using Random = UnityEngine.Random;


public class Map
{

 
    public List<MapLevel> LevelPool { get; set; } = new List<MapLevel>()
    {
        new LaunchMapLevel(),
        new UserSignupProductRoadMapLevel(),
        new MobileMapLevel(),
        new EmailMapLevel(),
        new SocketChatMapLevel(),
        new GeoLocationMapLevel(),
        new CodePipelineLevel(),
        new Metrics1Level(),
        new SslLevel(),
        new SaasLevel(),
        // new DiskSpaceLevel(),
        new CheckoutCartLevel(),
        new OnlinePaymentsProductRoadMapLevel()
    };
    protected MapLevel CurrentLevel { get; set; }
    public int CurrentSprintNumber { get; protected set; } = -1;

    public List<MapLevelVictoryConditionBase> GlobalVictoryConditions { get; set; } =
        new List<MapLevelVictoryConditionBase>();

    public List<string> BanishedRewardIds { get; set; } = new();
    protected List<MapLevelReward> MetaLevelRewards = new();

    public void Randomize()
    {
        
    }

    public void MarkMetaRewardRedeemed(MapLevelReward reward)
    {
        if (reward.Type != MapLevelReward.MapLevelRewardType.Meta)
        {
            Debug.LogError(reward.Type + " is not a meta reward");
            return;
        }
        MetaLevelRewards.Add(reward);
        /*if (SteamManager.Initialized)
        {
            Steamworks.SteamUserStats.GetAchievement(reward.Id, out bool achieved);
            if (achieved)
            {
                SteamUserStats.SetAchievement(reward.Id);
                SteamUserStats.StoreStats();
            }
        }*/
    }
    public MapLevel GetCurrentLevel()
    {
        return CurrentLevel;
    }
    public void SetCurrentLevel(MapLevel level)
    {
        CurrentSprintNumber += 1;
        CurrentLevel = level;
        level.OnSprintStart();
        GameManager.Instance.UIManager.victoryConditionListPanel.Refresh();
    }


    /*public void IncrStage()
    {
        CurrentLevel.MarkCompleted();
        List<MapLevel> levels = LevelPool.FindAll((level => level.State == MapLevel.MapLevelState.Incomplete));

        if (levels.Count == 0)
        {
            CurrentLevel.EndGame(
                "Well done! You have made it to the end of the run. Try again with new infrastructure and abilities that you unlocked during the run. Be sure tho check the Meta Challenges menu."
            );
            return;
        }

        CurrentSprintNumber += 1;
      

        GameManager.Instance.MetaStats.Incr(MetaStat.Sprint);
        if (!GameManager.Instance.GetInfrastructureInstanceByID("product-road-map").IsActive())
        {
            
        }
        else
        {
            GameManager.Instance.UIManager.productRoadMap.Show(UIProductRoadMap.State.Select);
        }

    }*/

 

    public void AddGlobalVictoryCondition(MapLevelVictoryConditionBase condition)
    {
        condition.SetGlobal();
        GlobalVictoryConditions.Add(condition);
        GameManager.Instance.UIManager.victoryConditionListPanel.Refresh();
    }


    public List<MapLevelReward> GetMetaRewards()
    {
        return MetaLevelRewards;
    }
}




