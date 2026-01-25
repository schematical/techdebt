// DebugPanel.cs

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System.IO;

public class DebugPanel : MonoBehaviour
{
    public Button instaBuildButton;
    public Button instaResearchButton;
    public Button unlockAllTechButton;
    public Button triggerEventButton;
    public TextMeshProUGUI mouseCoordsText;

    private GameManager gameManager;
    private UIManager uiManager;
    private GridManager gridManager;

    void Start()
    {
        gameManager = GameManager.Instance;
        uiManager = FindObjectOfType<UIManager>();
        gridManager = GridManager.Instance;
        instaBuildButton.onClick.AddListener(InstaBuild);
        instaResearchButton.onClick.AddListener(InstaResearch);
        unlockAllTechButton.onClick.AddListener(UnlockAllTechnologies);

        // Create and configure the End Run button
        Button endRunButton = Instantiate(unlockAllTechButton, unlockAllTechButton.transform.parent);
        endRunButton.GetComponentInChildren<TextMeshProUGUI>().text = "End Run";
        endRunButton.onClick.AddListener(EndRun);
        endRunButton.transform.position = unlockAllTechButton.transform.position + new Vector3(0, -30, 0);

        // Create and configure the Export State button
        Button exportStateButton = Instantiate(unlockAllTechButton, unlockAllTechButton.transform.parent);
        exportStateButton.GetComponentInChildren<TextMeshProUGUI>().text = "Export State";
        exportStateButton.onClick.AddListener(ExportState);
        exportStateButton.transform.position = endRunButton.transform.position + new Vector3(0, -30, 0);
    }

    void Update()
    {
        if (mouseCoordsText != null && gridManager != null && Camera.main != null)
        {
            Vector3 mousePos = Mouse.current.position.ReadValue();
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
            Vector3Int cellPos = gridManager.grid.WorldToCell(worldPos);
            mouseCoordsText.text = $"X: {cellPos.x}, Y: {cellPos.y}";
        }
    }

    private void InstaBuild()
    {
        if (gameManager == null) return;

        var plannedInfrastructure = gameManager.ActiveInfrastructure.FirstOrDefault(i => i.data.CurrentState == InfrastructureData.State.Planned);
        if (plannedInfrastructure != null)
        {
            plannedInfrastructure.SetState(InfrastructureData.State.Operational);
            Debug.Log($"Insta-built {plannedInfrastructure.data.DisplayName}");
        }
        else
        {
            Debug.Log("No planned infrastructure to insta-build.");
        }
    }

    private void InstaResearch()
    {
        if (gameManager == null) return;

        if (gameManager.CurrentlyResearchingTechnology != null)
        {
            string techName = gameManager.CurrentlyResearchingTechnology.DisplayName;
            gameManager.ApplyResearchProgress(gameManager.CurrentlyResearchingTechnology.ResearchPointCost);
            Debug.Log($"Insta-researched {techName}");
            if (uiManager != null)
            {
                uiManager.RefreshTechTreePanel();
            }
        }
        else
        {
            Debug.Log("No technology is currently being researched.");
        }
    }

    private void UnlockAllTechnologies()
    {
        if (gameManager == null) return;

        gameManager.UnlockAllTechnologies();
        Debug.Log("All technologies unlocked.");
        if (uiManager != null)
        {
            uiManager.RefreshTechTreePanel();
        }
    }

    private void EndRun()
    {
        GameManager.Instance.Stats.Get(StatType.Money).SetBaseValue(-1000);
        GameManager.Instance.GameLoopManager.EndGame();
    }
    
    private void ExportState()
    {
        if (gameManager == null) return;

        // Create a serializable container for the data
        GameStateExport exportData = new GameStateExport
        {
            ActiveInfrastructure = gameManager.ActiveInfrastructure.Select(i => i.data).ToList(),
            NetworkPacketDatas = gameManager.NetworkPacketDatas
        };

        // Serialize to JSON using Unity's built-in utility
        string json = JsonUtility.ToJson(exportData, true);

        // Define file path using a platform-agnostic directory
        string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string fileName = $"GameState__{timestamp}.json";
        string filePath = Path.Combine(Application.persistentDataPath, fileName);

        try
        {
            File.WriteAllText(filePath, json);
            Debug.Log($"Game state exported to: {filePath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to export game state: {e.Message}");
        }
    }
}

[System.Serializable]
public class GameStateExport
{
    public List<InfrastructureData> ActiveInfrastructure;
    public List<NetworkPacketData> NetworkPacketDatas;
}