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
using Stats;
using UI;
using Random = UnityEngine.Random;

public class UIDebugPanel : UIPanel
{
   
    public TextMeshProUGUI mouseCoordsText;
    public Button spawnNPC;
    protected StatModifier trafficStatModifier;
    protected UIPanelLineSectionText trafficText;

    void Start()
    {

      
        

    }

    public override void Show()
    {
        base.Show();
        AddTrafficLine();
        AddButton("Insta-Build", () => { InstaBuild(); });
        AddButton("Insta-Research", () => { InstaResearch(); });
        AddButton("Unlock All Tech", () => { UnlockAllTechnologies(); });
        AddButton("SpawnNPC", () => { SpawnNPC(); });
        AddButton("End Day", () => { EndDay(); }); 
        AddButton("End Run", () => { EndRun(); });
        AddButton("Next Sprint", () => { NextSprint(); });
        AddButton("Left Bar", () => { GameManager.Instance.UIManager.leftMenuPanel.Show(); });
        AddButton("Events", () =>
        {
            GameManager.Instance.UIManager.eventDebugPanel.Show();
        });
        AddButton("Make It Rain", () => { MakeItRain(); });
        AddButton("Misc", () => { RunMisc(); });
    }

    private UIPanelLine AddTrafficLine()
    {
        UIPanelLine trafficLine = AddLine<UIPanelLine>();
        trafficText = trafficLine.Add<UIPanelLineSectionText>();
        trafficText.text.text =
            $"Traffic: {GameManager.Instance.Stats.GetStatValue(StatType.Traffic)}";
        UIPanelLineSectionButton upButton = trafficLine.Add<UIPanelLineSectionButton>();
        upButton.text.text = "+";
        upButton.button.onClick.AddListener(() => { AdjustTraffic(1); });
        UIPanelLineSectionButton downButton = trafficLine.Add<UIPanelLineSectionButton>();
        downButton.text.text = "-";
        downButton.button.onClick.AddListener(() => { AdjustTraffic(-1); });
        return trafficLine;
    }

    private void AdjustTraffic(int i)
    {
        if (trafficStatModifier == null)
        {
            trafficStatModifier = new StatModifier("debug", 2f);
            GameManager.Instance.Stats.AddModifier(StatType.Traffic, trafficStatModifier);
        }

        if (i > 0)
        {
            trafficStatModifier.SetValue(trafficStatModifier.Value * 1.25f);
        }
        else
        {
            trafficStatModifier.SetValue(trafficStatModifier.Value * 1/1.25f);
        }
        trafficText.text.text =
            $"Traffic: {GameManager.Instance.Stats.GetStatValue(StatType.Traffic)}";

    }

    private void NextSprint()
    {
        GameManager.Instance.Map.IncrStage();
        Close();
        GameManager.Instance.UIManager.productRoadMap.Show(UIProductRoadMap.State.Select);
    }

    private void RunMisc()
    {

    }
    private void MakeItRain(){

        GameManager.Instance.UIManager.TriggerScreenShake(.25f, 1);
        NetworkPacketData networkPacketData = GameManager.Instance.GetNetworkPacketDatas().Find((data => data.Type == NetworkPacketData.PType.Purchase));
        if (networkPacketData == null)
        {
            foreach (NetworkPacketData networkPacketData2 in GameManager.Instance.GetNetworkPacketDatas())
            {
                Debug.LogError($"NetworkPacketData {networkPacketData2.Type}");
            }
            throw new System.Exception("No network packet found `NetworkPacketData.PType.Purchase`");
        }
        networkPacketData.Stats.Stats[StatType.NetworkPacket_Probibility].SetBaseValue(1000);
        GameManager.Instance.InfrastructureUpdateNetworkTargets();
        GameManager.Instance.UIManager.moneyPanel.AddCoin();
        GameManager.Instance.UIManager.moneyPanel.ExplodeCoins(10);
        
        
    }

    private void SpawnNPC()
    {
        Close();
        /*var door = GameManager.Instance.GetInfrastructureInstanceByID("door");
        if (door == null)
        {
            throw new SystemException("Cannot spawn NPC because 'server' infrastructure was not found.");
        }

        GameObject npcGO = GameManager.Instance.prefabManager.Create("NPCXSS", door.transform.position);
        if (npcGO == null)
        {
            throw new SystemException("Failed to create 'NPCXSS' from PrefabManager. Is the prefab configured?");
        }

        NPCXSS npc = npcGO.GetComponent<NPCXSS>();
        npc.SetStatCollection();
        GameManager.Instance.cameraController.ZoomToAndFollow(npc.transform);
        gameObject.SetActive(false);*/
        GameManager.Instance.SpawnNPCBug();
        InfrastructureInstance door = GameManager.Instance.GetInfrastructureInstanceByID("door");
        for (int i = 0; i < 5; i++)
        {
            GameObject npcGO = GameManager.Instance.prefabManager.Create("NukeItem",
                door.transform.position + new Vector3(Random.Range(-10, 10), Random.Range(-10, 10), 0));
        }


    }

    protected override void Update()
    {
        base.Update();
        Vector3 mousePos = Mouse.current.position.ReadValue();
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
        Vector3Int cellPos = GameManager.Instance.gridManager.grid.WorldToCell(worldPos);
        mouseCoordsText.text = cellPos.ToString();
  
    }

    private void InstaBuild()
    {

        Close();
        var plannedInfrastructure = GameManager.Instance.ActiveInfrastructure.FirstOrDefault(i => i.data.CurrentState == InfrastructureData.State.Planned);
        if (plannedInfrastructure != null)
        {
            plannedInfrastructure.SetState(InfrastructureData.State.Operational);
            Debug.Log($"Insta-built {plannedInfrastructure.data.Id}");
        }
        else
        {
            Debug.Log("No planned infrastructure to insta-build.");
        }
    }

    private void InstaResearch()
    {
        Close();
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
        Close();
        GameManager.Instance.UnlockAllTechnologies();
        Debug.Log("All technologies unlocked.");
        
        GameManager.Instance.UIManager.techTreePanel.Refresh();
    }

    private void EndRun()
    {
        Close();
        GameManager.Instance.Stats.Get(StatType.Money).SetBaseValue(-1000);
        GameManager.Instance.Map.GetCurrentLevel().EndGame();
    }  
    private void EndDay()
    {
        Close();
        GameManager.Instance.GameLoopManager.dayTimer = 1000;
    }
    
}
