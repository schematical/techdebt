// DebugPanel.cs

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System.IO;
using NPCs;

public class UIDebugPanel : UIPanel
{
   
    public TextMeshProUGUI mouseCoordsText;
    public Button spawnNPC;
    
    private GameManager gameManager;
    private UIManager uiManager;
    private GridManager gridManager;

    void Start()
    {

        AddButton("Insta-Build", () => { InstaBuild(); });
        AddButton("Insta-Research", () => { InstaResearch(); });
        AddButton("Unlock All Tech", () => { UnlockAllTechnologies(); });
        AddButton("Trigger Event", () => GameManager.Instance.UIManager.ToggleEventTriggerPanel());
        AddButton("SpawnNPC", () => { SpawnNPC(); });
        AddButton("End Run", () => { EndRun(); });
        AddButton("Export State", () => { ExportState(); });

    }

    private void SpawnNPC()
    {
        var door = GameManager.Instance.GetInfrastructureInstanceByID("door");
        if (door == null)
        {
            throw new SystemException("Cannot spawn NPCBug because 'server' infrastructure was not found.");
        }

        GameObject npcGO = GameManager.Instance.prefabManager.Create("NPCBugMinor", door.transform.position);
        if (npcGO == null)
        {
            throw new SystemException("Failed to create 'NPCBug' from PrefabManager. Is the prefab configured?");
        }

        NPCBug npcBug = npcGO.GetComponent<NPCBug>();
        npcBug.Initialize();
        GameManager.Instance.cameraController.ZoomToAndFollow(npcBug.transform);
        gameObject.SetActive(false);
        
    }

    void Update()
    {
        Vector3 mousePos = Mouse.current.position.ReadValue();
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
        Vector3Int cellPos = GameManager.Instance.gridManager.grid.WorldToCell(worldPos);
        mouseCoordsText.text = cellPos.ToString();
  
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