using System;
using System.Collections.Generic;
using System.Linq;
using Events;
using MetaChallenges;
using NPCs;
using Stats;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class GameLoopManager : MonoBehaviour
{


    public enum GameState { Plan, Play, WaitingForNpcsToExpire, Summary }
    public GameState CurrentState { get; set; }

    protected float DayDurationSeconds = 120f;
    public bool playTimerActive = true;
    protected int currentDay = 0;
    public float dayTimer = 0f;
    public float dailyPacketIncome = 0f;
    
    public int GetCurrentDay()
    {
        return currentDay;
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
        GameManager.Instance.UIManager.Resume();
        CurrentState = GameState.Plan;
        Vector3 vector3 = GameManager.Instance.GetInfrastructureInstanceByID("door").transform.position;
        foreach (NPCBase npc in GameManager.Instance.AllNpcs)
        {
            if (npc.gameObject.activeInHierarchy)
            {
                if (
                    npc.GetComponent<BossNPC>() != null &&
                    GameManager.Instance.Tutorial != null
                    )
                {
                    continue;
                }

                if (npc.IsDead())
                {
                    continue;
                }
                npc.OnPlanPhaseStart();
                npc.transform.position = vector3;
                npc.gameObject.SetActive(false);
          
            }
        }


        
     

        // Update UI
        GameManager.Instance.UIManager.UpdateGameStateDisplay(CurrentState.ToString());
        GameManager.Instance.UIManager.ShowPlanUI();
        GameManager.Instance.Map.GetCurrentLevel().PlanPhaseCheck();
    }

 



    public void BeginPlayPhase()
    {
        CurrentState = GameState.Play;
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
        GameManager.Instance.UIManager.UpdateGameStateDisplay(CurrentState.ToString());
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
        GameManager.Instance.Map.GetCurrentLevel().SummaryPhaseCheck();
        
      
    
        float packetsFailed = GameManager.Instance.GetStatValue(StatType.PacketsFailed);
    
    
        float packetsServiced = GameManager.Instance.GetStatValue(StatType.PacketsSucceeded);
        
        
        float percentageSuccess = packetsServiced / (packetsServiced + packetsFailed);
        if (packetsServiced < 10)
        {
            percentageSuccess = 0;
        }


        float money = GameManager.Instance.GetStatValue(StatType.Money);

        string infraCosts = "\n\n<b>Infrastructure Costs:</b>\n";
        foreach (InfrastructureInstance instance in GameManager.Instance.ActiveInfrastructure)
        {
            float cost = instance.GetDailyCost();
            if (instance.IsActive() && cost > 0)
            {
                infraCosts += $"{instance.GetWorldObjectType().DisplayName}: ${cost}\n";
            }
        }

        int sprintNumber = GameManager.Instance.Map.CurrentStageIndex;
        GameManager.Instance. Stats.AddModifier(
            StatType.Traffic,
            new StatModifier($"traffic_sprint_{sprintNumber}_day_{currentDay}", GameManager.Instance.GetStatValue(StatType.Difficulty))
        );
        

   

        float attackPossibility = GameManager.Instance.Stats.GetStatValue(StatType.AttackPossibility);
        if (attackPossibility == 0f)
        {
            attackPossibility = GameManager.Instance.IncrStat(StatType.AttackPossibility, 0.05f);
        }
        else
        {
            attackPossibility = GameManager.Instance.Stats.AddModifier(
                StatType.AttackPossibility,
                new StatModifier(
                    $"attackPossibility_sprint_{sprintNumber}_day_{currentDay}",
                    1.1f  
                )
            );
        }
      
        string summaryText = $"End of Day {currentDay - 1}\n" +
                             $"Total Packets: {packetsFailed + packetsServiced} \n" +
                             $"Packets Failed: {packetsFailed} \n" +
                             $"Packets Succeeded: {packetsServiced} \n" +
                             $"Percentage Served: %{percentageSuccess:F2} \n" +
                             $"Total Costs: ${Math.Round(totalDailyCost)} \n" +
                             $"Total Income: ${Math.Round(dailyPacketIncome)}\n" +
                             $"Net Income: ${dailyPacketIncome - totalDailyCost}\n" +
                             $"Total: {money} \n" +
                             //$"Tomorrow's Expected Income: ${updatedDailyIncome} - ({(1 - percentageSuccess):F2}% Failed Penalty)\n" +
                             $"Tomorrow's Attack Possibility: {attackPossibility:F2}%\n\n" +
                             $"Victory Conditions: \n";
        foreach (MapLevelVictoryConditionBase condition in GameManager.Instance.Map.GetCurrentLevel()
                     .GetCombinedVictoryConditions())
        {
            summaryText += $"  - {condition.GetDescription()} \n";
        }
                             

        summaryText += infraCosts;

        GameManager.Instance.UIManager.moneyPanel.SpendCoins(totalDailyCost);
        
        
        GameManager.Instance.UIManager.ShowSummaryUI(summaryText);
        GameManager.Instance.MetaStats.Incr(MetaStat.Day);

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

        GameManager.Instance.UpdateMetaProgress();

    }

    public void Reset()
    {
        currentDay = 0;
        CurrentState = GameState.Plan;
    }
    
}
