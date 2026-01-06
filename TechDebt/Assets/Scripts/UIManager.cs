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
using System.Linq;
using Stats;
using static NPCTask;
using Events;

public class UIManager : MonoBehaviour

{
    public enum TimeState { Paused, Normal, Fast, SuperFast }
    
    // UI Containers
    // private Canvas mainCanvas;
    private GameObject buildPhaseUIContainer;
    private GameObject summaryPhaseUIContainer;
    private GameObject statsBarUIContainer;
    private GameObject hireDevOpsPanel;
    private GameObject infrastructureDetailPanel;
    private GameObject itemDetailPanel;
    private GameObject timeControlsContainer;
    private GameObject leftMenuBar;
    private GameObject taskListPanel;
    private GameObject techTreePanel;
    private GameObject npcListPanel;
    private GameObject npcDetailPanel;
    private GameObject alertPanel;
    private GameObject eventLogPanel;
    private GameObject debugPanel;
    private GameObject eventTriggerPanel;
    
    
    // UI Elements
    private Dictionary<StatType, TextMeshProUGUI> statTexts = new Dictionary<StatType, TextMeshProUGUI>();
    private TextMeshProUGUI _infrastructureDetailText;
    private TextMeshProUGUI _eventLogText;
    private Button _planBuildButton;
    private Button _upsizeButton;
    private Button _downsizeButton;
        private TextMeshProUGUI totalDailyCostText;
        private TextMeshProUGUI gameStateText;
        private TextMeshProUGUI clockText;
    
    
    
        // Time Control Buttons & Colors
    private Button pauseButton, playButton, fastForwardButton, superFastForwardButton;
    private Color activeColor = new Color(0.5f, 0.8f, 1f); // Light blue for active button
    private Color inactiveColor = Color.gray;
    private TimeState _currentTimeState = TimeState.Normal;
    private TimeState _timeStateBeforePause = TimeState.Normal;
    
    // Task List
    private Dictionary<NPCTask, GameObject> _taskUIMap = new Dictionary<NPCTask, GameObject>();
    private Transform taskListContent;

    private float taskListUpdateCooldown = 0.5f;

    private float lastTaskListUpdateTime;

    

    // Tech Tree

    private Transform techTreeContent;

    
    private InfrastructureInstance _selectedInfrastructure;
    private Items.ItemBase _selectedItem;
    private NPCDevOps _selectedNPC;
    void OnEnable() 
    { 
        GameManager.OnStatsChanged += UpdateStatsDisplay; 
        GameManager.OnDailyCostChanged += UpdateDailyCostDisplay;
        GameManager.OnTechnologyUnlocked += RefreshTechTreePanelOnEvent;
        GameManager.OnTechnologyResearchStarted += RefreshTechTreePanelOnEvent;
        GameManager.OnCurrentEventsChanged += UpdateEventLog;
    }

    void OnDisable() 
    {
        GameManager.OnStatsChanged -= UpdateStatsDisplay; 
        GameManager.OnDailyCostChanged -= UpdateDailyCostDisplay;
        GameManager.OnTechnologyUnlocked -= RefreshTechTreePanelOnEvent;
        GameManager.OnTechnologyResearchStarted -= RefreshTechTreePanelOnEvent;
        GameManager.OnCurrentEventsChanged -= UpdateEventLog;
    }

    private void ToggleEventLogPanel()
    {
        bool wasActive = eventLogPanel.activeSelf;
        CloseAllSidebarPanels();
        if (!wasActive)
        {
            eventLogPanel.SetActive(true);
            UpdateEventLog();
        }
    }
    
    private void UpdateEventLog()
    {
        if (_eventLogText == null || !eventLogPanel.activeSelf) return;

        var currentEvents = GameManager.Instance.CurrentEvents;
        
        string log = "<b>Current Events:</b>\n";
        if (currentEvents.Count == 0)
        {
            log += "No active events.";
        }
        else
        {
            foreach (var ev in currentEvents)
            {
                log += $"- {ev.GetType().Name.Replace("Event", "")}\n";
            }
        }
        _eventLogText.text = log;
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
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            TogglePause();
        }

        if (taskListPanel != null && taskListPanel.activeSelf && Time.time - lastTaskListUpdateTime > taskListUpdateCooldown)
        {
            RefreshTaskList();
            lastTaskListUpdateTime = Time.time;
        }

