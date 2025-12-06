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
        
        currentDay++;
        StartCoroutine(StartBuildPhase());
    }
}
