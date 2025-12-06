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
    public int currentDay = 1;

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

    public IEnumerator StartBuildPhase()
    {
        CurrentState = GameState.Build;
        Debug.Log($"Starting Build Phase for Day {currentDay}.");

        // Wait for UIManager to be ready before showing UI
        yield return new WaitUntil(() => UIManager.Instance != null);
        UIManager.Instance.ShowBuildUI();

        // The build phase continues until the player ends it via the UI.
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

        // Tell all NPCDevOps to start their work
        NPCDevOps[] allNpcs = FindObjectsOfType<NPCDevOps>();
        foreach (var npc in allNpcs)
        {
            npc.OnPlayPhaseStart();
        }

        // Wait for the day to end
        yield return new WaitForSeconds(dayDurationSeconds);

        StartCoroutine(StartSummaryPhase());
    }

    private IEnumerator StartSummaryPhase()
    {
        CurrentState = GameState.Summary;
        Debug.Log($"Starting Summary Phase for Day {currentDay}.");

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowSummaryUI($"End of Day {currentDay}");
        }

        // Wait for a few seconds before starting the next day's build phase
        yield return new WaitForSeconds(5f); 
        
        // Deduct daily costs for all unlocked infrastructure and hired NPCs
        float totalDailyCost = 0f;
        if (GameManager.Instance != null && GameManager.Instance.AllInfrastructure != null)
        {
            foreach (var infra in GameManager.Instance.AllInfrastructure)
            {
                if (infra.IsUnlockedInGame)
                {
                    totalDailyCost += infra.DailyCost;
                }
            }
        }

        NPCDevOps[] allNpcs = FindObjectsOfType<NPCDevOps>();
        foreach (var npc in allNpcs)
        {
            totalDailyCost += npc.Data.DailyCost;
        }

        if (GameManager.Instance.TrySpendStat(StatType.Money, totalDailyCost))
        {
            Debug.Log($"Day {currentDay} ended. Deducted ${totalDailyCost} for costs.");
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowSummaryUI($"End of Day {currentDay}\nTotal Costs: -${totalDailyCost}");
            }
        }
        else
        {
            // Handle bankruptcy or other negative consequences
            Debug.LogWarning($"Day {currentDay} ended. Ran out of money! Game Over?");
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowSummaryUI($"End of Day {currentDay}\nRAN OUT OF MONEY!\nCosts: -${totalDailyCost}");
            }
            yield return new WaitForSeconds(5f); // Let player see the message
            // For now, just continue, but here you'd implement game over logic.
        }

        currentDay++;
        StartCoroutine(StartBuildPhase());
    }
}
