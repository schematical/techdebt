using Stats;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameLoopManager : MonoBehaviour
{


    public enum GameState { Build, Play, WaitingForNpcsToExpire, Summary }
    public GameState CurrentState { get; private set; }

    public float dayDurationSeconds = 60f;
    public int currentDay = 0;
    public float dayTimer = 0f;
    
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
            case GameState.WaitingForNpcsToExpire:
                bool allNpcsExpired = true;
                foreach (var npc in FindObjectsOfType<NPCDevOps>())
                {
                    if (npc.gameObject.activeInHierarchy)
                    {
                        allNpcsExpired = false;
                        break;
                    }
                }

                if (allNpcsExpired)
                {
                    BeginBuildPhase();
                }
                break;
        }
    }

    public void EndGame()
    {
        currentDay = 0;
        GameManager.Instance.ResetNPCs();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
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

        if (currentDay > 1)
        {
            // --- Prepare Summary Text ---
            float totalDailyCost = GameManager.Instance.CalculateTotalDailyCost();
            GameManager.Instance.IncrStat(StatType.Money, totalDailyCost * -1);
            string summaryText = $"End of Day {currentDay - 1}\n" +
                                 $"Total Costs: -${totalDailyCost}";

            if (GameManager.Instance.GetStat(StatType.Money) < 0)
            {
                summaryText += "\n\n<color=red>GAME OVER! You ran out of money.</color>";
                EndGame();
            }
            GameManager.Instance.UIManager.ShowSummaryUI(summaryText);
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
        GameManager.Instance.CheckEvents();
    }

    private void BeginSummaryPhase()
    {
        Time.timeScale = 1f;
        CurrentState = GameState.WaitingForNpcsToExpire;

        // Assign "go to door" task to all NPCs
        foreach (var npc in FindObjectsOfType<NPCDevOps>())
        {
            if (npc.gameObject.activeInHierarchy)
            {
                npc.EndDay();
            }
        }
        
        foreach (var e in GameManager.Instance.Events)
        {
            if (e.IsOver())
            {
                e.End();
            }
        }

    }
}
