using System;
using System.Collections.Generic;
using System.Linq;
using Stats;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameLoopManager : MonoBehaviour
{


    public enum GameState { Build, Play, WaitingForNpcsToExpire, Summary }
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

    public void EndGame()
    {
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
        // SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    public void BeginBuildPhase()
    {
   
        Time.timeScale = 1f;
        CurrentState = GameState.Build;

        /*if (currentDay > 0)
        {
            Sprite sprite = GameManager.Instance.prefabManager.GetPrefab("Manual").GetComponent<SpriteRenderer>().sprite;
            GameManager.Instance.UIManager.MultiSelectPanel.Add("manual1", sprite, "Manual 1", "$10").OnClick((string id) => Debug.Log("OnClick: " + id));
            GameManager.Instance.UIManager.MultiSelectPanel.Add("manual2", sprite, "Manual 2", "$10").OnClick((string id) => Debug.Log("OnClick: " + id));
            GameManager.Instance.UIManager.MultiSelectPanel.Add("manual3", sprite, "Manual 3", "$10").OnClick((string id) => Debug.Log("OnClick: " + id));

        }*/
        currentDay++;

        // Notify NPCs
        foreach (var npc in FindObjectsOfType<NPCDevOps>())
        {
            npc.OnBuildPhaseStart();
        }
        
      

        // Update UI
        GameManager.Instance.UIManager.UpdateGameStateDisplay(CurrentState.ToString());
        GameManager.Instance.UpdateInfrastructureVisibility();
        GameManager.Instance.UIManager.ShowBuildUI();
    }

    public void ForceBeginBuildPhase()
    {
        Vector3 vector3 = GameManager.Instance.GetInfrastructureInstanceByID("door").transform.position;
        foreach (var npc in FindObjectsOfType<NPCDevOps>())
        {
            if (npc.gameObject.activeInHierarchy)
            {
                npc.transform.position = vector3;
                npc.gameObject.SetActive(false);
            }
        }
        BeginBuildPhase();
    }

    public void EndBuildPhaseAndStartPlayPhase()
    {
        BeginPlayPhase();
    }

    private void BeginPlayPhase()
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
        GameManager.Instance.UIManager.HideBuildUI();
        
        GameManager.Instance. Stats.AddModifier(
            StatType.Traffic,
            new StatModifier(StatModifier.ModifierType.Multiply,  GameManager.Instance.GetStat(StatType.Difficulty))
        );
        GameManager.Instance. Stats.AddModifier(
            StatType.DailyIncome,
            new StatModifier(StatModifier.ModifierType.Multiply,  GameManager.Instance.GetStat(StatType.Difficulty))
        );
        GameManager.Instance.CheckEvents();
    }

    private void BeginSummaryPhase()
    {
       
        Time.timeScale = 1f;
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
                             $"Total Costs: ${totalDailyCost} \n" +
                             $"Total Income: ${actualIncome}\n" +
                             $"Net Income: ${actualIncome - totalDailyCost}\n" + 
                             $"Total: {money}";

        if (GameManager.Instance.GetStat(StatType.Money) < 0)
        {
            summaryText += "\n\n<color=red>GAME OVER! You ran out of money.</color>";
            EndGame();
        }
        GameManager.Instance.UIManager.ShowSummaryUI(summaryText);
  

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
