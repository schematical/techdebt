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
        CurrentState = GameState.Build;
        Debug.Log("Starting Build Phase...");
        currentDay++; // Day starts here
        GameManager.Instance.UpdateInfrastructureVisibility(); 
        UIManager.Instance.ShowBuildUI();
        yield return null; // Wait a frame
    }

    public void EndBuildPhaseAndStartPlayPhase()
    {
        StartCoroutine(StartPlayPhase());
    }
    
    private IEnumerator StartPlayPhase()
    {
        CurrentState = GameState.Play;
        Debug.Log($"Starting Play Phase for Day {currentDay}.");

        if (UIManager.Instance != null)
        {
            UIManager.Instance.HideBuildUI();
        }

        NPCDevOps[] allNpcs = FindObjectsOfType<NPCDevOps>();
        foreach (var npc in allNpcs)
        {
            npc.OnPlayPhaseStart();
        }

        yield return new WaitForSeconds(dayDurationSeconds);

        StartCoroutine(StartSummaryPhase());
    }

    private IEnumerator StartSummaryPhase()
    {
        CurrentState = GameState.Summary;
        Debug.Log($"Starting Summary Phase for Day {currentDay}.");

        // Deduct daily costs for all unlocked infrastructure and hired NPCs
        float totalDailyCost = GameManager.Instance.CalculateTotalDailyCost();

        string summaryText;
        if (GameManager.Instance.TrySpendStat(StatType.Money, totalDailyCost))
        {
            summaryText = $"End of Day {currentDay}\nTotal Costs: -${totalDailyCost}";
            Debug.Log($"Day {currentDay} ended. Deducted ${totalDailyCost} for costs.");
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
        StartCoroutine(StartBuildPhase());
    }
}
