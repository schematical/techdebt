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
using DefaultNamespace;
using NPCs;

public class UIDebugPanel : UIPanel
{
   
    public TextMeshProUGUI mouseCoordsText;
    public Button spawnNPC;
    


    void Start()
    {

        AddButton("Insta-Build", () => { InstaBuild(); });
        AddButton("Insta-Research", () => { InstaResearch(); });
        AddButton("Unlock All Tech", () => { UnlockAllTechnologies(); });
        AddButton("Trigger Event", () => GameManager.Instance.UIManager.ToggleEventTriggerPanel());
        AddButton("SpawnNPC", () => { SpawnNPC(); });
        AddButton("End Run", () => { EndRun(); });
        AddButton("Export State", () => { ExportState(); }); 
        AddButton("Misc", () => { RunMisc(); });

    }

    private void RunMisc()
    {
        InstaBuild();
        foreach (var infra in GameManager.Instance.ActiveInfrastructure)
        {
            ApplicationServer applicationServer = infra.GetComponent<ApplicationServer>();
            if (
                applicationServer == null ||
                !infra.IsActive()
            )
            {
                continue;
            }

            applicationServer.ShowLevelUpGraphic(Rarity.Epic);
            GameManager.Instance.cameraController.ZoomTo(applicationServer.transform);
            
        }


        gameObject.SetActive(false);
        
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


        var plannedInfrastructure = GameManager.Instance.ActiveInfrastructure.FirstOrDefault(i => i.data.CurrentState == InfrastructureData.State.Planned);
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
  

        if (GameManager.Instance.CurrentlyResearchingTechnology != null)
        {
            string techName = GameManager.Instance.CurrentlyResearchingTechnology.DisplayName;
            GameManager.Instance.ApplyResearchProgress(GameManager.Instance.CurrentlyResearchingTechnology.ResearchPointCost);
            Debug.Log($"Insta-researched {techName}");
     
            GameManager.Instance.UIManager.techTreePanel.Refresh();
            
        }
        else
        {
            Debug.Log("No technology is currently being researched.");
        }
    }

    private void UnlockAllTechnologies()
    {


        GameManager.Instance.UnlockAllTechnologies();
        Debug.Log("All technologies unlocked.");
        if (GameManager.Instance.UIManager != null)
        {
            GameManager.Instance.UIManager.techTreePanel.Refresh();
        }
    }

    private void EndRun()
    {
        GameManager.Instance.Stats.Get(StatType.Money).SetBaseValue(-1000);
        GameManager.Instance.GameLoopManager.EndGame();
    }
    
    private void ExportState()
    {

        // Create a serializable container for the data
        GameStateExport exportData = new GameStateExport
        {
            ActiveInfrastructure = GameManager.Instance.ActiveInfrastructure.Select(i => i.data).ToList(),
            NetworkPacketDatas = GameManager.Instance.NetworkPacketDatas
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