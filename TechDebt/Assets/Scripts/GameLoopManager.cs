using UnityEngine;
using UnityEngine.SceneManagement;

public class GameLoopManager : MonoBehaviour
{
    private CameraController _cameraController;
    private DeskOverlayController _deskOverlayController;

    public enum GameState { Build, Play, Summary }
    public GameState CurrentState { get; private set; }

    public float dayDurationSeconds = 60f;
    public int currentDay = 0;
    public float dayTimer = 0f;

    private float summaryPhaseTimer = 0f;
    private const float SummaryPhaseDuration = 5f;

    void Awake()
    {
        _cameraController = FindObjectOfType<CameraController>();
        if (_cameraController == null)
        {
            Debug.LogError("GameLoopManager: CameraController not found in scene!");
        }
    }

    void Start()
    {
        // Get the DeskOverlayController reference from the GameManager in Start()
        // to ensure GameManager has completed its Awake() cycle.
        _deskOverlayController = GameManager.Instance.DeskOverlayController;
        if (_deskOverlayController == null)
        {
            Debug.LogError("GameLoopManager: DeskOverlayController not found in scene! Make sure it's assigned in GameManager.");
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
                    // Check for Game Over condition AFTER the summary has been displayed
                    
                    
                    if (GameManager.Instance.GetStat(StatType.Money) < 0)
                    {
                        // Reset and reload
                        Debug.Log("Ran out of money!");
                        EndGame();
                        return;
                    }
                    float packetsServiced = GameManager.Instance.GetStat(StatType.PacketsServiced);
                    float packetsFailed = GameManager.Instance.GetStat(StatType.PacketsFailed);
                    float packetsRatioRequirements = GameManager.Instance.GetStat(StatType.PRR);
                    float packetFaledPct = packetsFailed / (packetsFailed + packetsServiced);
                    if (packetFaledPct > packetsRatioRequirements)
                    {
                        Debug.Log("Failed too much!");
                        EndGame();
                    }
                    if (GameManager.Instance.GetStat(StatType.Money) < 0)
                    {
                        // Reset and reload
                        Debug.Log("Ran out of money!");
                        EndGame();
                        return;
                    }
                    
                    BeginBuildPhase();
                    
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

        // Show the desk overlay and transition camera
        _deskOverlayController?.ShowOverlay();
        _cameraController?.TransitionToDeskView();

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
        GameManager.Instance.SetStat(StatType.PacketsSent, 0);
        GameManager.Instance.SetStat(StatType.PacketsServiced, 0);
        GameManager.Instance.SetStat(StatType.PacketsFailed, 0);

        // Transition camera to game view (zoomed into the screen)
        _cameraController?.TransitionToGameView();

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

        // Show the desk overlay and start camera transition
        _deskOverlayController?.ShowOverlay();
        _cameraController?.TransitionToDeskView();

        float totalDailyCost = GameManager.Instance.CalculateTotalDailyCost();
        GameManager.Instance.IncrStat(StatType.Money, totalDailyCost * -1);

        // --- Prepare Summary Text ---
        string summaryText = $"End of Day {currentDay}\n" +
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
