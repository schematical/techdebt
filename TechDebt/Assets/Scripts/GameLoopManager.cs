using UnityEngine;
using UnityEngine.SceneManagement;

public class GameLoopManager : MonoBehaviour
{
    public static GameLoopManager Instance { get; private set; }

    public enum GameState { Build, Play, Summary }
    public GameState CurrentState { get; private set; }

    public float dayDurationSeconds = 60f;
    public int currentDay = 0;
    public float dayTimer = 0f;

    private float summaryPhaseTimer = 0f;
    private const float SummaryPhaseDuration = 5f;

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    void Start()
    {
        BeginBuildPhase();
    }

    void Update()
    {
        switch (CurrentState)
        {
            case GameState.Play:
                dayTimer += Time.deltaTime;
                UIManager.Instance.UpdateClockDisplay(dayTimer, dayDurationSeconds);
                if (dayTimer >= dayDurationSeconds)
                {
                    BeginSummaryPhase();
                }
                break;

            case GameState.Summary:
                summaryPhaseTimer += Time.deltaTime;
                if (summaryPhaseTimer >= SummaryPhaseDuration)
                {
                    // Check for Game Over condition AFTER the summary has been displayed
                    Debug.Log("Checking for Game Over condition: " + GameManager.Instance.GetStat(StatType.Money));
                    if (GameManager.Instance.GetStat(StatType.Money) < 0)
                    {
                        // Reset and reload
                        Debug.Log("HIT GAME OVER CONDITION!");
                        currentDay = 0;
                        GameManager.Instance.ResetNPCs();
                        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                    }
                    else
                    {
                        // Proceed to the next day
                        BeginBuildPhase();
                    }
                }
                break;
        }
    }

    private void BeginBuildPhase()
    {
        Time.timeScale = 1f;
        CurrentState = GameState.Build;
        currentDay++;

        // Notify NPCs
        foreach (var npc in FindObjectsOfType<NPCDevOps>())
        {
            npc.OnBuildPhaseStart();
        }

        // Update UI
        UIManager.Instance.UpdateGameStateDisplay(CurrentState.ToString());
        GameManager.Instance.UpdateInfrastructureVisibility();
        UIManager.Instance.ShowBuildUI();
    }

    public void EndBuildPhaseAndStartPlayPhase()
    {
        BeginPlayPhase();
    }

    private void BeginPlayPhase()
    {
        CurrentState = GameState.Play;
        dayTimer = 0f;

        // Notify NPCs
        foreach (var npc in FindObjectsOfType<NPCDevOps>())
        {
            npc.OnPlayPhaseStart();
        }

        // Update UI
        UIManager.Instance.UpdateGameStateDisplay(CurrentState.ToString());
        UIManager.Instance.HideBuildUI();
    }

    private void BeginSummaryPhase()
    {
        Time.timeScale = 1f;
        CurrentState = GameState.Summary;
        summaryPhaseTimer = 0f;

        // --- Calculate Income & Expenses ---
        int packetIncome = GameManager.Instance.GetAndResetPacketRoundTripCount();
        GameManager.Instance.AddStat(StatType.Money, packetIncome);
        float totalDailyCost = GameManager.Instance.CalculateTotalDailyCost();
        GameManager.Instance.TrySpendStat(StatType.Money, totalDailyCost);

        // --- Prepare Summary Text ---
        string summaryText = $"End of Day {currentDay}\n" +
                             $"Packet Income: +${packetIncome}\n" +
                             $"Total Costs: -${totalDailyCost}";

        if (GameManager.Instance.GetStat(StatType.Money) < 0)
        {
            summaryText += "\n\n<color=red>GAME OVER! You ran out of money.</color>";
        }

        // --- Update UI ---
        UIManager.Instance.UpdateGameStateDisplay(CurrentState.ToString());
        UIManager.Instance.ShowSummaryUI(summaryText);
    }
}
