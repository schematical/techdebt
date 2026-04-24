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
                if (
                    playTimerActive && 
                    !GameManager.Instance.UIManager.IsPausedState()
                )
                {
                    dayTimer += Time.fixedDeltaTime;
                    GameManager.Instance.UIManager.UpdateClockDisplay(dayTimer, DayDurationSeconds);
                  
              
                    if (dayTimer >= DayDurationSeconds)
                    {
                        TriggerNextDay();
                    }
                }

                break;
            
        }
    }

    private void TriggerNextDay()
    {
        GameManager.Instance.Map.GetCurrentLevel().PostSummaryCheck();
        currentDay++;
      
        GameManager.Instance.Map.GetCurrentLevel().PlanPhaseCheck();
        
        dayTimer = 0f;
        GameManager.Instance.SetStat(StatType.PacketsSent, 0);
        GameManager.Instance.SetStat(StatType.PacketsSucceeded, 0);
        GameManager.Instance.SetStat(StatType.PacketsFailed, 0);
        GameManager.Instance.SetStat(StatType.TotalNetworkPacketLatency, 0);

        int sprintNumber = GameManager.Instance.Map.CurrentSprintNumber;
        GameManager.Instance.Stats.AddModifier(
            StatType.Traffic,
            new StatModifier($"traffic_sprint_{sprintNumber}_day_{currentDay}", GameManager.Instance.GetStatValue(StatType.Difficulty))
        );
        
        GameManager.Instance.UIManager.moneyPanel.Show();
        GameManager.Instance.UIManager.Resume();
        GameManager.Instance.UIManager.toastHolderPanel.Add($"Day {currentDay} Starting");
        GameManager.Instance.UIManager.toastHolderPanel.Add($"Expected Traffic: { Math.Round(GameManager.Instance.GetStatValue(StatType.Traffic))} Packets/Day");
        
        GameManager.Instance.IncrStat(
            StatType.AttackPossibility,
            GameManager.Instance.GetStatValue(StatType.AttackPossibilityAccumulationRate)
        );
        float day = GameManager.Instance.MetaStats.Incr(MetaStat.Day);
        DaySummaryEvent myEvent = new DaySummaryEvent
        {
            SprintLevel = GameManager.Instance.Map.GetCurrentLevel().Name,
            Day = (int) day,
            SprintNumber = sprintNumber,
            LevelName = GameManager.Instance.Map.GetCurrentLevel().Name,
        };

        GameManager.Instance.RecordEvent(myEvent);
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
        GameManager.Instance.UIManager.SetTimeScalePlay(true);

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
