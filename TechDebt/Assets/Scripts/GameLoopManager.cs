using System;
using System.Collections.Generic;
using System.Linq;
using DefaultNamespace.Util.Analytics;
using Tutorial;
using MetaChallenges;
using NPCs;
using Stats;
using UI;
using Unity.Services.Analytics;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class GameLoopManager : MonoBehaviour
{


    public enum GameState { Plan, Play, WaitingForNpcsToExpire, Summary }
    public GameState CurrentState { get; set; }

    protected float DayDurationSeconds = 120f;
    private bool playTimerActive = true;
    protected int currentDay = 0;
    public float dayTimer = 0f;
    public float dailyPacketIncome = 0f;
    
    public int GetCurrentDay()
    {
        return currentDay;
    }

    public void SetPlayTimerActive(bool active)
    {
        Debug.Log("SetPlayTimerActive: " + active);
        playTimerActive = active;
    }
    public int GetDaysLeftInSprint()
    {
        return GameManager.Instance.Map.GetCurrentLevel().SprintDuration - currentDay;
    }
    void FixedUpdate()
    {
        switch (CurrentState)
        {
            case GameState.Play:
                if (playTimerActive)
                {
                    dayTimer += Time.deltaTime;
                    GameManager.Instance.UIManager.UpdateClockDisplay(dayTimer, DayDurationSeconds);
                  
              
                    if (dayTimer >= DayDurationSeconds)
                    {
                        BeginSummaryPhase();
                    }
                }

                break;
            
        }
    }

    public float GetDayDurationSeconds()
    {
        return DayDurationSeconds;
    }


    public void BeginPlanPhase()
    {
        currentDay++;
        dailyPacketIncome = 0;
        GameManager.Instance.UIManager.Resume();
        CurrentState = GameState.Plan;
        
        foreach (NPCBase npc in GameManager.Instance.AllNpcs)
        {
            if (npc.gameObject.activeInHierarchy)
            {
                if (npc.IsDead())
                {
                    continue;
                }
                npc.OnPlanPhaseStart();
            }
        }


        
     

        // Update UI
        GameManager.Instance.UIManager.ShowPlanUI();
        GameManager.Instance.Map.GetCurrentLevel().PlanPhaseCheck();
    }

 



    public void BeginPlayPhase()
    {
        CurrentState = GameState.Play;
        if (GameManager.Instance.TutorialManager != null)
        {
            GameManager.Instance.TutorialManager.Trigger(TutorialStepId.Day_Start);
        }

        GameManager.Instance.InvokeOnPhaseChange(CurrentState);
        dayTimer = 0f;
        GameManager.Instance.SetStat(StatType.PacketsSent, 0);
        GameManager.Instance.SetStat(StatType.PacketsSucceeded, 0);
        GameManager.Instance.SetStat(StatType.PacketsFailed, 0);
        GameManager.Instance.SetStat(StatType.TotalNetworkPacketLatency, 0);

        // Notify NPCs
        foreach (var npc in GameManager.Instance.AllNpcs)
        {
            switch (npc.CurrentState)
            {
                case(NPCBase.State.Dead):
                    break;
                default:
                    npc.OnPlayPhaseStart();
                    break;
            }
           
        }


        // Update UI
        GameManager.Instance.UIManager.HidePlanUI();
        GameManager.Instance.UIManager.moneyPanel.Show();
        

        
        GameManager.Instance.UIManager.Resume();
      

        
    }



    private void BeginSummaryPhase()
    {
     
        
        CurrentState = GameState.WaitingForNpcsToExpire;
        GameManager.Instance.InvokeOnPhaseChange(CurrentState);
        float totalDailyCost = GameManager.Instance.CalculateTotalDailyCost();
        GameManager.Instance.IncrStat(StatType.Money, totalDailyCost * -1);
        // GameManager.Instance.Map.GetCurrentLevel().SummaryPhaseCheck();
        
      
    
        float packetsFailed = GameManager.Instance.GetStatValue(StatType.PacketsFailed);
    
    
        float packetsServiced = GameManager.Instance.GetStatValue(StatType.PacketsSucceeded);
        
        
        float percentageSuccess = packetsServiced / (packetsServiced + packetsFailed);
        if (packetsServiced < 10)
        {
            percentageSuccess = 0;
        }


        float money = GameManager.Instance.GetStatValue(StatType.Money);

        List<KeyValuePair<string, float>> infraCostsList = new List<KeyValuePair<string, float>>();
        foreach (InfrastructureInstance instance in GameManager.Instance.ActiveInfrastructure)
        {
            float cost = instance.GetDailyCost();
            if (instance.IsActive() && cost > 0)
            {
                infraCostsList.Add(new KeyValuePair<string, float>(instance.GetWorldObjectType().DisplayName, cost));
            }
        }

        int sprintNumber = GameManager.Instance.Map.CurrentStageIndex;
        GameManager.Instance. Stats.AddModifier(
            StatType.Traffic,
            new StatModifier($"traffic_sprint_{sprintNumber}_day_{currentDay}", GameManager.Instance.GetStatValue(StatType.Difficulty))
        );
        

   

        float attackPossibility = GameManager.Instance.Stats.GetStatValue(StatType.AttackPossibility);
        
        attackPossibility = GameManager.Instance.IncrStat(
            StatType.AttackPossibility,
            GameManager.Instance.GetStatValue(StatType.AttackPossibilityAccumulationRate)
        );
        
      
        List<string> victoryConditions = new List<string>();
        foreach (MapLevelVictoryConditionBase condition in GameManager.Instance.Map.GetCurrentLevel()
                     .GetCombinedVictoryConditions())
        {
            victoryConditions.Add(condition.GetDescription());
        }

        SummaryData summaryData = new SummaryData
        {
            Day = currentDay,
            PacketsTotal = packetsFailed + packetsServiced,
            PacketsFailed = packetsFailed,
            PacketsSucceeded = packetsServiced,
            PercentageServed = percentageSuccess * 100f,
            TotalCosts = (float)Math.Round(totalDailyCost),
            TotalIncome = (float)Math.Round(dailyPacketIncome),
            NetIncome = dailyPacketIncome - totalDailyCost,
            TotalMoney = money,
            AttackPossibility = attackPossibility,
            VictoryConditions = victoryConditions,
            InfraCosts = infraCostsList
        };

        GameManager.Instance.UIManager.moneyPanel.SpendCoins(totalDailyCost);
        
        GameManager.Instance.UIManager.ShowSummaryUI(summaryData);
        float day = GameManager.Instance.MetaStats.Incr(MetaStat.Day);
        DaySummaryEvent myEvent = new DaySummaryEvent
        {
            SprintLevel = GameManager.Instance.Map.GetCurrentLevel().Name,
            Day = (int) day,
            SprintNumber = sprintNumber,
        };

        GameManager.Instance.RecordEvent(myEvent);

        // Assign "go to door" task to all NPCs
        foreach (NPCBase npc in GameManager.Instance.AllNpcs.ToList())
        {
            if (npc.gameObject.activeInHierarchy)
            {
                npc.EndDay();
            }
        }
        
        foreach (EventBase e in GameManager.Instance.CurrentEvents.ToList())
        {
            if (e.IsOver())
            {
                e.End();
            }
        }

        //GameManager.Instance.UpdateMetaProgress();

    }

    public void PostSummaryCheck()
    {
        GameManager.Instance.Map.GetCurrentLevel().PostSummaryCheck();
    }

    public void Reset()
    {
        currentDay = 0;
        // SetPlayTimerActive(true);
        CurrentState = GameState.Plan;
    }

    public void BeginDemo()
    {
        
        Reset();
        SetPlayTimerActive(false);
        BeginPlayPhase();
    }
}
