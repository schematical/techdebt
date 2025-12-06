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
    private Dictionary<StatType, TextMeshProUGUI> statTexts = new Dictionary<StatType, TextMeshProUGUI>();

    // Sub-panel containers
    private GameObject mainBuildPanel;
    private GameObject purchaseInfraPanel;
    private GameObject hireDevOpsPanel;

    private GameObject currentInfraPreview;
    private TextMeshProUGUI tooltipText;

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else { Instance = this; SetupUIInfrastructure(); }
    }

    void OnEnable() { GameManager.OnStatsChanged += UpdateStatsDisplay; }
    void OnDisable() { GameManager.OnStatsChanged -= UpdateStatsDisplay; }

    private void SetupUIInfrastructure()
    {
        if (FindObjectOfType<EventSystem>() == null)
        {
            var esGO = new GameObject("EventSystem");
            esGO.AddComponent<EventSystem>();
            esGO.AddComponent<InputSystemUIInputModule>();
        }

        GameObject canvasGO = new GameObject("MainCanvas");
        mainCanvas = canvasGO.AddComponent<Canvas>();
        mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();
        
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
        statsBarUIContainer.GetComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f, 0.85f);
        var layout = statsBarUIContainer.AddComponent<HorizontalLayoutGroup>();
        layout.padding = new RectOffset(10, 10, 5, 5);
        layout.spacing = 15;
        layout.childAlignment = TextAnchor.MiddleCenter;
        
        foreach (StatType type in Enum.GetValues(typeof(StatType)))
        {
            var text = CreateText(statsBarUIContainer.transform, type.ToString(), $"{type}: 0", 18);
            statTexts.Add(type, text);
        }
    }

    private void SetupBuildPhaseUI(Transform parent)
    {
        buildPhaseUIContainer = CreateUIPanel(parent, "BuildPhaseUI", new Vector2(220, 200), new Vector2(0, 0), new Vector2(0, 0), new Vector2(110, 100));
        var vlg = buildPhaseUIContainer.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(10,10,10,10);
        vlg.spacing = 5;

        // --- Main Build Panel ---
        mainBuildPanel = new GameObject("MainBuildPanel");
        mainBuildPanel.transform.SetParent(buildPhaseUIContainer.transform, false);
        var mainVlg = mainBuildPanel.AddComponent<VerticalLayoutGroup>();
        mainVlg.spacing = 5;
        CreateButton(mainBuildPanel.transform, "Purchase Infrastructure", () => ShowSubPanel(purchaseInfraPanel));
        CreateButton(mainBuildPanel.transform, "Hire NPCDevOps", () => { RefreshHireDevOpsPanel(); ShowSubPanel(hireDevOpsPanel); });
        CreateButton(mainBuildPanel.transform, "Start Day", () => GameLoopManager.Instance.EndBuildPhaseAndStartPlayPhase());

        // --- Purchase Infrastructure Panel ---
        purchaseInfraPanel = new GameObject("PurchaseInfraPanel");
purchaseInfraPanel.transform.SetParent(buildPhaseUIContainer.transform, false);
        var infraVlg = purchaseInfraPanel.AddComponent<VerticalLayoutGroup>();
        infraVlg.spacing = 5;
        CreateButton(purchaseInfraPanel.transform, "< Back", () => ShowSubPanel(mainBuildPanel));
        
        // --- Hire DevOps Panel ---
        hireDevOpsPanel = new GameObject("HireDevOpsPanel");
hireDevOpsPanel.transform.SetParent(buildPhaseUIContainer.transform, false);
        var hireVlg = hireDevOpsPanel.AddComponent<VerticalLayoutGroup>();
        hireVlg.spacing = 5;
        CreateButton(hireDevOpsPanel.transform, "< Back", () => ShowSubPanel(mainBuildPanel));
    }
    
    private void SetupSummaryPhaseUI(Transform parent)
    {
        summaryPhaseUIContainer = CreateUIPanel(parent, "SummaryPhaseUI", new Vector2(400, 100), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero);
        CreateText(summaryPhaseUIContainer.transform, "Summary Text", "End of Day Summary", 24);
    }
    
    private void SetupTooltip(Transform parent)
    {
        var tooltipGO = CreateUIPanel(parent, "Tooltip", new Vector2(200, 60), Vector2.zero, Vector2.zero, Vector2.zero);
        tooltipText = CreateText(tooltipGO.transform, "TooltipText", "", 14);
        tooltipText.alignment = TextAlignmentOptions.Left;
        tooltipGO.SetActive(false);
    }

    public void RefreshBuildUI()
    {
        ShowSubPanel(mainBuildPanel);

        // Clear old infra buttons (skip first button, which is "Back")
        for (int i = purchaseInfraPanel.transform.childCount - 1; i > 0; i--) Destroy(purchaseInfraPanel.transform.GetChild(i).gameObject);

        if (GameManager.Instance != null)
        {
            foreach (var infra in GameManager.Instance.AllInfrastructure)
            {
                if (!infra.IsUnlockedInGame)
                {
                    var btn = CreateButton(purchaseInfraPanel.transform, $"Buy {infra.DisplayName}", () => GameManager.Instance.UnlockInfrastructure(infra));
                    AddEventTrigger(btn.gameObject, EventTriggerType.PointerEnter, (e) => ShowInfraPreview(infra));
                    AddEventTrigger(btn.gameObject, EventTriggerType.PointerExit, (e) => HideInfraPreview());
                }
            }
        }
    }

    private void RefreshHireDevOpsPanel()
    {
        // Clear old hire buttons (skip "Back" button)
        for (int i = hireDevOpsPanel.transform.childCount - 1; i > 0; i--) Destroy(hireDevOpsPanel.transform.GetChild(i).gameObject);

        var candidates = GameManager.Instance.GenerateNPCCandidates(3);
        foreach (var candidate in candidates)
        {
            CreateButton(hireDevOpsPanel.transform, $"Hire {candidate.Name} (${candidate.DailyCost}/day)", () => {
                GameManager.Instance.HireNPCDevOps(candidate);
                ShowSubPanel(mainBuildPanel); // Go back to main menu after hiring
            });
        }
    }

    private void ShowSubPanel(GameObject panelToShow)
    {
        mainBuildPanel.SetActive(panelToShow == mainBuildPanel);
        purchaseInfraPanel.SetActive(panelToShow == purchaseInfraPanel);
        hireDevOpsPanel.SetActive(panelToShow == hireDevOpsPanel);
    }

    private void ShowInfraPreview(InfrastructureData infra)
    {
        if (currentInfraPreview != null) Destroy(currentInfraPreview);

        Vector3 worldPos = GridManager.Instance.gridComponent.CellToWorld(new Vector3Int(infra.GridPosition.x, infra.GridPosition.y, 0));
        currentInfraPreview = Instantiate(infra.Prefab, worldPos, Quaternion.identity);

        foreach (var sr in currentInfraPreview.GetComponentsInChildren<SpriteRenderer>())
        {
            sr.color = new Color(0.5f, 1f, 0.5f, 0.5f); // Greenish tint, semi-transparent
        }
        
        tooltipText.transform.parent.gameObject.SetActive(true);
        tooltipText.text = $"<b>{infra.DisplayName}</b>\nUnlock Cost: ${infra.UnlockCost}\nDaily Cost: ${infra.DailyCost}";
    }

    void Update()
    {
        if(tooltipText.transform.parent.gameObject.activeSelf)
        {
            // Tooltip follows mouse
            tooltipText.transform.parent.GetComponent<RectTransform>().position = Mouse.current.position.ReadValue();
        }
    }

    private void HideInfraPreview()
    {
        if (currentInfraPreview != null) Destroy(currentInfraPreview);
        tooltipText.transform.parent.gameObject.SetActive(false);
    }
    
    private void UpdateStatsDisplay()
    {
        if (GameManager.Instance == null) return;
        foreach (var statText in statTexts)
        {
            statText.Value.text = $"{statText.Key}: {GameManager.Instance.GetStat(statText.Key)}";
        }
    }

    public void ShowBuildUI()
    {
        buildPhaseUIContainer.SetActive(true);
        summaryPhaseUIContainer.SetActive(false);
        RefreshBuildUI();
    }

    public void HideBuildUI() => buildPhaseUIContainer.SetActive(false);
    public void ShowSummaryUI(string text)
    {
        summaryPhaseUIContainer.SetActive(true);
        summaryPhaseUIContainer.GetComponentInChildren<TextMeshProUGUI>().text = text;
    }

    // --- UI Helper Methods ---
    private GameObject CreateUIPanel(Transform parent, string name, Vector2 size, Vector2 anchorMin, Vector2 anchorMax, Vector2 pos)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.sizeDelta = size;
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.anchoredPosition = pos;
        go.AddComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f, 0.7f);
        return go;
    }

    private Button CreateButton(Transform parent, string text, UnityAction action)
    {
        GameObject go = new GameObject($"{text.Replace(" ", "")}Button");
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(180, 40);
        go.AddComponent<Image>().color = Color.gray;
        var btn = go.AddComponent<Button>();
        btn.onClick.AddListener(action);
        CreateText(btn.transform, "ButtonText", text, 14);
        return btn;
    }

    private TextMeshProUGUI CreateText(Transform parent, string name, string content, int size)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = content;
        tmp.fontSize = size;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        return tmp;
    }

    private void AddEventTrigger(GameObject go, EventTriggerType type, UnityAction<BaseEventData> action)
    {
        EventTrigger trigger = go.GetComponent<EventTrigger>() ?? go.AddComponent<EventTrigger>();
        var entry = new EventTrigger.Entry { eventID = type };
        entry.callback.AddListener(action);
        trigger.triggers.Add(entry);
    }
}