        if (infrastructureDetailPanel != null && infrastructureDetailPanel.activeSelf)
        {
            UpdateInfrastructureDetailPanel();
        }
        if (npcDetailPanel != null && npcDetailPanel.activeSelf)
        {
            UpdateNPCDetailPanel();
        }
    }

    private void UpdateInfrastructureDetailPanel()
    {
        if (_selectedInfrastructure == null) return;
        
        string content = $"<b>{_selectedInfrastructure.data.DisplayName}</b>\n";
        content += $"Type: {_selectedInfrastructure.data.Type}\n";
        content += $"State: {_selectedInfrastructure.data.CurrentState}\n\n";

        content += "<b>Stats:</b>\n";
        foreach (var stat in _selectedInfrastructure.data.Stats.Stats.Values)
        {
            content += $"- {stat.Type}: {stat.Value:F2} (Base: {stat.BaseValue:F2})\n";
            if (stat.Modifiers.Count > 0)
            {
                content += "  <i>Modifiers:</i>\n";
                foreach (var mod in stat.Modifiers)
                {
                    content += $"  - {mod.Value:F2} ({mod.Type}) @ {mod.Source.GetType().Name}\n";
                }
            }
        }
        
        content += "\n<b>Connections:</b>\n";
        if (_selectedInfrastructure.CurrConnections.Count == 0)
        {
            content += "No active connections.";
        }
        else
        {
            foreach (var kvp in _selectedInfrastructure.CurrConnections)
            {
                content += $"- <b>{kvp.Key}:</b> ";
                content += string.Join(", ", kvp.Value.Select(conn => conn.TargetID));
                content += "\n";
            }
        }

        _infrastructureDetailText.text = content;
    }

    private void UpdateNPCDetailPanel()
    {
        if (_selectedNPC == null) return;

        var detailText = npcDetailPanel.GetComponentInChildren<TextMeshProUGUI>();
        if (detailText == null) return;
        
        string content = $"<b>{_selectedNPC.name}</b>\n\n";
        content += "<b>Stats:</b>\n";

        foreach (var stat in _selectedNPC.Data.Stats.Stats.Values)
        {
            content += $"- {stat.Type}: {stat.Value:F2} (Base: {stat.BaseValue:F2})\n";
            if (stat.Modifiers.Any())
            {
                content += "  <i>Modifiers:</i>\n";
                foreach (var mod in stat.Modifiers)
                {
                    string sourceName = mod.Source != null ? mod.Source.GetType().Name : "Unknown";
                    content += $"  - {mod.Value:F2} ({mod.Type}) @ {sourceName}\n";
                }
            }
        }
        detailText.text = content;
    }

    public void SetupUIInfrastructure()

    {

                if (FindObjectOfType<EventSystem>() == null)

                {

                    var esGO = new GameObject("EventSystem");

                    esGO.AddComponent<EventSystem>();

                    esGO.AddComponent<InputSystemUIInputModule>();

                }
                
                var canvas = GetComponentInParent<Canvas>();
                if (canvas != null)
                {
                    if (canvas.GetComponent<GraphicRaycaster>() == null)
                    {
                        canvas.gameObject.AddComponent<GraphicRaycaster>();
                    }
                }
                else
                {
                    Debug.LogWarning("UIManager could not find a parent Canvas. UI clicks may not work.");
                }

        

                if (Camera.main.GetComponent<PhysicsRaycaster>() == null)

                {

                    Debug.LogWarning("Main Camera is missing a PhysicsRaycaster component. Clicking on NPCs or other game objects may not work. Please add one in the Unity Editor.");

                }





        // The CanvasScaler and GraphicRaycaster should already be on the GameObject in the Editor.

        // If they need to be added programmatically, add them here to the mainCanvas.gameObject.

        

        SetupStatsBar(transform);

        SetupBuildPhaseUI(transform);

                SetupSummaryPhaseUI(transform);

                SetupInfrastructureDetailPanel(transform);

                SetupTimeControls(transform);

        SetupLeftMenuBar(transform);

        SetupAlertPanel(transform);
        SetupDebugPanel(transform);
        SetupItemDetailPanel(transform);
        
        

        // Initial state

        buildPhaseUIContainer.SetActive(false);

        summaryPhaseUIContainer.SetActive(false);

        timeControlsContainer.SetActive(false);

        UpdateStatsDisplay();

    }
    
    #region UI Setup Methods

    private void SetupEventLogPanel(Transform parent)
    {
        eventLogPanel = CreateUIPanel(parent, "EventLogPanel", new Vector2(300, 0), new Vector2(0, 0), new Vector2(0, 1), new Vector2(175, 0));
        var vlg = eventLogPanel.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(10, 10, 10, 10);
        vlg.spacing = 5;
        vlg.childControlWidth = true;

        _eventLogText = CreateText(eventLogPanel.transform, "EventLogText", "", 14);
        _eventLogText.alignment = TextAlignmentOptions.TopLeft;
        
        eventLogPanel.SetActive(false); // Start hidden
    }
    
    private void SetupAlertPanel(Transform parent)
    {
        alertPanel = CreateUIPanel(parent, "AlertPanel", new Vector2(400, 200), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero);
        var vlg = alertPanel.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(15, 15, 15, 15);
        vlg.spacing = 10;

        var eventText = CreateText(alertPanel.transform, "AlertText", "Alert Text Goes Here", 18);
        eventText.enableWordWrapping = true;
        eventText.alignment = TextAlignmentOptions.TopLeft;
        
        var okButton = CreateButton(alertPanel.transform, "OK", () => alertPanel.SetActive(false));
        var layoutElement = okButton.gameObject.AddComponent<LayoutElement>();
        layoutElement.minHeight = 40;
        layoutElement.flexibleHeight = 0; // Don't stretch button vertically

        alertPanel.SetActive(false);
    }
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
        CreateButton(leftMenuBar.transform, "NPCs", ToggleNPCListPanel, new Vector2(40, 40));
        CreateButton(leftMenuBar.transform, "Events", ToggleEventLogPanel, new Vector2(40, 40));
        // --- Setup all associated panels ---
        SetupTaskListPanel(parent);
        SetupTechTreePanel(parent);
        SetupNPCListPanel(parent);
        SetupNPCDetailPanel(parent);
        SetupEventLogPanel(parent);
    }
    
    private void SetupNPCListPanel(Transform parent)
    {
        npcListPanel = CreateUIPanel(parent, "NPCListPanel", new Vector2(300, 0), new Vector2(0, 0), new Vector2(0, 1), new Vector2(175, 0));
        var vlg = npcListPanel.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(10, 10, 10, 10);
        vlg.spacing = 5;
        vlg.childControlWidth = true;

        CreateText(npcListPanel.transform, "Header", "Hired NPCs", 20);

        // This panel will be populated at runtime
        npcListPanel.SetActive(false);
    }
    
    private Button _followButton;

    private void SetupNPCDetailPanel(Transform parent)
    {
        npcDetailPanel = CreateUIPanel(parent, "NPCDetailPanel", new Vector2(300, 400), new Vector2(0, 0), new Vector2(0, 0), new Vector2(225, 200));
        var vlg = npcDetailPanel.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(10, 10, 10, 10);
        vlg.spacing = 5;
        vlg.childForceExpandHeight = false; // Allow buttons to control their own height

        // Header container for title and close button
        var headerContainer = new GameObject("HeaderContainer");
        headerContainer.transform.SetParent(npcDetailPanel.transform, false);
        var headerLayout = headerContainer.AddComponent<HorizontalLayoutGroup>();
        headerLayout.childControlWidth = true;
        headerLayout.childForceExpandWidth = true;
        var headerRt = headerContainer.GetComponent<RectTransform>();
        headerRt.sizeDelta = new Vector2(0, 30);
        headerContainer.gameObject.AddComponent<LayoutElement>().preferredHeight = 30;
        
        CreateText(headerContainer.transform, "DetailText", "NPC Details", 14);
        var closeButton = CreateButton(headerContainer.transform, "X", HideNPCDetail, new Vector2(25, 25));
        var closeButtonLayout = closeButton.gameObject.AddComponent<LayoutElement>();
        closeButtonLayout.minWidth = 25;
        closeButtonLayout.flexibleWidth = 0;

        // Content area - flexible height to fill space
        var contentArea = new GameObject("ContentArea");
        contentArea.transform.SetParent(npcDetailPanel.transform, false);
        var contentVlg = contentArea.AddComponent<VerticalLayoutGroup>();
        contentVlg.childControlWidth = true;
        contentVlg.childForceExpandHeight = true;
        contentArea.AddComponent<LayoutElement>().flexibleHeight = 1;

        var detailText = CreateText(contentArea.transform, "NPCDetailContentText", "", 14);
        detailText.alignment = TextAlignmentOptions.TopLeft; // Align text to top-left
        detailText.gameObject.AddComponent<LayoutElement>().flexibleHeight = 1; // Make text flexible

        // Button container at the bottom
        var bottomButtonContainer = new GameObject("BottomButtonContainer");
        bottomButtonContainer.transform.SetParent(npcDetailPanel.transform, false);
        var bottomHlg = bottomButtonContainer.AddComponent<HorizontalLayoutGroup>();
        bottomHlg.spacing = 5;
        bottomHlg.childControlWidth = true;
        bottomHlg.childForceExpandWidth = true;
        bottomButtonContainer.gameObject.AddComponent<LayoutElement>().preferredHeight = 40; // Fixed height for buttons
        
        _followButton = CreateButton(bottomButtonContainer.transform, "Follow", () =>
        {
            var cameraController = FindObjectOfType<CameraController>();
            if (cameraController != null && _selectedNPC != null)
            {
                cameraController.StartFollowing(_selectedNPC.transform);
            }
        });
        
        npcDetailPanel.SetActive(false);
    }

    private void ToggleNPCListPanel()
    {
        bool wasActive = npcListPanel.activeSelf;
        CloseAllSidebarPanels();
        if (!wasActive)
        {
            npcListPanel.SetActive(true);
            RefreshNPCListPanel();
        }
    }

    private void RefreshNPCListPanel()
    {
        if (npcListPanel == null) return;

        // Clear existing NPCs, skipping the header
        for (int i = npcListPanel.transform.childCount - 1; i > 0; i--)
        {
            Destroy(npcListPanel.transform.GetChild(i).gameObject);
        }

        var npcs = FindObjectsOfType<NPCDevOps>();
        foreach (var npc in npcs)
        {
            NPCDevOps localNpc = npc; // Local copy for the closure
            CreateButton(npcListPanel.transform, npc.name, () => ShowNPCDetail(localNpc));
        }
    }
    
    public void ShowNPCDetail(NPCDevOps npc)
    {
        _selectedNPC = npc;
        npcListPanel.SetActive(false); // Close the list when detail is shown
        npcDetailPanel.SetActive(true);
        UpdateNPCDetailPanel(); // Initial update
    }

    public void HideNPCDetail()
    {
        _selectedNPC = null;
        npcDetailPanel.SetActive(false);
    }

    private void CloseAllSidebarPanels()
    {
        if (taskListPanel != null) taskListPanel.SetActive(false);
        if (techTreePanel != null) techTreePanel.SetActive(false);
        if (npcListPanel != null) npcListPanel.SetActive(false);
        if (npcDetailPanel != null) npcDetailPanel.SetActive(false);
        if (eventLogPanel != null) eventLogPanel.SetActive(false);
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
        scrollRect.horizontal = false; // Disable horizontal scrolling
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
        contentRt.sizeDelta = new Vector2(0, 0); // Width is controlled by parent, height by fitter

        taskListPanel.SetActive(false); // Start hidden
    }

    
    private void ToggleTaskListPanel()
    {
        bool wasActive = taskListPanel.activeSelf;
        CloseAllSidebarPanels();
        if (!wasActive)
        {
            taskListPanel.SetActive(true);
            RefreshTaskList();
        }
    }


    private void SetupTechTreePanel(Transform parent)
    {
        // Panel stretches from top to bottom, anchored to the left.
        techTreePanel = CreateUIPanel(parent, "TechTreePanel", new Vector2(400, 0), new Vector2(0, 0), new Vector2(0, 1), new Vector2(275, 0));
        var vlg = techTreePanel.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(10, 10, 10, 10);
        vlg.spacing = 10;
        vlg.childControlWidth = true;
        vlg.childForceExpandHeight = false;

        CreateText(techTreePanel.transform, "Header", "Technology Tree", 22).gameObject.AddComponent<LayoutElement>().preferredHeight = 25;

        // --- ScrollView Setup ---
        var scrollViewGO = new GameObject("ScrollView");
        scrollViewGO.transform.SetParent(techTreePanel.transform, false);
        var scrollRect = scrollViewGO.AddComponent<ScrollRect>();
        scrollViewGO.AddComponent<LayoutElement>().flexibleHeight = 1; // Allow scroll view to expand

        // --- Viewport Setup (child of ScrollView) ---
        var viewportGO = new GameObject("Viewport");
        viewportGO.transform.SetParent(scrollViewGO.transform, false);
        var viewportRT = viewportGO.AddComponent<RectTransform>();
        viewportRT.anchorMin = Vector2.zero;
        viewportRT.anchorMax = Vector2.one;
        viewportRT.sizeDelta = new Vector2(-20, 0); // Leave space on the right for the scrollbar
        viewportRT.pivot = new Vector2(0, 1);
        viewportGO.AddComponent<RectMask2D>();
        var viewportImg = viewportGO.AddComponent<Image>();
        viewportImg.color = new Color(0.1f, 0.1f, 0.1f, 0.5f);
        viewportImg.raycastTarget = false;
        scrollRect.viewport = viewportRT;

        // --- Content Setup (child of Viewport) ---
        var contentGO = new GameObject("Content");
        contentGO.transform.SetParent(viewportGO.transform, false);
        techTreeContent = contentGO.transform;
        var contentRT = contentGO.AddComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0, 1);
        contentRT.anchorMax = new Vector2(1, 1);
        contentRT.pivot = new Vector2(0.5f, 1);
        contentRT.sizeDelta = new Vector2(0, 0); // Width is controlled by parent, height by fitter
        var contentVLG = contentGO.AddComponent<VerticalLayoutGroup>();
        contentVLG.padding = new RectOffset(10, 10, 10, 10);
        contentVLG.spacing = 15;
        contentVLG.childControlWidth = true;
        contentVLG.childForceExpandHeight = false;
        var csf = contentGO.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        scrollRect.content = contentRT;

        // --- Scrollbar Setup (child of ScrollView) ---
        var scrollbarGO = new GameObject("ScrollbarVertical");
        scrollbarGO.transform.SetParent(scrollViewGO.transform, false);
        var sb = scrollbarGO.AddComponent<Scrollbar>();
        var sbRT = scrollbarGO.GetComponent<RectTransform>();
        sbRT.anchorMin = new Vector2(1, 0);
        sbRT.anchorMax = new Vector2(1, 1);
        sbRT.pivot = new Vector2(1, 1);
        sbRT.sizeDelta = new Vector2(20, 0);
        scrollRect.verticalScrollbar = sb;
        
        var handleGO = new GameObject("Handle");
        handleGO.transform.SetParent(scrollbarGO.transform, false);
        var handleImg = handleGO.AddComponent<Image>();
        handleImg.color = Color.grey;
        var handleRT = handleGO.GetComponent<RectTransform>();
        handleRT.sizeDelta = Vector2.zero; // Stretch to fill
        sb.handleRect = handleRT;
        sb.targetGraphic = handleImg;

        techTreePanel.SetActive(false);
    }

    public void ToggleTechTreePanel()
    {
        bool wasActive = techTreePanel.activeSelf;
        CloseAllSidebarPanels();
        if (!wasActive)
        {
            techTreePanel.SetActive(true);
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
            Technology localTech = tech; // Create a local copy for the closure
            var techPanel = CreateUIPanel(techTreeContent, $"Tech_{tech.TechnologyID}", new Vector2(380, 120), Vector2.zero, Vector2.one, Vector2.zero);
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

            var researchButton = CreateButton(techPanel.transform, "Research", () => GameManager.Instance.SelectTechnologyForResearch(localTech));
            var layoutElement = researchButton.gameObject.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = 40f;
            
            var buttonText = researchButton.GetComponentInChildren<TextMeshProUGUI>();

            // Hotfix for the tech tree button text
            buttonText.enableWordWrapping = true;
            buttonText.enableAutoSizing = false;
            buttonText.fontSize = 14;

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
            
            public void ForceRefreshTechTreePanel()
            {
                RefreshTechTreePanel();
            }
        
            private void RefreshTaskList()
            {
                if (taskListContent == null)
                {            var contentTransform = taskListPanel.transform.Find("ScrollView/Viewport/Content");
            if (contentTransform != null) taskListContent = contentTransform;
            else { Debug.LogError("Could not find taskListContent transform!"); return; }
        }

        if (GameManager.Instance?.AvailableTasks == null) return;

        var currentTasks = new HashSet<NPCTask>(GameManager.Instance.AvailableTasks.Where(t => t.CurrentStatus != Status.Completed));
        var tasksToRemove = _taskUIMap.Keys.Where(t => !currentTasks.Contains(t)).ToList();

        foreach (var task in tasksToRemove)
        {
            Destroy(_taskUIMap[task]);
            _taskUIMap.Remove(task);
        }

        var sortedTasks = GameManager.Instance.AvailableTasks
            .OrderByDescending(t => t.Priority)
            .ToList();

        for (int i = 0; i < sortedTasks.Count; i++)
        {
            var task = sortedTasks[i];
            NPCTask localTask = task; 

            GameObject taskEntryPanel;
            if (!_taskUIMap.ContainsKey(task))
            {
                taskEntryPanel = new GameObject($"TaskEntry_{task.GetHashCode()}");
                taskEntryPanel.transform.SetParent(taskListContent, false);
                var hlg = taskEntryPanel.AddComponent<HorizontalLayoutGroup>();
                hlg.spacing = 5;
                hlg.childControlWidth = true;
                hlg.childForceExpandWidth = true;
                var taskEntryLayout = taskEntryPanel.AddComponent<LayoutElement>();
                taskEntryLayout.minHeight = 80;

                var textEntry = CreateText(taskEntryPanel.transform, "TaskText", "", 14);
                var textLayoutElement = textEntry.gameObject.AddComponent<LayoutElement>();
                textLayoutElement.flexibleWidth = 1;
                textEntry.alignment = TextAlignmentOptions.Left;
                textEntry.enableAutoSizing = false;
                textEntry.enableWordWrapping = true;
                
                var buttonContainer = new GameObject("ButtonContainer");
                buttonContainer.transform.SetParent(taskEntryPanel.transform, false);
                var buttonVLG = buttonContainer.AddComponent<VerticalLayoutGroup>();
                buttonVLG.spacing = 2;
                var buttonContainerLayout = buttonContainer.AddComponent<LayoutElement>();
                buttonContainerLayout.minWidth = 45;
                buttonContainerLayout.flexibleWidth = 0;

                var upButton = CreateButton(buttonContainer.transform, "↑", () => {
                    GameManager.Instance.IncreaseTaskPriority(localTask);
                    RefreshTaskList(); 
                }, new Vector2(40, 40));
                upButton.name = "UpButton";

                var downButton = CreateButton(buttonContainer.transform, "↓", () => {
                    GameManager.Instance.DecreaseTaskPriority(localTask);
                    RefreshTaskList();
                }, new Vector2(40, 40));
                downButton.name = "DownButton";

                _taskUIMap[task] = taskEntryPanel;
            }

            taskEntryPanel = _taskUIMap[task];
            taskEntryPanel.transform.SetSiblingIndex(i);

            var textComponent = taskEntryPanel.GetComponentInChildren<TextMeshProUGUI>();
            string statusColor = task.CurrentStatus == Status.Executing ? "yellow" : "white";
            string assignee = task.AssignedNPC != null ? task.AssignedNPC.name : "Unassigned";
            string taskText = $"<b>{task.GetType().Name}</b> ({task.Priority})\n";
            if (task is BuildTask buildTask) taskText += $"Target: {buildTask.TargetInfrastructure.data.ID}\n";
            taskText += $"<color={statusColor}>Status: {task.CurrentStatus}</color> | Assignee: {assignee}";
            textComponent.text = taskText;

            var buttons = taskEntryPanel.GetComponentsInChildren<Button>();
            var upButtonComponent = buttons.FirstOrDefault(b => b.name == "UpButton");
            if (upButtonComponent != null) upButtonComponent.interactable = (i > 0);

            var downButtonComponent = buttons.FirstOrDefault(b => b.name == "DownButton");
            if (downButtonComponent != null) downButtonComponent.interactable = (i < sortedTasks.Count - 1);
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

        var statsToDisplay = new List<StatType>
        {
            StatType.Money,
            StatType.TechDebt,
            StatType.Traffic,
            StatType.PacketsSent,
            StatType.PacketsServiced,
            StatType.PacketIncome,
            StatType.Difficulty,
            StatType.PRR
        };
        
        foreach (StatType type in statsToDisplay)
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
    
    private void SetupInfrastructureDetailPanel(Transform parent)
    {
        GameObject panelGO = CreateUIPanel(parent, "InfrastructureDetailPanel", new Vector2(300, 400), new Vector2(0, 0), new Vector2(0, 0), new Vector2(225, 200));
        infrastructureDetailPanel = panelGO;
        UIPanel uiPanel = panelGO.GetComponent<UIPanel>();

        if (uiPanel == null)
        {
            Debug.LogError($"InfrastructureDetailPanel GameObject '{panelGO.name}' is missing a UIPanel component.");
            return;
        }

        uiPanel.titleText.text = "Infrastructure Details";
        uiPanel.closeButton.onClick.AddListener(HideInfrastructureDetail);

        GameObject textAreaPrefab = GameManager.Instance.prefabManager.GetPrefab("UITextArea");
        if (textAreaPrefab != null)
        {
            GameObject textAreaGO = Instantiate(textAreaPrefab, uiPanel.scrollContent);
            UITextArea uiTextArea = textAreaGO.GetComponent<UITextArea>();
            if (uiTextArea != null)
            {
                _infrastructureDetailText = uiTextArea.textArea;
                _infrastructureDetailText.alignment = TextAlignmentOptions.TopLeft;
                textAreaGO.AddComponent<LayoutElement>().flexibleHeight = 1;
            }
            else
            {
                Debug.LogError("UITextArea prefab is missing the UITextArea component.");
            }
        }
        else
        {
            Debug.LogError("UITextArea prefab not found in PrefabManager.");
            _infrastructureDetailText = CreateText(uiPanel.scrollContent, "DetailContentText", "", 14);
            _infrastructureDetailText.alignment = TextAlignmentOptions.TopLeft;
            _infrastructureDetailText.gameObject.AddComponent<LayoutElement>().flexibleHeight = 1;
        }

        _planBuildButton = uiPanel.AddButton("Plan Build", () => GameManager.Instance.PlanInfrastructure(_selectedInfrastructure)).button;
        _planBuildButton.gameObject.SetActive(false);

        _upsizeButton = uiPanel.AddButton("Upsize", () => { /* Temp action */ }).button;
        _downsizeButton = uiPanel.AddButton("Downsize", () => { /* Temp action */ }).button;

        _upsizeButton.gameObject.SetActive(false);
        _downsizeButton.gameObject.SetActive(false);

        infrastructureDetailPanel.SetActive(false);
    }
    
    private void SetupDebugPanel(Transform parent)
    {
        debugPanel = CreateUIPanel(parent, "DebugPanel", new Vector2(200, 150), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 200));
        var vlg = debugPanel.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(10,10,10,10);
        vlg.spacing = 5;

        var debugPanelComponent = debugPanel.AddComponent<DebugPanel>();
        debugPanelComponent.mouseCoordsText = CreateText(debugPanel.transform, "MouseCoordsText", "X: -, Y: -", 14);
        debugPanelComponent.instaBuildButton = CreateButton(debugPanel.transform, "Insta-Build", () => {});
        debugPanelComponent.instaResearchButton = CreateButton(debugPanel.transform, "Insta-Research", () => {});
        debugPanelComponent.unlockAllTechButton = CreateButton(debugPanel.transform, "Unlock All Tech", () => {});
        debugPanelComponent.triggerEventButton = CreateButton(debugPanel.transform, "Trigger Event", () => ToggleEventTriggerPanel());

        SetupEventTriggerPanel(parent);
        debugPanel.SetActive(false);
    }

    public void ToggleDebugPanel()
    {
        debugPanel.SetActive(!debugPanel.activeSelf);
    }

    private void SetupEventTriggerPanel(Transform parent)
    {
        eventTriggerPanel = CreateUIPanel(parent, "EventTriggerPanel", new Vector2(250, 400), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(200, 0));
        var vlg = eventTriggerPanel.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(10,10,10,10);
        vlg.spacing = 5;

        CreateText(eventTriggerPanel.transform, "Header", "Trigger Event", 18);
        
        // --- ScrollView Setup ---
        var scrollViewGO = new GameObject("ScrollView");
        scrollViewGO.transform.SetParent(eventTriggerPanel.transform, false);
        var scrollRect = scrollViewGO.AddComponent<ScrollRect>();
        scrollViewGO.AddComponent<LayoutElement>().flexibleHeight = 4; // 4 parts of the space

        var viewportGO = new GameObject("Viewport");
        viewportGO.transform.SetParent(scrollViewGO.transform, false);
        var viewportRT = viewportGO.AddComponent<RectTransform>();
        viewportRT.anchorMin = Vector2.zero;
        viewportRT.anchorMax = Vector2.one;
        viewportRT.sizeDelta = Vector2.zero;
        viewportRT.pivot = new Vector2(0, 1);
        viewportGO.AddComponent<RectMask2D>();
        scrollRect.viewport = viewportRT;

        var contentGO = new GameObject("Content");
        contentGO.transform.SetParent(viewportGO.transform, false);
        var contentRT = contentGO.AddComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0, 1);
        contentRT.anchorMax = new Vector2(1, 1);
        contentRT.pivot = new Vector2(0.5f, 1);
        contentRT.sizeDelta = new Vector2(0, 0);
        var contentVLG = contentGO.AddComponent<VerticalLayoutGroup>();
        contentVLG.padding = new RectOffset(5,5,5,5);
        contentVLG.spacing = 5;
        contentVLG.childControlWidth = true;
        var csf = contentGO.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        scrollRect.content = contentRT;


        if (GameManager.Instance != null && GameManager.Instance.Events != null)
        {
            foreach (var gameEvent in GameManager.Instance.Events)
            {
                EventBase localEvent = gameEvent; // Local copy for the closure
                var button = CreateButton(contentGO.transform, gameEvent.GetType().Name, () => {
                    GameManager.Instance.TriggerEvent(localEvent);
                    eventTriggerPanel.SetActive(false); // Close panel after triggering
                });
                button.gameObject.AddComponent<LayoutElement>().minHeight = 30;
            }
        }
        
        var backButton = CreateButton(eventTriggerPanel.transform, "< Back", () => eventTriggerPanel.SetActive(false));
        backButton.gameObject.AddComponent<LayoutElement>().flexibleHeight = 1; // 1 part of the space


        eventTriggerPanel.SetActive(false);
    }

    private void ToggleEventTriggerPanel()
    {
        eventTriggerPanel.SetActive(!eventTriggerPanel.activeSelf);
    }
    #endregion
    
    #region Tooltip Logic
    public void ShowInfrastructureDetail(InfrastructureInstance instance)
    {
        _selectedInfrastructure = instance;
        infrastructureDetailPanel.SetActive(true);
        // Update text in Update() to keep it live

        bool conditionsMet = GameManager.Instance.AreUnlockConditionsMet(_selectedInfrastructure.data);

        if (_selectedInfrastructure.data.CurrentState == InfrastructureData.State.Unlocked && conditionsMet)
        {
            _planBuildButton.gameObject.SetActive(true);
            _planBuildButton.onClick.RemoveAllListeners();
            _planBuildButton.onClick.AddListener(() => GameManager.Instance.PlanInfrastructure(_selectedInfrastructure));
        }
        else
        {
            _planBuildButton.gameObject.SetActive(false);
        }

        if (_selectedInfrastructure.data.CurrentState == InfrastructureData.State.Operational)
        {
            _upsizeButton.gameObject.SetActive(true);
            _downsizeButton.gameObject.SetActive(true);

            // Update listeners to pass integer direction
            _upsizeButton.onClick.RemoveAllListeners();
            _upsizeButton.onClick.AddListener(() => GameManager.Instance.RequestInfrastructureResize(_selectedInfrastructure, 1));
            _downsizeButton.onClick.RemoveAllListeners();
            _downsizeButton.onClick.AddListener(() => GameManager.Instance.RequestInfrastructureResize(_selectedInfrastructure, -1));

            // Set interactable state based on size level
            _upsizeButton.interactable = _selectedInfrastructure.CurrentSizeLevel < 4;
            _downsizeButton.interactable = _selectedInfrastructure.CurrentSizeLevel > 0;
        }
        else
        {
            _upsizeButton.gameObject.SetActive(false);
            _downsizeButton.gameObject.SetActive(false);
        }
    }

    public void HideInfrastructureDetail()
    {
        _selectedInfrastructure = null;
        infrastructureDetailPanel.SetActive(false);
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
    public void SetTimeScalePause() => TogglePause();
    public void SetTimeScalePlay() => SetTimeState(TimeState.Normal);
    public void SetTimeScaleFastForward() => SetTimeState(TimeState.Fast);
    public void SetTimeScaleSuperFastForward() => SetTimeState(TimeState.SuperFast);

    private void SetTimeState(TimeState newState)
    {
        _currentTimeState = newState;

        float newTimeScale = 1f;
        switch (newState)
        {
            case TimeState.Paused:
                newTimeScale = 0f;
                break;
            case TimeState.Normal:
                newTimeScale = 1f;
                break;
            case TimeState.Fast:
                newTimeScale = 2f;
                break;
            case TimeState.SuperFast:
                newTimeScale = 8f;
                break;
        }

        if (newState != TimeState.Paused)
        {
            _timeStateBeforePause = newState;
        }
        
        GameManager.Instance.SetDesiredTimeScale(newTimeScale);
        UpdateTimeScaleButtons();
    }

    private void TogglePause()
    {
        if (_currentTimeState == TimeState.Paused)
        {
            SetTimeState(_timeStateBeforePause);
        }
        else
        {
            _timeStateBeforePause = _currentTimeState;
            SetTimeState(TimeState.Paused);
        }
    }

    private void UpdateTimeScaleButtons()
    {
        if (superFastForwardButton == null) return;
        pauseButton.GetComponent<Image>().color = _currentTimeState == TimeState.Paused ? activeColor : inactiveColor;
        playButton.GetComponent<Image>().color = _currentTimeState == TimeState.Normal ? activeColor : inactiveColor;
        fastForwardButton.GetComponent<Image>().color = _currentTimeState == TimeState.Fast ? activeColor : inactiveColor;
        superFastForwardButton.GetComponent<Image>().color = _currentTimeState == TimeState.SuperFast ? activeColor : inactiveColor;
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

    public void ShowAlert(string alertText)
    {
        alertPanel.SetActive(true);
        alertPanel.GetComponentInChildren<TextMeshProUGUI>().text = alertText;
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
        if (totalDailyCostText != null)
        {
            totalDailyCostText.text = $"Total Daily Cost: ${totalCost}";
        }
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
            CreateButton(hireDevOpsPanel.transform, $"Hire (${candidate.Stats.GetStatValue(StatType.NPC_DailyCost)}/day)", () => {
                GameManager.Instance.HireNPCDevOps(candidate);
                hireDevOpsPanel.SetActive(false);
            });
        }
    }
    #endregion



    #region UI Helper Methods
    private GameObject CreateUIPanel(Transform p, string n, Vector2 s, Vector2 min, Vector2 max, Vector2 pos)
    {
     

        GameObject uiPanelPrefab = GameManager.Instance.prefabManager.GetPrefab("UIPanel");

        if (uiPanelPrefab == null)
        {
            Debug.LogError($"UIPanel prefab with name 'UIPanel' not found in PrefabManager. Cannot create UI Panel.");
            return new GameObject(n); // Return an empty GameObject to prevent null reference errors
        }

        var go = Instantiate(uiPanelPrefab, p);
        go.name = n;
        var rt = go.GetComponent<RectTransform>();
        if (rt == null)
        {
            Debug.LogError($"Instantiated UIPanel prefab '{uiPanelPrefab.name}' is missing a RectTransform component. Cannot create UI Panel.");
            return go; // Return the instantiated object, but it might not behave as expected
        }

        rt.sizeDelta = s; 
        rt.anchorMin = min; 
        rt.anchorMax = max; 
        rt.anchoredPosition = pos;
        // The Image component (or any other background graphic) is expected to be part of the prefab

        // Ensure scrollContent has VerticalLayoutGroup and set spacing
        var uiPanel = go.GetComponent<UIPanel>();
        if (uiPanel != null && uiPanel.scrollContent != null)
        {
            var vlg = uiPanel.scrollContent.GetComponent<VerticalLayoutGroup>();
            if (vlg == null)
            {
                vlg = uiPanel.scrollContent.gameObject.AddComponent<VerticalLayoutGroup>();
            }
            vlg.spacing = 5; // Add 5 units of spacing between child elements
            vlg.childControlWidth = true; // Ensure children control their own width
            vlg.childForceExpandHeight = false; // Allow children to control their own height
            
            // Also ensure ContentSizeFitter is present on scrollContent for dynamic height adjustment
            var csf = uiPanel.scrollContent.GetComponent<ContentSizeFitter>();
            if (csf == null)
            {
                csf = uiPanel.scrollContent.gameObject.AddComponent<ContentSizeFitter>();
            }
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }
        
        return go;
    }

    private Button CreateButton(Transform p, string t, UnityAction a)
    {
        var go = new GameObject($"Button_{{t}}");
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
        var go = new GameObject(n);
        go.transform.SetParent(p, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.raycastTarget = false;
        tmp.text = c;
        tmp.fontSize = s;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.enableWordWrapping = false;
        tmp.enableAutoSizing = true;
        tmp.fontSizeMin = 8;
        tmp.fontSizeMax = s;

        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;
        rt.anchoredPosition = Vector2.zero;
        return tmp;
    }
    #endregion

    #region Item Detail Panel
    private void SetupItemDetailPanel(Transform parent)
    {
        itemDetailPanel = CreateUIPanel(parent, "ItemDetailPanel", new Vector2(250, 150), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero);
        var vlg = itemDetailPanel.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(15, 15, 15, 15);
        vlg.spacing = 10;
        vlg.childAlignment = TextAnchor.MiddleCenter;

        CreateText(itemDetailPanel.transform, "Header", "Item Found!", 20);

        // This button's text and action will be set dynamically
        var useButton = CreateButton(itemDetailPanel.transform, "Use", () => { });
        useButton.name = "UseItemButton";

        var cancelButton = CreateButton(itemDetailPanel.transform, "Cancel", HideItemDetail);

        itemDetailPanel.SetActive(false);
    }

    public void ShowItemDetail(Items.ItemBase item)
    {
        _selectedItem = item;
        SetTimeState(TimeState.Paused); // Pause the game
        itemDetailPanel.SetActive(true);

        var useButton = itemDetailPanel.transform.Find("UseItemButton").GetComponent<Button>();
        var buttonText = useButton.GetComponentInChildren<TextMeshProUGUI>();

        buttonText.text = item.UseVerb();
        useButton.onClick.RemoveAllListeners();
        useButton.onClick.AddListener(() => {
            GameManager.Instance.CreateUseItemTask(_selectedItem);
            HideItemDetail();
        });
    }

    public void HideItemDetail()
    {
        _selectedItem = null;
        itemDetailPanel.SetActive(false);
        // This will resume to whatever the state was before pausing
        SetTimeState(_timeStateBeforePause); 
    }
    #endregion
}

