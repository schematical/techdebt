// UIManager.cs
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    private Canvas mainCanvas;
    private GameObject buildPhaseUIContainer;
    private GameObject summaryPhaseUIContainer;
    private GameObject statsBarUIContainer;
    private GameObject hireDevOpsPanel;
    private GameObject tooltipPanel;
    
    private Dictionary<StatType, TextMeshProUGUI> statTexts = new Dictionary<StatType, TextMeshProUGUI>();
    private TextMeshProUGUI tooltipText;
    private Button tooltipButton;
    private TextMeshProUGUI totalDailyCostText;
    private TextMeshProUGUI gameStateText;

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else { Instance = this; SetupUIInfrastructure(); }
    }

    void OnEnable() 
    { 
        GameManager.OnStatsChanged += UpdateStatsDisplay; 
        GameManager.OnDailyCostChanged += UpdateDailyCostDisplay;
    }
    void OnDisable() 
    { 
        GameManager.OnStatsChanged -= UpdateStatsDisplay; 
        GameManager.OnDailyCostChanged -= UpdateDailyCostDisplay;
    }

    void Update()
    {
        // No dynamic tooltip positioning needed anymore
    }

    private void SetupUIInfrastructure()
    {
        if (FindObjectOfType<EventSystem>() == null)
        {
            var esGO = new GameObject("EventSystem");
            esGO.AddComponent<EventSystem>();
            esGO.AddComponent<InputSystemUIInputModule>();
        }

        mainCanvas = new GameObject("MainCanvas").AddComponent<Canvas>();
        mainCanvas.renderMode = RenderMode.ScreenSpaceCamera;
        mainCanvas.worldCamera = Camera.main;
        mainCanvas.planeDistance = 10; // Render UI 10 units in front of the camera
        mainCanvas.gameObject.AddComponent<CanvasScaler>();
        var graphicRaycaster = mainCanvas.gameObject.AddComponent<GraphicRaycaster>();

        // TEMPORARY DIAGNOSTIC: Re-enable GraphicRaycaster. It was disabled for debugging.
        // If issues persist, this might be the culprit, but for now we re-enable it.
        graphicRaycaster.enabled = true;
        Debug.LogWarning("DIAGNOSTIC: GraphicRaycaster on MainCanvas has been re-enabled.");
        
        SetupStatsBar(mainCanvas.transform);
        SetupBuildPhaseUI(mainCanvas.transform);
        SetupSummaryPhaseUI(mainCanvas.transform);
        SetupTooltip(mainCanvas.transform);
        
        buildPhaseUIContainer.SetActive(false);
        summaryPhaseUIContainer.SetActive(false);
        UpdateStatsDisplay();
    }

    private void SetupStatsBar(Transform parent)
    {
        statsBarUIContainer = CreateUIPanel(parent, "StatsBarUI", new Vector2(0, 40), new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -20));
        var layout = statsBarUIContainer.AddComponent<HorizontalLayoutGroup>();
        layout.padding = new RectOffset(10, 10, 5, 5);
        layout.spacing = 15;
        
        gameStateText = CreateText(statsBarUIContainer.transform, "GameStateText", "State: Initializing", 18);
        gameStateText.color = Color.cyan;

        foreach (StatType type in Enum.GetValues(typeof(StatType)))
        {
            statTexts.Add(type, CreateText(statsBarUIContainer.transform, type.ToString(), $"{type}: 0", 18));
        }
    }

    private void SetupBuildPhaseUI(Transform parent)
    {
        buildPhaseUIContainer = CreateUIPanel(parent, "BuildPhaseUI", new Vector2(220, 180), new Vector2(0, 0), new Vector2(0, 0), new Vector2(110, 90));
        var vlg = buildPhaseUIContainer.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(10,10,10,10);
        vlg.spacing = 5;

        totalDailyCostText = CreateText(buildPhaseUIContainer.transform, "DailyCostText", "Daily Cost: $0", 16);
        totalDailyCostText.color = Color.yellow;
        
        CreateButton(buildPhaseUIContainer.transform, "Hire NPCDevOps", () => hireDevOpsPanel.SetActive(true));
        CreateButton(buildPhaseUIContainer.transform, "Start Day", () => GameLoopManager.Instance.EndBuildPhaseAndStartPlayPhase());

        // --- Hire DevOps Panel (Sub-panel) ---
        hireDevOpsPanel = CreateUIPanel(parent, "HireDevOpsPanel", new Vector2(220, 150), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero);
        var hireVlg = hireDevOpsPanel.AddComponent<VerticalLayoutGroup>();
        hireVlg.padding = new RectOffset(10,10,10,10);
        hireVlg.spacing = 5;
        CreateButton(hireDevOpsPanel.transform, "< Back", () => hireDevOpsPanel.SetActive(false));
    }
    
    private void SetupSummaryPhaseUI(Transform parent)
    {
        summaryPhaseUIContainer = CreateUIPanel(parent, "SummaryPhaseUI", new Vector2(400, 100), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero);
        CreateText(summaryPhaseUIContainer.transform, "Summary Text", "", 24);
    }
    
    private void SetupTooltip(Transform parent)
    {
        tooltipPanel = CreateUIPanel(parent, "Tooltip", new Vector2(200, 150), new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(110, 0)); // Anchored to left middle, offset right
        var vlg = tooltipPanel.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(5,5,5,5);
        tooltipText = CreateText(tooltipPanel.transform, "TooltipText", "", 14);
        tooltipText.alignment = TextAlignmentOptions.Left;
        tooltipButton = CreateButton(tooltipPanel.transform, "Action", () => {});
        tooltipPanel.SetActive(false);
    }


    public void UpdateGameStateDisplay(string state)
    {
        if (gameStateText != null)
        {
            gameStateText.text = $"State: {state}";
        }
    }
    
    public void ShowInfrastructureTooltip(InfrastructureInstance instance)
    {
        Debug.Log($"ShowInfrastructureTooltip called for {instance.data.DisplayName}.");
        // Only show tooltip if conditions are met or it's not locked (planned/operational)
        bool conditionsMet = GameManager.Instance.AreUnlockConditionsMet(instance.data);
        if (instance.data.CurrentState == InfrastructureData.State.Locked && !conditionsMet)
        {
            Debug.Log($"Tooltip hidden for {instance.data.DisplayName} because unlock conditions are not met.");
            HideTooltip();
            return;
        }

        tooltipPanel.SetActive(true);
        string tooltipContent = $"<b>{instance.data.DisplayName}</b>\n";

        if (instance.data.CurrentState == InfrastructureData.State.Locked)
        {
            if (instance.data.UnlockConditions != null && instance.data.UnlockConditions.Length > 0)
            {
                tooltipContent += "\n<b>Unlock Requirements:</b>\n";
                foreach (var condition in instance.data.UnlockConditions)
                {
                    tooltipContent += $"- {GetConditionDescription(condition)}\n";
                }
            }
        }
        tooltipContent += $"\nDaily Cost: ${instance.data.DailyCost}\nBuild Time: {instance.data.BuildTime}s";

        tooltipText.text = tooltipContent;
        
        tooltipButton.gameObject.SetActive(true);
        tooltipButton.onClick.RemoveAllListeners();
        
        switch (instance.data.CurrentState)
        {
            case InfrastructureData.State.Locked:
                tooltipButton.GetComponentInChildren<TextMeshProUGUI>().text = "Locked";
                tooltipButton.interactable = false;
                break;
            case InfrastructureData.State.Unlocked:
                tooltipButton.GetComponentInChildren<TextMeshProUGUI>().text = "Plan Build";
                tooltipButton.interactable = true;
                Debug.Log($"[UIManager] Adding Plan Build listener for {instance.data.DisplayName} (State: {instance.data.CurrentState}).");
                tooltipButton.onClick.AddListener(() => {
                    Debug.Log($"[UIManager] Plan Build button clicked for {instance.data.DisplayName} (State: {instance.data.CurrentState}). Calling GameManager.Instance.PlanInfrastructure.");
                    GameManager.Instance.PlanInfrastructure(instance.data);
                });
                break;
            case InfrastructureData.State.Planned:
                tooltipButton.GetComponentInChildren<TextMeshProUGUI>().text = "Build Planned";
                tooltipButton.interactable = false;
                break;
            case InfrastructureData.State.Operational:
                tooltipButton.GetComponentInChildren<TextMeshProUGUI>().text = "Operational";
                tooltipButton.interactable = false;
                break;
        }
    }

    private string GetConditionDescription(UnlockCondition condition)
    {
        switch (condition.Type)
        {
            case UnlockCondition.ConditionType.Day:
                return $"Day {condition.RequiredValue}";
            default:
                return "Unknown Condition";
        }
    }

    public void HideTooltip() => tooltipPanel.SetActive(false);
    
    private void RefreshHireDevOpsPanel()
    {
        for (int i = hireDevOpsPanel.transform.childCount - 1; i > 0; i--) Destroy(hireDevOpsPanel.transform.GetChild(i).gameObject);

        var candidates = GameManager.Instance.GenerateNPCCandidates(3);
        foreach (var candidate in candidates)
        {
            CreateButton(hireDevOpsPanel.transform, $"Hire (${candidate.DailyCost}/day)", () => {
                GameManager.Instance.HireNPCDevOps(candidate);
                hireDevOpsPanel.SetActive(false);
            });
        }
    }
    
    private void UpdateStatsDisplay()
    {
        if (GameManager.Instance == null) return;
        foreach (var statText in statTexts)
        {
            statText.Value.text = $"{statText.Key}: {GameManager.Instance.GetStat(statText.Key)}";
        }
    }

    private void UpdateDailyCostDisplay()
    {
        if (GameManager.Instance == null) return;
        float totalCost = GameManager.Instance.CalculateTotalDailyCost();
        totalDailyCostText.text = $"Total Daily Cost: ${totalCost}";
    }

    public void ShowBuildUI()
    {
        buildPhaseUIContainer.SetActive(true);
        summaryPhaseUIContainer.SetActive(false);
        hireDevOpsPanel.SetActive(false);
        RefreshHireDevOpsPanel();
        UpdateDailyCostDisplay(); // Update cost when UI is shown
    }

    public void HideBuildUI() => buildPhaseUIContainer.SetActive(false);
    public void ShowSummaryUI(string text)
    {
        summaryPhaseUIContainer.SetActive(true);
        summaryPhaseUIContainer.GetComponentInChildren<TextMeshProUGUI>().text = text;
    }

    private GameObject CreateUIPanel(Transform p, string n, Vector2 s, Vector2 min, Vector2 max, Vector2 pos)
    {
        var go = new GameObject(n); go.transform.SetParent(p, false);
        var rt = go.AddComponent<RectTransform>(); rt.sizeDelta = s; rt.anchorMin = min; rt.anchorMax = max; rt.anchoredPosition = pos;
        go.AddComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
        return go;
    }

    private Button CreateButton(Transform p, string t, UnityAction a)
    {
        var go = new GameObject($"Button_{t}"); go.transform.SetParent(p, false);
        var rt = go.AddComponent<RectTransform>(); rt.sizeDelta = new Vector2(180, 40);
        go.AddComponent<Image>().color = Color.gray;
        var btn = go.AddComponent<Button>(); btn.onClick.AddListener(a);
        CreateText(btn.transform, "Text", t, 14);
        return btn;
    }

    private TextMeshProUGUI CreateText(Transform p, string n, string c, int s)
    {
        var go = new GameObject(n); go.transform.SetParent(p, false);
        var tmp = go.AddComponent<TextMeshProUGUI>(); tmp.text = c; tmp.fontSize = s; tmp.alignment = TextAlignmentOptions.Center; tmp.color = Color.white;
        return tmp;
    }
}
