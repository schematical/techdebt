using UnityEngine;
using UnityEngine.SceneManagement;

public class GameLoopManager : MonoBehaviour
{


    public enum GameState { Build, Play, Summary }
    public GameState CurrentState { get; private set; }

    public float dayDurationSeconds = 60f;
    public int currentDay = 0;
    public float dayTimer = 0f;

    private float summaryPhaseTimer = 0f;
    private const float SummaryPhaseDuration = 5f;

    void Awake()
    {

    }



    void Update()
    {
        switch (CurrentState)
        {
            case GameState.Play:
                dayTimer += Time.deltaTime;
                GameManager.Instance.UIManager.UpdateClockDisplay(dayTimer, dayDurationSeconds);
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

    public void BeginBuildPhase()
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
        GameManager.Instance.UIManager.UpdateGameStateDisplay(CurrentState.ToString());
        GameManager.Instance.UpdateInfrastructureVisibility();
        GameManager.Instance.UIManager.ShowBuildUI();
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
        GameManager.Instance.UIManager.UpdateGameStateDisplay(CurrentState.ToString());
        GameManager.Instance.UIManager.HideBuildUI();
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
        GameManager.Instance.UIManager.UpdateGameStateDisplay(CurrentState.ToString());
        GameManager.Instance.UIManager.ShowSummaryUI(summaryText);
    }
}
