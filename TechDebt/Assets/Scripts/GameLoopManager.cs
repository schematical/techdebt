using System;
using System.Collections.Generic;
using System.Linq;
using MetaChallenges;
using Stats;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameLoopManager : MonoBehaviour
{


    public enum GameState { Plan, Play, WaitingForNpcsToExpire, Summary }
    public GameState CurrentState { get; private set; }

    public float dayDurationSeconds = 120f;
    public bool playTimerActive = true;
    public int currentDay = 0;
    public float dayTimer = 0f;
    
    void FixedUpdate()
    {
        switch (CurrentState)
        {
            case GameState.Play:
                if (playTimerActive)
                {
                    dayTimer += Time.deltaTime;
                    GameManager.Instance.UIManager.UpdateClockDisplay(dayTimer, dayDurationSeconds);
                  
              
                    if (dayTimer >= dayDurationSeconds)
                    {
                        BeginSummaryPhase();
                    }
                }

                break;
            
        }
    }

    public float GetDayDurationSeconds()
    {
        return dayDurationSeconds;
    }

    public void EndGame()
    {
        GameManager.Instance.UIManager.SetTimeScalePause();
        NPCBase bossNPC = GameManager.Instance.AllNpcs.Find((npc) => npc.GetComponent<BossNPC>() != null);
        GameManager.Instance.cameraController.ZoomToAndFollow(bossNPC.transform);
        GameManager.Instance.UpdateMetaProgress();
        GameManager.Instance.UIManager.ShowNPCDialog(
            bossNPC.GetComponent<SpriteRenderer>().sprite,
            "You have failed to keep our infrastructure up and running with in our budget. You are fired!",
            new List<DialogButtonOption>()
            {
                new DialogButtonOption() { Text = "Start Over", OnClick = () =>
                    {
                        
                        ResetGame();
                    }
                },
                new DialogButtonOption() { Text = "Main Menu", OnClick = () =>
                    {
                        SceneManager.LoadScene("MainMenu");  
                    }
                },
            }
        );
    }
    public void ResetGame()
    {
        currentDay = 0;
        GameManager.Instance.ResetNPCs();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    public void BeginPlanPhase()
    {
   
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
                npc.OnPlanPhaseStart();
                npc.transform.position = vector3;
                npc.gameObject.SetActive(false);
          
            }
        }

        GameManager.Instance.UIManager.Resume();
        CurrentState = GameState.Plan;
        
        currentDay++;

        // Update UI
        GameManager.Instance.UIManager.UpdateGameStateDisplay(CurrentState.ToString());
        GameManager.Instance.UIManager.ShowPlanUI();
    }

 



    public void BeginPlayPhase()
    {
        CurrentState = GameState.Play;
        GameManager.Instance.InvokeOnPhaseChange(CurrentState);
        dayTimer = 0f;
        GameManager.Instance.SetStat(StatType.PacketsSent, 0);
        GameManager.Instance.SetStat(StatType.PacketsServiced, 0);
        GameManager.Instance.SetStat(StatType.PacketsFailed, 0);

        // Notify NPCs
        foreach (var npc in GameManager.Instance.AllNpcs)
        {
            npc.gameObject.SetActive(true);
            npc.OnPlayPhaseStart();
        }

        // Update UI
        GameManager.Instance.UIManager.UpdateGameStateDisplay(CurrentState.ToString());
        GameManager.Instance.UIManager.HidePlanUI();
        
        
        GameManager.Instance.UIManager.Resume();
        // GameManager.Instance.CheckEvents();
    }

    private void BeginSummaryPhase()
    {
       
        
        CurrentState = GameState.WaitingForNpcsToExpire;
        GameManager.Instance.InvokeOnPhaseChange(CurrentState);
        
        // --- Prepare Summary Text ---
        float totalDailyCost = GameManager.Instance.CalculateTotalDailyCost();
        GameManager.Instance.IncrStat(StatType.Money, totalDailyCost * -1);
        float dailyPacketIncome = GameManager.Instance.GetStat(StatType.DailyIncome);
    
        float packetsFailed = GameManager.Instance.GetStat(StatType.PacketsFailed);
    
    
        float packetsServiced = GameManager.Instance.GetStat(StatType.PacketsServiced);
        
        
        float percentageSuccess = packetsServiced / (packetsServiced + packetsFailed);
        if (packetsServiced < 10)
        {
            percentageSuccess = 0;
        }

        float actualIncome = (float)Math.Round(dailyPacketIncome * percentageSuccess);
        float money = GameManager.Instance.IncrStat(StatType.Money, actualIncome);

        string infraCosts = "\n\n<b>Infrastructure Costs:</b>\n";
        foreach (var instance in GameManager.Instance.ActiveInfrastructure)
        {
            float cost = instance.data.Stats.GetStatValue(StatType.Infra_DailyCost);
            if (instance.IsActive() && cost > 0)
            {
                infraCosts += $"{instance.data.DisplayName}: ${cost}\n";
            }
        }
        GameManager.Instance. Stats.AddModifier(
            StatType.Traffic,
            new StatModifier(StatModifier.ModifierType.Multiply,  GameManager.Instance.GetStat(StatType.Difficulty))
        );
        
        float adjustedDailyIncomeMultiplier = GameManager.Instance.GetStat(StatType.Difficulty) * percentageSuccess;
        Debug.Log($"adjustedDailyIncomeMultiplier: {adjustedDailyIncomeMultiplier} = {GameManager.Instance.GetStat(StatType.Difficulty)} * {percentageSuccess}");
        GameManager.Instance.Stats.AddModifier(
            StatType.DailyIncome,
            new StatModifier(StatModifier.ModifierType.Multiply, adjustedDailyIncomeMultiplier)
        );
        float updatedDailyIncome = GameManager.Instance.GetStat(StatType.DailyIncome);
        /*GameManager.Instance.FloatingTextFactory.ShowText(
            $"+${hourlyIncome}",
            GameManager.Instance.GetInfrastructureInstanceByID("internetPipes").transform.position,
            new Color(0f, 1f, 0f)
        );*/
        string summaryText = $"End of Day {currentDay - 1}\n" +
                             $"Total Packets: {packetsFailed  + packetsServiced} \n" +
                             $"Packets Failed: {packetsFailed} \n" +
                             $"Packets Succeeded: {packetsServiced} \n" +
                             $"Percentage Served: %{Math.Round(percentageSuccess * 100)} \n" +
                             $"Total Costs: ${Math.Round(totalDailyCost)} \n" +
                             $"Total Income: ${Math.Round(actualIncome)}\n" +
                             $"Net Income: ${actualIncome - totalDailyCost}\n" + 
                             $"Tomorrow's Expected Income: ${updatedDailyIncome} - ({Math.Round((1 - percentageSuccess) * 100)}% Failed Penalty)\n" +
                             $"Total: {money}";

        summaryText += infraCosts;

       
        
        if (GameManager.Instance.GetStat(StatType.Money) < 0)
        {
            summaryText += "\n\n<color=red>GAME OVER! You ran out of money.</color>";
            GameManager.Instance.UIManager.ShowSummaryUI(summaryText);
            EndGame();
            return;
        }
        GameManager.Instance.UIManager.ShowSummaryUI(summaryText);
        GameManager.Instance.MetaStats.Incr(MetaStat.Day);

        // Assign "go to door" task to all NPCs
        foreach (var npc in GameManager.Instance.AllNpcs.ToList())
        {
            if (npc.gameObject.activeInHierarchy)
            {
                npc.EndDay();
            }
        }
        
        foreach (var e in GameManager.Instance.CurrentEvents.ToList())
        {
            if (e.IsOver())
            {
                e.End();
            }
        }

    }
}
