using System;
using System.Collections.Generic;
using System.Linq;
using DefaultNamespace.Rewards;
using MetaChallenges;
using NPCs;
using Stats;
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
        new SslLevel(),
        new SaasLevel(),
        new DiskSpaceLevel(),
        new CheckoutCartLevel(),
        new OnlinePaymentsProductRoadMapLevel()
    };
    protected MapLevel CurrentLevel { get; set; }
    public int CurrentSprintNumber { get; protected set; } = -1;

    public List<MapLevelVictoryConditionBase> GlobalVictoryConditions { get; set; } =
        new List<MapLevelVictoryConditionBase>();

    public void Randomize()
    {
        
    }

    public MapLevel GetCurrentLevel()
    {
        return CurrentLevel;
    }
    public void SetCurrentLevel(MapLevel level)
    {
        CurrentSprintNumber += 1;
        CurrentLevel = level;
        GameManager.Instance.UIManager.victoryConditionListPanel.Refresh();
        level.OnSprintStart();
    }


    public void IncrStage()
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

    }

 

    public void AddGlobalVictoryCondition(MapLevelVictoryConditionBase condition)
    {
        condition.SetGlobal();
        GlobalVictoryConditions.Add(condition);
        GameManager.Instance.UIManager.victoryConditionListPanel.Refresh();
    }



    
}




