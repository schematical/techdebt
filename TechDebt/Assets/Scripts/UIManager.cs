using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
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


    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            SetupUIInfrastructure();
        }
    }

    void OnEnable()
    {
        GameManager.OnStatsChanged += UpdateStatsDisplay;
    }

    void OnDisable()
    {
        GameManager.OnStatsChanged -= UpdateStatsDisplay;
    }

    private void SetupUIInfrastructure()
    {
        // Ensure there's an EventSystem
        if (FindObjectOfType<EventSystem>() == null)
        {
            var eventSystemGO = new GameObject("EventSystem");
            eventSystemGO.AddComponent<EventSystem>();
            eventSystemGO.AddComponent<InputSystemUIInputModule>();
        }

        // Create the main Canvas
        GameObject canvasGO = new GameObject("MainCanvas");
        mainCanvas = canvasGO.AddComponent<Canvas>();
        mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();
        
        // --- Stats Bar UI ---
        statsBarUIContainer = CreateUIPanel(mainCanvas.transform, "StatsBarUI", new Vector2(0, 40), new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -20));
        statsBarUIContainer.GetComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f, 0.85f);
        var layoutGroup = statsBarUIContainer.AddComponent<HorizontalLayoutGroup>();
        layoutGroup.padding = new RectOffset(10, 10, 5, 5);
        layoutGroup.spacing = 15;
        layoutGroup.childAlignment = TextAnchor.MiddleCenter;
        layoutGroup.childControlWidth = true;
        layoutGroup.childForceExpandWidth = true;
        SetupStatBar();
        
        // --- Build Phase UI ---
        buildPhaseUIContainer = CreateUIPanel(mainCanvas.transform, "BuildPhaseUI", new Vector2(200, 150), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 10));
        CreateButton(buildPhaseUIContainer.transform, "Hire NPCDevOps", new Vector2(0, 90), () => GameManager.Instance.HireNPCDevOps());
        CreateButton(buildPhaseUIContainer.transform, "Place Server", new Vector2(0, 45), () => GameManager.Instance.PlaceNewServer());
        CreateButton(buildPhaseUIContainer.transform, "Start Day", new Vector2(0, 0), () => GameLoopManager.Instance.EndBuildPhaseAndStartPlayPhase());
        
        // --- Summary Phase UI ---
        summaryPhaseUIContainer = CreateUIPanel(mainCanvas.transform, "SummaryPhaseUI", new Vector2(400, 100), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero);
        CreateText(summaryPhaseUIContainer.transform, "Summary Text", "End of Day Summary", 24);
        
        // Initially hide some UI containers
        buildPhaseUIContainer.SetActive(false);
        summaryPhaseUIContainer.SetActive(false);
        
        // Initial stats display
        UpdateStatsDisplay();
    }
    
    private void SetupStatBar()
    {
        var statTypes = System.Enum.GetValues(typeof(StatType));
        foreach (StatType type in statTypes)
        {
            var textComponent = CreateText(statsBarUIContainer.transform, type.ToString(), $"{type}: 0", 18);
            // The HorizontalLayoutGroup now handles all positioning.
            statTexts.Add(type, textComponent);
        }
    }
    
    private void UpdateStatsDisplay()
    {
        if (GameManager.Instance == null) return;

        foreach (var statText in statTexts)
        {
            float value = GameManager.Instance.GetStat(statText.Key);
            statText.Value.text = $"{statText.Key}: {value}";
        }
    }

    public void ShowBuildUI()
    {
        buildPhaseUIContainer.SetActive(true);
        summaryPhaseUIContainer.SetActive(false);
    }

    public void HideBuildUI()
    {
        buildPhaseUIContainer.SetActive(false);
    }

    public void ShowSummaryUI(string summaryText)
    {
        summaryPhaseUIContainer.SetActive(true);
        TextMeshProUGUI tmp = summaryPhaseUIContainer.GetComponentInChildren<TextMeshProUGUI>();
        if (tmp != null)
        {
            tmp.text = summaryText;
        }
    }

    // --- UI Helper Methods ---

    private GameObject CreateUIPanel(Transform parent, string name, Vector2 size, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition)
    {
        GameObject panelGO = new GameObject(name);
        panelGO.transform.SetParent(parent, false);
        RectTransform rt = panelGO.AddComponent<RectTransform>();
        rt.sizeDelta = size;
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.anchoredPosition = anchoredPosition;
        panelGO.AddComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f, 0.7f);
        return panelGO;
    }

    private void CreateButton(Transform parent, string text, Vector2 position, UnityAction onClickAction)
    {
        GameObject buttonGO = new GameObject($"{text.Replace(" ", "")}Button");
        buttonGO.transform.SetParent(parent, false);

        RectTransform rt = buttonGO.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(180, 40);
        rt.anchoredPosition = position;
        
        buttonGO.AddComponent<Image>().color = Color.gray;
        Button button = buttonGO.AddComponent<Button>();
        button.onClick.AddListener(onClickAction);

        CreateText(button.transform, "ButtonText", text, 18);
    }

    private TextMeshProUGUI CreateText(Transform parent, string name, string content, int fontSize)
    {
        GameObject textGO = new GameObject(name);
        textGO.transform.SetParent(parent, false);
        TextMeshProUGUI tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.text = content;
        tmp.fontSize = fontSize;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        return tmp;
    }
}
