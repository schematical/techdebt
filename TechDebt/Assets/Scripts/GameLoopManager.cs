// GameLoopManager.cs
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GameLoopManager : MonoBehaviour
{
    public static GameLoopManager Instance { get; private set; }

    public enum GameState { Build, Play, Summary }
    public GameState CurrentState { get; private set; }

    public float dayDurationSeconds = 60f; // A day is 60 seconds long
    public int currentDay = 0;
    public float dayTimer = 0f;

    void Update()
    {
        if (CurrentState == GameState.Play)
        {
            dayTimer += Time.deltaTime;
            UIManager.Instance.UpdateClockDisplay(dayTimer, dayDurationSeconds);

            if (dayTimer >= dayDurationSeconds)
            {
                StartCoroutine(StartSummaryPhase());
                return; // Prevent calling the coroutine multiple times
            }
        }
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    void Start()
    {
        // The game begins in the Build phase
        StartCoroutine(StartBuildPhase());
    }

    private IEnumerator StartBuildPhase()
    {
        Time.timeScale = 1f; // Reset time scale
        CurrentState = GameState.Build;
        
        // Notify all NPCs to stop their current tasks and go idle
        foreach (var npc in FindObjectsOfType<NPCDevOps>())
        {
            npc.OnBuildPhaseStart();
        }

        // Wait a frame to ensure UIManager is ready before calling it
        yield return null; 
        UIManager.Instance.UpdateGameStateDisplay(CurrentState.ToString());

        currentDay++; // Day starts here
        GameManager.Instance.UpdateInfrastructureVisibility(); 
        UIManager.Instance.ShowBuildUI();
    }

    public void EndBuildPhaseAndStartPlayPhase()
    {
        StartCoroutine(StartPlayPhase());
    }
    
    private IEnumerator StartPlayPhase()
    {
        CurrentState = GameState.Play;
        UIManager.Instance.UpdateGameStateDisplay(CurrentState.ToString());

        if (UIManager.Instance != null)
        {
            UIManager.Instance.HideBuildUI();
        }

        
        dayTimer = 0f; // Reset timer at the start of the day
        
        NPCDevOps[] allNpcs = FindObjectsOfType<NPCDevOps>();
        foreach (var npc in allNpcs)
        {
            npc.OnPlayPhaseStart();
        }

        yield return null; // Coroutine must yield something
    }

    private bool isSummaryPhaseStarted = false;
    private IEnumerator StartSummaryPhase()
    {
        if (isSummaryPhaseStarted) yield break;
        isSummaryPhaseStarted = true;

        Time.timeScale = 1f; // Reset time scale
        CurrentState = GameState.Summary;
        UIManager.Instance.UpdateGameStateDisplay(CurrentState.ToString());

        // Deduct daily costs for all unlocked infrastructure and hired NPCs
        float totalDailyCost = GameManager.Instance.CalculateTotalDailyCost();


        string summaryText;
        if (GameManager.Instance.TrySpendStat(StatType.Money, totalDailyCost))
        {
            summaryText = $"End of Day {currentDay}\nTotal Costs: -${totalDailyCost}";
        }
        else
        {
            summaryText = $"End of Day {currentDay}\nRAN OUT OF MONEY!\nCosts: -${totalDailyCost}";
            Debug.LogWarning($"Day {currentDay} ended. Ran out of money! Game Over?");
        }
        
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowSummaryUI(summaryText);
        }
        
        yield return new WaitForSeconds(5f); // Let player see the message

        // Transition to the next day's build phase
        isSummaryPhaseStarted = false; // Reset for the next day
        StartCoroutine(StartBuildPhase());
    }
}
