// UIManager.cs
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System;
using System.Linq;
using static NPCTask;

public class UIManager : MonoBehaviour
{


    // UI Containers
    private Canvas mainCanvas;
    private GameObject buildPhaseUIContainer;
    private GameObject summaryPhaseUIContainer;
    private GameObject statsBarUIContainer;
    private GameObject hireDevOpsPanel;
    private GameObject tooltipPanel;
    private GameObject timeControlsContainer;
    private GameObject leftMenuBar;
    private GameObject taskListPanel;
    private GameObject techTreePanel;
    
    // UI Elements
    private Dictionary<StatType, TextMeshProUGUI> statTexts = new Dictionary<StatType, TextMeshProUGUI>();
    private TextMeshProUGUI tooltipText;
    private Button tooltipButton;
    private TextMeshProUGUI totalDailyCostText;
    private TextMeshProUGUI gameStateText;
    private TextMeshProUGUI clockText;

    // Time Control Buttons & Colors
    private Button pauseButton, playButton, fastForwardButton, superFastForwardButton;
    private Color activeColor = new Color(0.5f, 0.8f, 1f); // Light blue for active button
    private Color inactiveColor = Color.gray;
    
    // Task List
    private Transform taskListContent;
    private float taskListUpdateCooldown = 0.5f;
    private float lastTaskListUpdateTime;
    
    public Transform techTreeContent;
    
    void OnEnable() 
    { 
        GameManager.OnStatsChanged += UpdateStatsDisplay; 
        GameManager.OnDailyCostChanged += UpdateDailyCostDisplay;
        GameManager.OnTechnologyUnlocked += RefreshTechTreePanelOnEvent;
    }
    void OnDisable() 
    {
        GameManager.OnStatsChanged -= UpdateStatsDisplay; 
        GameManager.OnDailyCostChanged -= UpdateDailyCostDisplay;
        GameManager.OnTechnologyUnlocked -= RefreshTechTreePanelOnEvent;
    }
    
    // An event handler to refresh the panel if it's active when a tech is unlocked.
    public void RefreshTechTreePanelOnEvent(Technology tech)
    {
        if (techTreePanel != null && techTreePanel.activeSelf)
        {
            RefreshTechTreePanel();
        }
    }
    
    void Update()
    {
        if (taskListPanel.activeSelf && Time.time - lastTaskListUpdateTime > taskListUpdateCooldown)        {
            RefreshTaskList();
            lastTaskListUpdateTime = Time.time;
        }
    }


    public void SetupUIInfrastructure()
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
        mainCanvas.planeDistance = 10;
        mainCanvas.gameObject.AddComponent<CanvasScaler>();
        mainCanvas.gameObject.AddComponent<GraphicRaycaster>();
        
        SetupStatsBar(mainCanvas.transform);
        SetupBuildPhaseUI(mainCanvas.transform);
        SetupSummaryPhaseUI(mainCanvas.transform);
        SetupTooltip(mainCanvas.transform);
        SetupTimeControls(mainCanvas.transform);
        SetupLeftMenuBar(mainCanvas.transform);
        
