using UnityEngine;
using UnityEngine.SceneManagement;

public class GameLoopManager : MonoBehaviour
{
    private DeskCameraController _deskCameraController;

    public enum GameState { Build, Play, Summary }
    public GameState CurrentState { get; private set; }

    public float dayDurationSeconds = 60f;
    public int currentDay = 0;
    public float dayTimer = 0f;

    private float summaryPhaseTimer = 0f;
    private const float SummaryPhaseDuration = 5f;

    void Start()
    {
        _deskCameraController = FindObjectOfType<DeskCameraController>();
        if (_deskCameraController == null)
        {
            Debug.LogError("GameLoopManager: DeskCameraController not found in scene!");
        }
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
                    if (GameManager.Instance.GetStat(StatType.Money) < 0)
                    {
                        Debug.Log("Ran out of money!");
                        EndGame();
                        return;
                    }
                    float packetsServiced = GameManager.Instance.GetStat(StatType.PacketsServiced);
                    float packetsFailed = GameManager.Instance.GetStat(StatType.PacketsFailed);
                    float packetsRatioRequirements = GameManager.Instance.GetStat(StatType.PRR);
                    if (packetsFailed > 0 && (packetsFailed / (packetsFailed + packetsServiced)) > packetsRatioRequirements)
                    {
                        Debug.Log("Failed too much!");
                        EndGame();
                    }
                    else
                    {
                        BeginBuildPhase();
                    }
                }
                break;
        }
    }

    public void EndGame()
    {
        Debug.Log("HIT GAME OVER CONDITION!");
        currentDay = 0;
        GameManager.Instance.ResetNPCs();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void BeginBuildPhase()
    {
        Time.timeScale = 1f;
        CurrentState = GameState.Build;
        currentDay++;

        _deskCameraController?.TransitionToDeskView();

        foreach (var npc in FindObjectsOfType<NPCDevOps>())
        {
            npc.OnBuildPhaseStart();
        }

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
        GameManager.Instance.SetStat(StatType.PacketsSent, 0);
        GameManager.Instance.SetStat(StatType.PacketsServiced, 0);
        GameManager.Instance.SetStat(StatType.PacketsFailed, 0);

        _deskCameraController?.TransitionToGameView();

        foreach (var npc in FindObjectsOfType<NPCDevOps>())
        {
            npc.OnPlayPhaseStart();
        }

        GameManager.Instance.UIManager.UpdateGameStateDisplay(CurrentState.ToString());
        GameManager.Instance.UIManager.HideBuildUI();
    }

    private void BeginSummaryPhase()
    {
        Time.timeScale = 1f;
        CurrentState = GameState.Summary;
        summaryPhaseTimer = 0f;

        _deskCameraController?.TransitionToDeskView();

        float totalDailyCost = GameManager.Instance.CalculateTotalDailyCost();
        GameManager.Instance.IncrStat(StatType.Money, totalDailyCost * -1);

        string summaryText = $"End of Day {currentDay}\n" +
                             $"Total Costs: -${totalDailyCost}";

        if (GameManager.Instance.GetStat(StatType.Money) < 0)
        {
            summaryText += "\n\n<color=red>GAME OVER! You ran out of money.</color>";
        }

        GameManager.Instance.UIManager.UpdateGameStateDisplay(CurrentState.ToString());
        GameManager.Instance.UIManager.ShowSummaryUI(summaryText);
    }
}