        // Initial state
        buildPhaseUIContainer.SetActive(false);
        summaryPhaseUIContainer.SetActive(false);
        timeControlsContainer.SetActive(false);
        UpdateStatsDisplay();
    }
    
    #region UI Setup Methods
    private void SetupLeftMenuBar(Transform parent)
    {
        leftMenuBar = CreateUIPanel(parent, "LeftMenuBar", new Vector2(50, 0), new Vector2(0, 0), new Vector2(0, 1), new Vector2(25, 0));
        var vlg = leftMenuBar.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(5, 5, 10, 10);
        vlg.spacing = 10;
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;

        CreateButton(leftMenuBar.transform, "Tasks", ToggleTaskListPanel, new Vector2(40, 40));
        CreateButton(leftMenuBar.transform, "Tech", ToggleTechTreePanel, new Vector2(40, 40));
        
        SetupTaskListPanel(parent);
        SetupTechTreePanel(parent);
    }
    
    private void SetupTaskListPanel(Transform parent)
    {
        taskListPanel = CreateUIPanel(parent, "TaskListPanel", new Vector2(300, 0), new Vector2(0, 0), new Vector2(0, 1), new Vector2(175, 0));
        var vlg = taskListPanel.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(10, 10, 10, 10);
        vlg.spacing = 5;
        vlg.childControlWidth = true;

        var header = CreateText(taskListPanel.transform, "Header", "Available Tasks", 20);
        header.GetComponent<RectTransform>().sizeDelta = new Vector2(280, 30);

        var scrollView = new GameObject("ScrollView");
        scrollView.transform.SetParent(taskListPanel.transform, false);
        var scrollRect = scrollView.AddComponent<ScrollRect>();
        var scrollRt = scrollView.GetComponent<RectTransform>();
        scrollRt.sizeDelta = new Vector2(280, 0); // Width is fixed, height will be flexible
        
        var viewport = new GameObject("Viewport");
        viewport.transform.SetParent(scrollView.transform, false);
        viewport.AddComponent<RectMask2D>();
        var viewportImage = viewport.AddComponent<Image>();
        viewportImage.color = new Color(0.1f, 0.1f, 0.1f, 0.5f);
        scrollRect.viewport = viewport.GetComponent<RectTransform>();
        
        var contentGo = new GameObject("Content");
        contentGo.transform.SetParent(viewport.transform, false);
        taskListContent = contentGo.transform;
        var contentVlg = contentGo.AddComponent<VerticalLayoutGroup>();
        contentVlg.padding = new RectOffset(5,5,5,5);
        contentVlg.spacing = 8;
        contentVlg.childControlWidth = true;
        var csf = contentGo.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        scrollRect.content = contentGo.GetComponent<RectTransform>();

        // Set anchors for proper scrolling behavior
        var viewportRt = viewport.GetComponent<RectTransform>();
        viewportRt.anchorMin = Vector2.zero;
        viewportRt.anchorMax = Vector2.one;
        viewportRt.sizeDelta = Vector2.zero;

        var contentRt = contentGo.GetComponent<RectTransform>();
        contentRt.anchorMin = new Vector2(0, 1);
        contentRt.anchorMax = new Vector2(1, 1);
        contentRt.pivot = new Vector2(0.5f, 1);
        contentRt.sizeDelta = new Vector2(0, 0);

        taskListPanel.SetActive(false); // Start hidden
    }

    
    private void ToggleTaskListPanel()
    {
        bool isActive = !taskListPanel.activeSelf;
        taskListPanel.SetActive(isActive);
        if (isActive)
        {
            RefreshTaskList();
        }
    }


    private void SetupTechTreePanel(Transform parent)
    {
        techTreePanel = CreateUIPanel(parent, "TechTreePanel", new Vector2(400, 0), new Vector2(0, 0), new Vector2(0, 1), new Vector2(275, 0)); // Positioned next to Task List
        var vlg = techTreePanel.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(10, 10, 10, 10);
        vlg.spacing = 5;
        vlg.childControlWidth = true;

        CreateText(techTreePanel.transform, "Header", "Technology Tree", 22);

        var scrollView = new GameObject("ScrollView");
        scrollView.transform.SetParent(techTreePanel.transform, false);
        var scrollRect = scrollView.AddComponent<ScrollRect>();
        var scrollRt = scrollView.GetComponent<RectTransform>();
        scrollRt.sizeDelta = new Vector2(380, 0);

        var viewport = new GameObject("Viewport");
        viewport.transform.SetParent(scrollView.transform, false);
        viewport.AddComponent<RectMask2D>();
        var viewportImage = viewport.AddComponent<Image>();
        viewportImage.color = new Color(0.1f, 0.1f, 0.1f, 0.5f);
        scrollRect.viewport = viewport.GetComponent<RectTransform>();

        var contentGo = new GameObject("Content");
        contentGo.transform.SetParent(viewport.transform, false);
        techTreeContent = contentGo.transform;
        Debug.Log($"TechTreeContent: {techTreeContent}");
        var contentVlg = contentGo.AddComponent<VerticalLayoutGroup>();
        contentVlg.padding = new RectOffset(10, 10, 10, 10);
        contentVlg.spacing = 15;
        contentVlg.childControlWidth = true;
        var csf = contentGo.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        scrollRect.content = contentGo.GetComponent<RectTransform>();

        var viewportRt = viewport.GetComponent<RectTransform>();
        viewportRt.anchorMin = Vector2.zero;
        viewportRt.anchorMax = Vector2.one;
        viewportRt.sizeDelta = Vector2.zero;

        var contentRt = contentGo.GetComponent<RectTransform>();
        contentRt.anchorMin = new Vector2(0, 1);
        contentRt.anchorMax = new Vector2(1, 1);
        contentRt.pivot = new Vector2(0.5f, 1);
        contentRt.sizeDelta = new Vector2(0, 0);

        techTreePanel.SetActive(false);
    }

    private void ToggleTechTreePanel()
    {
        bool isActive = !techTreePanel.activeSelf;
        techTreePanel.SetActive(isActive);
        if (isActive)
        {
            RefreshTechTreePanel();
        }
    }
    private void RefreshTechTreePanel()
    {
        // Self-heal: If techTreeContent is lost, try to find it again.
        if (techTreeContent == null)
        {
            if (techTreePanel != null)
            {
                var contentTransform = techTreePanel.transform.Find("ScrollView/Viewport/Content");
                if (contentTransform != null)
                {
                    techTreeContent = contentTransform;
                }
                else
                {
                    Debug.LogError("Could not find techTreeContent transform! UI will not be refreshed.");
                    return;
                }
            }
            else
            {
                Debug.LogError("techTreePanel is null. Cannot re-acquire content. UI will not be refreshed.");
                return;
            }
        }

        for (int i = techTreeContent.childCount - 1; i >= 0; i--)
        {
            Destroy(techTreeContent.GetChild(i).gameObject);
        }

        if (GameManager.Instance == null || GameManager.Instance.AllTechnologies == null) return;

        foreach (var tech in GameManager.Instance.AllTechnologies)
        {
            var techPanel = CreateUIPanel(techTreeContent, $"Tech_{tech.TechnologyID}", new Vector2(0, 120), Vector2.zero, Vector2.one, Vector2.zero);
            var techVLG = techPanel.AddComponent<VerticalLayoutGroup>();
            techVLG.padding = new RectOffset(8, 8, 8, 8);
            techVLG.spacing = 3;
            techVLG.childControlWidth = true;

            CreateText(techPanel.transform, "Title", $"<b>{tech.DisplayName}</b>", 18).alignment = TextAlignmentOptions.Left;
            CreateText(techPanel.transform, "Description", tech.Description, 14).alignment = TextAlignmentOptions.Left;

            string reqText = "Requires: ";
            if (tech.RequiredTechnologies == null || tech.RequiredTechnologies.Count == 0)
            {
                reqText += "None";
            }
            else
            {
                reqText += string.Join(", ", tech.RequiredTechnologies.Select(reqId =>
                {
                    var requiredTech = GameManager.Instance.GetTechnologyByID(reqId);
                    return requiredTech != null ? requiredTech.DisplayName : "Unknown";
                }));
            }
            CreateText(techPanel.transform, "Requirements", reqText, 12).alignment = TextAlignmentOptions.Left;

            var researchButton = CreateButton(techPanel.transform, "Research", () => GameManager.Instance.SelectTechnologyForResearch(tech));
            var buttonText = researchButton.GetComponentInChildren<TextMeshProUGUI>();
            var buttonImage = researchButton.GetComponent<Image>();

            bool prerequisitesMet = tech.RequiredTechnologies.All(reqId => GameManager.Instance.GetTechnologyByID(reqId)?.CurrentState == Technology.State.Unlocked);

            switch (tech.CurrentState)
            {
                case Technology.State.Unlocked:
                    buttonText.text = "Unlocked";
                    researchButton.interactable = false;
                    buttonImage.color = Color.green;
                    break;
                
                case Technology.State.Researching:
                    buttonText.text = $"Researching... ({tech.CurrentResearchProgress}/{tech.ResearchPointCost})";
                    researchButton.interactable = false;
                    buttonImage.color = Color.cyan; // A color to indicate active research
                    break;

                case Technology.State.Locked:
                    buttonText.text = "Research";
                    if (prerequisitesMet && GameManager.Instance.CurrentlyResearchingTechnology == null)
                    {
                        researchButton.interactable = true;
                        buttonImage.color = Color.yellow; // Can be researched
                    }
                    else
                    {
                        researchButton.interactable = false;
                        buttonImage.color = Color.gray; // Cannot be researched
                    }
                    break;
            }
        }
    }
    
    private void RefreshTaskList()
    {
        // Self-heal: If taskListContent is lost, try to find it again.
        if (taskListContent == null)
        {
            if (taskListPanel != null)
            {
                var contentTransform = taskListPanel.transform.Find("ScrollView/Viewport/Content");
                if (contentTransform != null)
                {
                    taskListContent = contentTransform;
                }
                else
                {
                    throw new System.Exception("Could not find taskListContent transform! UI will not be refreshed.");
                }
            }
            else
            {
                throw new System.Exception("taskListPanel is null. Cannot re-acquire content. UI will not be refreshed.");
            }
        }

        // Clear existing task entries safely
        for (int i = taskListContent.childCount - 1; i >= 0; i--)
        {
            Destroy(taskListContent.GetChild(i).gameObject);
        }

        if (GameManager.Instance == null || GameManager.Instance.AvailableTasks == null)
        {
            return;
        }
        
        var sortedTasks = GameManager.Instance.AvailableTasks
            .OrderBy(t => t.CurrentStatus)
            .ThenByDescending(t => t.Priority);

        foreach (var task in sortedTasks)
        {
            string statusColor = task.CurrentStatus == Status.Executing ? "yellow" : "white";
            string assignee = task.AssignedNPC != null ? task.AssignedNPC.name : "Unassigned";
            
            string taskText = $"<b>{task.GetType().Name}</b> ({task.Priority})\n" +
                              $"<color={statusColor}>Status: {task.CurrentStatus}</color>\n" +
                              $"Assignee: {assignee}";

            var textEntry = CreateText(taskListContent, "TaskEntry", taskText, 14);
            textEntry.alignment = TextAlignmentOptions.Left;
            textEntry.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 60); // Adjust height
        }
    }


    private void SetupStatsBar(Transform parent)
    {
        statsBarUIContainer = CreateUIPanel(parent, "StatsBarUI", new Vector2(0, 40), new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -20));
        var layout = statsBarUIContainer.AddComponent<HorizontalLayoutGroup>();
        layout.padding = new RectOffset(10, 10, 5, 5);
        layout.spacing = 15;
        
        gameStateText = CreateText(statsBarUIContainer.transform, "GameStateText", "State: Initializing", 18);
        gameStateText.color = Color.cyan;

        clockText = CreateText(statsBarUIContainer.transform, "ClockText", "9:00 AM", 18);

        foreach (StatType type in Enum.GetValues(typeof(StatType)))
        {
            statTexts.Add(type, CreateText(statsBarUIContainer.transform, type.ToString(), $"{type}: 0", 18));
        }
    }

    private void SetupBuildPhaseUI(Transform parent)
    {
        buildPhaseUIContainer = CreateUIPanel(parent, "BuildPhaseUI", new Vector2(220, 180), new Vector2(0, 0), new Vector2(0, 0), new Vector2(170, 90));
        var vlg = buildPhaseUIContainer.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(10,10,10,10);
        vlg.spacing = 5;

        totalDailyCostText = CreateText(buildPhaseUIContainer.transform, "DailyCostText", "Daily Cost: $0", 16);
        totalDailyCostText.color = Color.yellow;
        
        CreateButton(buildPhaseUIContainer.transform, "Hire NPCDevOps", () => hireDevOpsPanel.SetActive(true));
        CreateButton(buildPhaseUIContainer.transform, "Start Day", () => GameManager.Instance.GameLoopManager.EndBuildPhaseAndStartPlayPhase());

        // Hire DevOps Panel (Sub-panel)
        hireDevOpsPanel = CreateUIPanel(parent, "HireDevOpsPanel", new Vector2(220, 150), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero);
        var hireVlg = hireDevOpsPanel.AddComponent<VerticalLayoutGroup>();
        hireVlg.padding = new RectOffset(10,10,10,10);
        hireVlg.spacing = 5;
        // The first button is the "< Back" button
        CreateButton(hireDevOpsPanel.transform, "< Back", () => hireDevOpsPanel.SetActive(false));
        hireDevOpsPanel.SetActive(false); // Start hidden
    }
    
    private void SetupSummaryPhaseUI(Transform parent)
    {
        summaryPhaseUIContainer = CreateUIPanel(parent, "SummaryPhaseUI", new Vector2(400, 100), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero);
        CreateText(summaryPhaseUIContainer.transform, "Summary Text", "", 24);
    }
    
    private void SetupTooltip(Transform parent)
    {
        tooltipPanel = CreateUIPanel(parent, "Tooltip", new Vector2(200, 150), new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(435, 0));
        var vlg = tooltipPanel.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(5,5,5,5);
        tooltipText = CreateText(tooltipPanel.transform, "TooltipText", "", 14);
        tooltipText.alignment = TextAlignmentOptions.Left;
        tooltipButton = CreateButton(tooltipPanel.transform, "Action", () => {{}});
        tooltipPanel.SetActive(false);
    }

    private void SetupTimeControls(Transform parent)
    {
        timeControlsContainer = CreateUIPanel(parent, "TimeControls", new Vector2(200, 50), new Vector2(1, 0), new Vector2(1, 0), new Vector2(-110, 35));
        var layout = timeControlsContainer.AddComponent<HorizontalLayoutGroup>();
        layout.padding = new RectOffset(5, 5, 5, 5);
        layout.spacing = 5;

        pauseButton = CreateButton(timeControlsContainer.transform, "||", SetTimeScalePause, new Vector2(40, 40));
        playButton = CreateButton(timeControlsContainer.transform, ">", SetTimeScalePlay, new Vector2(40, 40));
        fastForwardButton = CreateButton(timeControlsContainer.transform, ">>", SetTimeScaleFastForward, new Vector2(40, 40));
        superFastForwardButton = CreateButton(timeControlsContainer.transform, ">>>", SetTimeScaleSuperFastForward, new Vector2(40, 40));
        
        UpdateTimeScaleButtons();
    }
    #endregion

    #region Time Controls
    public void SetTimeScalePause() => SetTimeScale(0f);
    public void SetTimeScalePlay() => SetTimeScale(1f);
    public void SetTimeScaleFastForward() => SetTimeScale(2f);
    public void SetTimeScaleSuperFastForward() => SetTimeScale(8f);

    private void SetTimeScale(float scale)
    {
        Time.timeScale = scale;
        UpdateTimeScaleButtons();
    }

    private void UpdateTimeScaleButtons()
    {
        if (superFastForwardButton == null) return;
        pauseButton.GetComponent<Image>().color = Mathf.Approximately(Time.timeScale, 0f) ? activeColor : inactiveColor;
        playButton.GetComponent<Image>().color = Mathf.Approximately(Time.timeScale, 1f) ? activeColor : inactiveColor;
        fastForwardButton.GetComponent<Image>().color = Mathf.Approximately(Time.timeScale, 2f) ? activeColor : inactiveColor;
        superFastForwardButton.GetComponent<Image>().color = Mathf.Approximately(Time.timeScale, 8f) ? activeColor : inactiveColor;
    }
    #endregion

    #region UI State Management
    public void ShowBuildUI()
    {
        buildPhaseUIContainer.SetActive(true);
        summaryPhaseUIContainer.SetActive(false);
        timeControlsContainer.SetActive(false);
        hireDevOpsPanel.SetActive(false);
        RefreshHireDevOpsPanel();
        UpdateDailyCostDisplay();
    }

    public void HideBuildUI()
    {
        buildPhaseUIContainer.SetActive(false);
        timeControlsContainer.SetActive(true);
    }

    public void ShowSummaryUI(string text)
    {
        summaryPhaseUIContainer.SetActive(true);
        timeControlsContainer.SetActive(false);
        summaryPhaseUIContainer.GetComponentInChildren<TextMeshProUGUI>().text = text;
    }
    #endregion

    #region UI Content Updates
    public void UpdateGameStateDisplay(string state)
    {
        if (gameStateText != null) gameStateText.text = $"State: {state}";
    }
    
    public void UpdateClockDisplay(float timeElapsed, float dayDuration)
    {
        if (clockText == null) return;
        
        int day = GameManager.Instance.GameLoopManager.currentDay;

        float dayPercentage = Mathf.Clamp01(timeElapsed / dayDuration);
        float totalWorkdayHours = 8f;
        float elapsedHours = totalWorkdayHours * dayPercentage;
        int currentHour = 9 + (int)elapsedHours;
        int currentMinute = (int)((elapsedHours - (int)elapsedHours) * 60);
        
        string amPm = currentHour < 12 ? "AM" : "PM";
        int displayHour = currentHour > 12 ? currentHour - 12 : currentHour;
        if (displayHour == 0) displayHour = 12;

        clockText.text = $"Day: {day} | {displayHour:D2}:{currentMinute:D2} {amPm}";
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

    private void RefreshHireDevOpsPanel()
    {
        // Clear old candidates, but skip the first child which is the "Back" button
        for (int i = hireDevOpsPanel.transform.childCount - 1; i > 0; i--)
        {
            Destroy(hireDevOpsPanel.transform.GetChild(i).gameObject);
        }

        var candidates = GameManager.Instance.GenerateNPCCandidates(3);
        foreach (var candidate in candidates)
        {
            CreateButton(hireDevOpsPanel.transform, $"Hire (${candidate.DailyCost}/day)", () => {
                GameManager.Instance.HireNPCDevOps(candidate);
                hireDevOpsPanel.SetActive(false);
            });
        }
    }
    #endregion

    #region Tooltip Logic
    public void ShowInfrastructureTooltip(InfrastructureInstance instance)
    {
        bool conditionsMet = GameManager.Instance.AreUnlockConditionsMet(instance.data);
        if (instance.data.CurrentState == InfrastructureData.State.Locked && !conditionsMet)
        {
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
                    tooltipContent += $"- {condition.GetDescription()}\n";
                }
            }
        }
        tooltipContent += $"\n Daily Cost: ${instance.data.DailyCost}\nBuild Time: {instance.data.BuildTime}s";
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
                tooltipButton.onClick.AddListener(() => GameManager.Instance.PlanInfrastructure(instance.data));
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

    public void HideTooltip() => tooltipPanel.SetActive(false);
    
    #endregion

    #region UI Helper Methods
    private GameObject CreateUIPanel(Transform p, string n, Vector2 s, Vector2 min, Vector2 max, Vector2 pos)
    {
        var go = new GameObject(n); go.transform.SetParent(p, false);
        var rt = go.AddComponent<RectTransform>(); rt.sizeDelta = s; rt.anchorMin = min; rt.anchorMax = max; rt.anchoredPosition = pos;
        go.AddComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
        return go;
    }

    private Button CreateButton(Transform p, string t, UnityAction a)
    {
        var go = new GameObject($"Button_{{t}}"); go.transform.SetParent(p, false);
        go.transform.SetParent(p, false);
        var rt = go.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(180, 40);
        go.AddComponent<Image>().color = Color.gray;
        var btn = go.AddComponent<Button>();
        btn.onClick.AddListener(a);
        CreateText(btn.transform, "Text", t, 14);
        return btn;
    }

    private Button CreateButton(Transform p, string t, UnityAction a, Vector2 size)
    {
        var btn = CreateButton(p, t, a);
        btn.GetComponent<RectTransform>().sizeDelta = size;
        return btn;
    }

    private TextMeshProUGUI CreateText(Transform p, string n, string c, int s)
    {
        var go = new GameObject(n); go.transform.SetParent(p, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = c;
        tmp.fontSize = s;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        return tmp;
    }
    #endregion
}
