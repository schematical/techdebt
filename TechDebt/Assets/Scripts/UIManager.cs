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
using UI;

public class UIManager : MonoBehaviour

{
    public enum TimeState
    {
        Paused,
        Normal,
        Fast,
        SuperFast
    }

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
    private UITextArea summaryPhaseText;
    private NPCDialogPanel _currentNPCDialogPanel;
    public UIMultiSelectPanel MultiSelectPanel;

    // UI Elements
    private Dictionary<StatType, TextMeshProUGUI> statTexts = new Dictionary<StatType, TextMeshProUGUI>();
    private TextMeshProUGUI _infrastructureDetailText;
    private TextMeshProUGUI _eventLogText;
    private TextMeshProUGUI _npcDetailText;
    private UITextArea _itemDetailDescriptionText;
    private TextMeshProUGUI _alertText;
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
    private List<TechTreeItemUI> _techTreeItems = new List<TechTreeItemUI>();

    // Tech Tree
    private Transform techTreeContent;

    private InfrastructureInstance _selectedInfrastructure;
    private Items.ItemBase _selectedItem;
    private NPCDevOps _selectedNPC;

    private List<Button> _taskButtons = new List<Button>();
    private bool _isInitialized = false;

    void Start()
    {
        Initialize();
    }

    public void Initialize()
    {
        if (_isInitialized) return;
        _isInitialized = true;

        SetupTimeControls(transform);
        SetupStatsBar(transform);
        SetupLeftMenuBar(transform);

        SetupEventLogPanel(transform);
        SetupNPCListPanel(transform);
        SetupNPCDetailPanel(transform);
        SetupTaskListPanel(transform);
        SetupTechTreePanel(transform);
        SetupBuildPhaseUI(transform);
        SetupInfrastructureDetailPanel(transform);
        SetupAlertPanel(transform);
        SetupItemDetailPanel(transform);
        SetupDebugPanel(transform);
        SetupEventTriggerPanel(transform);
        SetupNPCDialogPanel(transform);
        SetupSummaryPhaseUI(transform); // This was missing from Start()
        MultiSelectPanel.gameObject.SetActive(false);
        // Initial state
        UpdateTimeControlsUI();
        GameManager.Instance.Stats.Stats[StatType.Money].OnStatChanged +=
            (value) => UpdateStatText(StatType.Money, value);

        // Hide panels that shouldn't be visible at start
        if (techTreePanel != null) techTreePanel.SetActive(false);
        if (npcListPanel != null) npcListPanel.SetActive(false);
        if (npcDetailPanel != null) npcDetailPanel.SetActive(false);
        if (taskListPanel != null) taskListPanel.SetActive(false);
        if (infrastructureDetailPanel != null) infrastructureDetailPanel.SetActive(false);
        if (buildPhaseUIContainer != null) buildPhaseUIContainer.SetActive(false);
        if (summaryPhaseUIContainer != null) summaryPhaseUIContainer.SetActive(false);
        if (timeControlsContainer != null) timeControlsContainer.SetActive(false);
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
            Debug.LogWarning(
                "Main Camera is missing a PhysicsRaycaster component. Clicking on NPCs or other game objects may not work. Please add one in the Unity Editor.");
        }

        // Initialize the UI
        Initialize();

        // Update any displays that need it after initialization
        UpdateStatsDisplay();
    }

    public void UpdateStatText(StatType statType, float value)
    {
        if (statTexts.ContainsKey(statType))
        {
            statTexts[statType].text = $"{statType}: {value:F2}";
        }
    }

    private void UpdateTimeControlsUI()
    {
        if (superFastForwardButton == null) return;
        pauseButton.GetComponent<Image>().color = _currentTimeState == TimeState.Paused ? activeColor : inactiveColor;
        playButton.GetComponent<Image>().color = _currentTimeState == TimeState.Normal ? activeColor : inactiveColor;
        fastForwardButton.GetComponent<Image>().color =
            _currentTimeState == TimeState.Fast ? activeColor : inactiveColor;
        superFastForwardButton.GetComponent<Image>().color =
            _currentTimeState == TimeState.SuperFast ? activeColor : inactiveColor;
    }

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
                log += $"- {ev.GetEventDescription()}\n";
            }
        }

        _eventLogText.text = log;
    }

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

        if (taskListPanel != null && taskListPanel.activeSelf &&
            Time.time - lastTaskListUpdateTime > taskListUpdateCooldown)
        {
            RefreshTaskList();
            lastTaskListUpdateTime = Time.time;
        }

        if (techTreePanel != null && techTreePanel.activeSelf)
        {
            foreach (var item in _techTreeItems)
            {
                item.UpdateProgress();
            }
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

        if (_npcDetailText == null)
        {
            Debug.LogError("_npcDetailText is not assigned. Cannot update NPC Detail Panel.");
            return;
        }

        string content = $"<b>{_selectedNPC.name}</b>\n";
        content += $"Level: {_selectedNPC.level}\n";
        content += $"XP: {_selectedNPC.currentXP:F0}\n\n";
        
        content += "<b>Traits:</b>\n";
        if (_selectedNPC.Traits.Any())
        {
            foreach (var trait in _selectedNPC.Traits)
            {
                content += $"- {trait.Name} - Lvl: {trait.Level}\n";
            }
        }
        else
        {
            content += "No traits yet.\n";
        }
        
        content += "\n<b>Stats:</b>\n";

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

        _npcDetailText.text = content;
    }

    #region UI Setup Methods

    private void SetupEventLogPanel(Transform parent)
    {
        eventLogPanel = CreateUIPanel(parent, "EventLogPanel", new Vector2(300, 0), new Vector2(0, 0),
            new Vector2(0, 1), new Vector2(250, 0));
        UIPanel uiPanel = eventLogPanel.GetComponent<UIPanel>();
        if (uiPanel == null)
        {
            Debug.LogError("EventLogPanel is missing UIPanel component.");
            return;
        }

        uiPanel.titleText.text = "Event Log";

        GameObject textAreaPrefab = GameManager.Instance.prefabManager.GetPrefab("UITextArea");
        if (textAreaPrefab != null)
        {
            GameObject textAreaGO = Instantiate(textAreaPrefab, uiPanel.scrollContent);
            _eventLogText = textAreaGO.GetComponent<UITextArea>().textArea;
            _eventLogText.alignment = TextAlignmentOptions.TopLeft;
        }
        else
        {
            Debug.LogError("UITextArea prefab not found. Falling back to CreateText for EventLogPanel.");
            _eventLogText = CreateText(uiPanel.scrollContent, "EventLogText", "", 14);
            _eventLogText.alignment = TextAlignmentOptions.TopLeft;
        }

        eventLogPanel.SetActive(false); // Start hidden
    }

    private void SetupAlertPanel(Transform parent)
    {
        alertPanel = CreateUIPanel(parent, "AlertPanel", new Vector2(400, 200), new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f), Vector2.zero);
        UIPanel uiPanel = alertPanel.GetComponent<UIPanel>();
        if (uiPanel == null)
        {
            Debug.LogError("AlertPanel is missing UIPanel component.");
            return;
        }

        uiPanel.titleText.text = "Alert";
        if (uiPanel.closeButton != null)
        {
            uiPanel.closeButton.gameObject.SetActive(false); // Use OK button for modal dialogs
        }

        GameObject textAreaPrefab = GameManager.Instance.prefabManager.GetPrefab("UITextArea");
        if (textAreaPrefab != null)
        {
            GameObject textAreaGO = Instantiate(textAreaPrefab, uiPanel.scrollContent);
            UITextArea uiTextArea = textAreaGO.GetComponent<UITextArea>();
            _alertText = uiTextArea.textArea;
            _alertText.enableWordWrapping = true;
            _alertText.alignment = TextAlignmentOptions.TopLeft;
            _alertText.fontSize = 18;
        }
        else
        {
            Debug.LogError("UITextArea prefab not found for AlertPanel. Falling back to CreateText.");
            _alertText = CreateText(uiPanel.scrollContent, "AlertText", "Alert Text Goes Here", 18);
            _alertText.enableWordWrapping = true;
            _alertText.alignment = TextAlignmentOptions.TopLeft;
        }

        var okButton = uiPanel.AddButton("OK", () => alertPanel.SetActive(false));
        var layoutElement = okButton.gameObject.AddComponent<LayoutElement>();
        layoutElement.minHeight = 40;

        alertPanel.SetActive(false);
    }

    private void SetupLeftMenuBar(Transform parent)
    {
        leftMenuBar = CreateUIPanel(parent, "LeftMenuBar", new Vector2(100, 0), new Vector2(0, 0), new Vector2(0, 1),
            new Vector2(50, 0));
        UIPanel uiPanel = leftMenuBar.GetComponent<UIPanel>();
        if (uiPanel == null)
        {
            Debug.LogError("LeftMenuBar is missing UIPanel component.");
            return;
        }

        uiPanel.titleText.text = "";
        if (uiPanel.closeButton != null)
            uiPanel.closeButton.gameObject.SetActive(false); // No close button for main menu

        uiPanel.AddButton("Tasks", ToggleTaskListPanel).gameObject.AddComponent<LayoutElement>().preferredHeight = 40;
        uiPanel.AddButton("Tech", ToggleTechTreePanel).gameObject.AddComponent<LayoutElement>().preferredHeight = 40;
        uiPanel.AddButton("NPCs", ToggleNPCListPanel).gameObject.AddComponent<LayoutElement>().preferredHeight = 40;
        uiPanel.AddButton("Events", ToggleEventLogPanel).gameObject.AddComponent<LayoutElement>().preferredHeight = 40;

        // --- Setup all associated panels ---
        SetupTaskListPanel(parent);
        SetupTechTreePanel(parent);
        SetupNPCListPanel(parent);
        SetupNPCDetailPanel(parent);
        SetupEventLogPanel(parent);
    }

    private void SetupNPCListPanel(Transform parent)
    {
        npcListPanel = CreateUIPanel(parent, "NPCListPanel", new Vector2(300, 0), new Vector2(0, 0), new Vector2(0, 1),
            new Vector2(250, 0));
        UIPanel uiPanel = npcListPanel.GetComponent<UIPanel>();
        if (uiPanel != null)
        {
            uiPanel.titleText.text = "Hired NPCs";
        }
        else
        {
            Debug.LogError("NPCListPanel is missing UIPanel component. Title will not be set.");
            CreateText(npcListPanel.transform, "Header", "Hired NPCs", 20);
        }

        // This panel will be populated at runtime
        npcListPanel.SetActive(false);
    }

    private Button _followButton;

    private void SetupNPCDetailPanel(Transform parent)
    {
      
        npcDetailPanel = CreateUIPanel(parent, "NPCDetailPanel", new Vector2(300, 400), new Vector2(0, 0),
            new Vector2(0, 0), new Vector2(250, 200));
        UIPanel uiPanel = npcDetailPanel.GetComponent<UIPanel>();
        if (uiPanel == null)
        {
            Debug.LogError("NPCDetailPanel is missing UIPanel component.");
            return;
        }

        uiPanel.titleText.text = "NPC Details";
        uiPanel.closeButton.onClick.AddListener(HideNPCDetail);

        GameObject textAreaPrefab = GameManager.Instance.prefabManager.GetPrefab("UITextArea");
        if (textAreaPrefab != null)
        {
            GameObject textAreaGO = Instantiate(textAreaPrefab, uiPanel.scrollContent);
            _npcDetailText = textAreaGO.GetComponent<UITextArea>().textArea;
            _npcDetailText.alignment = TextAlignmentOptions.TopLeft;
        }
        else
        {
            Debug.LogError("UITextArea prefab not found. Falling back to CreateText for NPCDetailPanel.");
            _npcDetailText = CreateText(uiPanel.scrollContent, "NPCDetailContentText", "", 14);
            _npcDetailText.alignment = TextAlignmentOptions.TopLeft;
        }

        _followButton = uiPanel.AddButton("Follow", () =>
        {
            var cameraController = FindObjectOfType<CameraController>();
            if (cameraController != null && _selectedNPC != null)
            {
                cameraController.StartFollowing(_selectedNPC.transform);
            }
        }).button;

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

        var npcs = FindObjectsOfType<NPCDevOps>();
        UIPanel uiPanel = npcListPanel.GetComponent<UIPanel>();
        if (uiPanel == null)
        {
            Debug.LogError("NPCListPanel is missing UIPanel component. Cannot refresh list.");
            // Fallback to old method if UIPanel is missing
            for (int i = npcListPanel.transform.childCount - 1; i > 0; i--)
            {
                Destroy(npcListPanel.transform.GetChild(i).gameObject);
            }

            foreach (var npc in npcs)
            {
                NPCDevOps localNpc = npc;
                CreateButton(npcListPanel.transform, npc.name, () => ShowNPCDetail(localNpc));
            }

            return;
        }

        // Clear existing NPCs
        foreach (Transform child in uiPanel.scrollContent)
        {
            Destroy(child.gameObject);
        }

        foreach (var npc in npcs)
        {
            NPCDevOps localNpc = npc; // Local copy for the closure
            uiPanel.AddButton(npc.name, () => ShowNPCDetail(localNpc));
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
        taskListPanel = CreateUIPanel(parent, "TaskListPanel", new Vector2(300, 0), new Vector2(0, 0),
            new Vector2(0, 1), new Vector2(250, 0));
        UIPanel uiPanel = taskListPanel.GetComponent<UIPanel>();

        if (uiPanel != null)
        {
            uiPanel.titleText.text = "Available Tasks";
            taskListContent = uiPanel.scrollContent;
        }
        else
        {
            Debug.LogError("TaskListPanel is missing UIPanel component. Panel may not function correctly.");
            // Fallback for content to not be null
            var contentGO = new GameObject("Content");
            contentGO.transform.SetParent(taskListPanel.transform, false);
            taskListContent = contentGO.transform;
        }

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
        techTreePanel = CreateUIPanel(parent, "TechTreePanel", new Vector2(400, 0), new Vector2(0, 0),
            new Vector2(0, 1), new Vector2(300, 0));
        UIPanel uiPanel = techTreePanel.GetComponent<UIPanel>();
        if (uiPanel == null)
        {
            Debug.LogError("TechTreePanel is missing UIPanel component.");
            return;
        }

        uiPanel.titleText.text = "Technology Tree";
        techTreeContent = uiPanel.scrollContent; // The content area is already set up in the prefab

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
        if (techTreeContent == null)
        {
            Debug.LogError("techTreeContent is null. UI will not be refreshed.");
            return;
        }

        for (int i = techTreeContent.childCount - 1; i >= 0; i--)
        {
            Destroy(techTreeContent.GetChild(i).gameObject);
        }

        _techTreeItems.Clear();

        if (GameManager.Instance == null || GameManager.Instance.AllTechnologies == null) return;

        GameObject textAreaPrefab = GameManager.Instance.prefabManager.GetPrefab("UITextArea");
        if (textAreaPrefab == null)
        {
            Debug.LogError("UITextArea prefab not found. Cannot create tech panel content.");
            return;
        }

        foreach (var tech in GameManager.Instance.AllTechnologies)
        {
            Technology localTech = tech; // Create a local copy for the closure

            var techPanelGO = CreateUIPanel(techTreeContent, $"Tech_{tech.TechnologyID}", new Vector2(0, 150),
                Vector2.zero, Vector2.one, Vector2.zero);
            UIPanel techPanel = techPanelGO.GetComponent<UIPanel>();
            var techItemUI = techPanelGO.AddComponent<TechTreeItemUI>();

            if (techPanel.closeButton != null) techPanel.closeButton.gameObject.SetActive(false);

            var descriptionGO = Instantiate(textAreaPrefab, techPanel.scrollContent);
            var requirementsGO = Instantiate(textAreaPrefab, techPanel.scrollContent);

            techItemUI.titleText = techPanel.titleText;
            techItemUI.descriptionText = descriptionGO.GetComponent<UITextArea>().textArea;
            techItemUI.requirementsText = requirementsGO.GetComponent<UITextArea>().textArea;
            techItemUI.researchButton =
                techPanel.AddButton("Research", () => { }); // Action is now handled in TechTreeItemUI

            techItemUI.Initialize(localTech);
            _techTreeItems.Add(techItemUI);
        }
    }

    public void ForceRefreshTechTreePanel()
    {
        RefreshTechTreePanel();
    }

    private void RefreshTaskList()
    {
        if (taskListContent == null)
        {
            var contentTransform = taskListPanel.transform.Find("ScrollView/Viewport/Content");
            if (contentTransform != null) taskListContent = contentTransform;
            else
            {
                Debug.LogError("Could not find taskListContent transform!");
                return;
            }
        }

        if (GameManager.Instance?.AvailableTasks == null) return;

        var currentTasks =
            new HashSet<NPCTask>(GameManager.Instance.AvailableTasks.Where(t => t.CurrentState != State.Completed));
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

                var upButton = CreateButton(buttonContainer.transform, "↑", () =>
                {
                    GameManager.Instance.IncreaseTaskPriority(localTask);
                    RefreshTaskList();
                }, new Vector2(40, 40));
                upButton.name = "UpButton";

                var downButton = CreateButton(buttonContainer.transform, "↓", () =>
                {
                    GameManager.Instance.DecreaseTaskPriority(localTask);
                    RefreshTaskList();
                }, new Vector2(40, 40));
                downButton.name = "DownButton";

                _taskUIMap[task] = taskEntryPanel;
            }

            taskEntryPanel = _taskUIMap[task];
            taskEntryPanel.transform.SetSiblingIndex(i);

            var textComponent = taskEntryPanel.GetComponentInChildren<TextMeshProUGUI>();
            string statusColor = task.CurrentState == State.Executing ? "yellow" : "white";
            string assignee = task.AssignedNPC != null ? task.AssignedNPC.name : "Unassigned";
            string taskText = $"<b>{task.GetType().Name}</b> ({task.Priority})\n";
            if (task is BuildTask buildTask) taskText += $"Target: {buildTask.TargetInfrastructure.data.ID}\n";
            taskText += $"<color={statusColor}>Status: {task.CurrentState}</color> | Assignee: {assignee}";
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
        statTexts.Clear();
        statsBarUIContainer = CreateUIPanel(parent, "StatsBarUI", new Vector2(-100, 40), new Vector2(0, 1),
            new Vector2(1, 1), new Vector2(0, -20));

        UIPanel uiPanel = statsBarUIContainer.GetComponent<UIPanel>();
        if (uiPanel != null)
        {
            uiPanel.titleText.text = "";
            if (uiPanel.closeButton != null) uiPanel.closeButton.gameObject.SetActive(false);
        }

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
            StatType.DailyIncome,
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
        buildPhaseUIContainer = CreateUIPanel(parent, "BuildPhaseUI", new Vector2(220, 180), new Vector2(0, 0),
            new Vector2(0, 0), new Vector2(210, 90));
        UIPanel buildPanel = buildPhaseUIContainer.GetComponent<UIPanel>();
        if (buildPanel == null)
        {
            Debug.LogError("BuildPhaseUIContainer is missing UIPanel component.");
            return;
        }

        buildPanel.titleText.text = "Build Phase";

        GameObject textAreaPrefab = GameManager.Instance.prefabManager.GetPrefab("UITextArea");
        if (textAreaPrefab != null)
        {
            GameObject textAreaGO = Instantiate(textAreaPrefab, buildPanel.scrollContent);
            UITextArea uiTextArea = textAreaGO.GetComponent<UITextArea>();
            totalDailyCostText = uiTextArea.textArea;
            totalDailyCostText.text = "Daily Cost: $0";
            totalDailyCostText.color = Color.yellow;
            totalDailyCostText.fontSize = 16;
        }
        else
        {
            Debug.LogError("UITextArea prefab not found. Falling back to CreateText.");
            totalDailyCostText = CreateText(buildPanel.scrollContent, "DailyCostText", "Daily Cost: $0", 16);
            totalDailyCostText.color = Color.yellow;
        }

        buildPanel.AddButton("Hire NPCDevOps", () => hireDevOpsPanel.SetActive(true));
        buildPanel.AddButton("Start Day", () =>
        {
            GameManager.Instance.GameLoopManager.EndBuildPhaseAndStartPlayPhase();
            buildPanel.gameObject.SetActive(false);
        });

        // Hire DevOps Panel (Sub-panel)
        hireDevOpsPanel = CreateUIPanel(parent, "HireDevOpsPanel", new Vector2(220, 150), new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f), Vector2.zero);
        UIPanel hirePanel = hireDevOpsPanel.GetComponent<UIPanel>();
        if (hirePanel != null)
        {
            hirePanel.titleText.text = "Hire DevOps";

            // You might want to move this button to the top or bottom later by adjusting its sibling index
        }

        hireDevOpsPanel.SetActive(false); // Start hidden
    }

    private void SetupSummaryPhaseUI(Transform parent)
    {
        summaryPhaseUIContainer = CreateUIPanel(parent, "SummaryPhaseUI", new Vector2(400, 300),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero);
        CreateText(summaryPhaseUIContainer.transform, "Summary Text", "", 24);

        GameObject textPrefab = GameManager.Instance.prefabManager.GetPrefab("UITextArea");
        UIPanel uiPanel = summaryPhaseUIContainer.GetComponent<UIPanel>();
        summaryPhaseText = Instantiate(textPrefab, uiPanel.scrollContent).GetComponent<UITextArea>();

        uiPanel.AddButton("Continue", () =>
        {
            GameManager.Instance.GameLoopManager.ForceBeginBuildPhase();
            summaryPhaseUIContainer.SetActive(false);
        });
    }

    private void SetupInfrastructureDetailPanel(Transform parent)
    {
        GameObject panelGO = CreateUIPanel(parent, "InfrastructureDetailPanel", new Vector2(300, 400),
            new Vector2(0, 0), new Vector2(0, 0), new Vector2(250, 200));
        infrastructureDetailPanel = panelGO;
        UIPanel uiPanel = panelGO.GetComponent<UIPanel>();

        if (uiPanel == null)
        {
            Debug.LogError($"InfrastructureDetailPanel GameObject '{{panelGO.name}}' is missing a UIPanel component.");
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

        _planBuildButton = uiPanel
            .AddButton("Plan Build", () => GameManager.Instance.PlanInfrastructure(_selectedInfrastructure)).button;
        _planBuildButton.gameObject.SetActive(false);

        _upsizeButton = uiPanel.AddButton("Upsize", () =>
        {
            /* Temp action */
        }).button;
        _downsizeButton = uiPanel.AddButton("Downsize", () =>
        {
            /* Temp action */
        }).button;

        _upsizeButton.gameObject.SetActive(false);
        _downsizeButton.gameObject.SetActive(false);

        infrastructureDetailPanel.SetActive(false);
    }

    private void SetupDebugPanel(Transform parent)
    {
        debugPanel = CreateUIPanel(parent, "DebugPanel", new Vector2(300, 500), new Vector2(0.5f, 1),
            new Vector2(0.5f, 1), new Vector2(0, -300));
        UIPanel uiPanel = debugPanel.GetComponent<UIPanel>();
        if (uiPanel == null)
        {
            Debug.LogError("DebugPanel is missing UIPanel component.");
            return;
        }

        uiPanel.titleText.text = "Debug";

        var debugPanelComponent = debugPanel.AddComponent<DebugPanel>();

        // Create and assign UI elements using prefabs
        GameObject textAreaPrefab = GameManager.Instance.prefabManager.GetPrefab("UITextArea");
        if (textAreaPrefab != null)
        {
            GameObject textGO = Instantiate(textAreaPrefab, uiPanel.scrollContent);
            debugPanelComponent.mouseCoordsText = textGO.GetComponent<UITextArea>().textArea;
            debugPanelComponent.mouseCoordsText.text = "X: -, Y: -";
        }
        else
        {
            debugPanelComponent.mouseCoordsText =
                CreateText(uiPanel.scrollContent, "MouseCoordsText", "X: -, Y: -", 14);
        }

        debugPanelComponent.instaBuildButton = uiPanel.AddButton("Insta-Build", () => { }).button;
        debugPanelComponent.instaResearchButton = uiPanel.AddButton("Insta-Research", () => { }).button;
        debugPanelComponent.unlockAllTechButton = uiPanel.AddButton("Unlock All Tech", () => { }).button;
        debugPanelComponent.triggerEventButton =
            uiPanel.AddButton("Trigger Event", () => ToggleEventTriggerPanel()).button;
        uiPanel.AddButton("Add Schemata-Bucks", () => MetaCurrency.Add(100));

        SetupEventTriggerPanel(parent);
        debugPanel.SetActive(false);
    }

    public void ToggleDebugPanel()
    {
        debugPanel.SetActive(!debugPanel.activeSelf);
    }

    private void SetupEventTriggerPanel(Transform parent)
    {
        eventTriggerPanel = CreateUIPanel(parent, "EventTriggerPanel", new Vector2(250, 400), new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f), new Vector2(200, 0));
        UIPanel uiPanel = eventTriggerPanel.GetComponent<UIPanel>();
        if (uiPanel == null)
        {
            Debug.LogError("EventTriggerPanel is missing UIPanel component.");
            return;
        }

        uiPanel.titleText.text = "Trigger Event";

        // Populate the scroll view with buttons for each event
        if (GameManager.Instance != null && GameManager.Instance.Events != null)
        {
            foreach (var gameEvent in GameManager.Instance.Events)
            {
                EventBase localEvent = gameEvent; // Local copy for the closure
                uiPanel.AddButton(gameEvent.GetEventDescription(), () =>
                {
                    GameManager.Instance.TriggerEvent(localEvent);
                    eventTriggerPanel.SetActive(false); // Close panel after triggering
                });
            }
        }

        // Add a "Back" button at the end
        var backButton = uiPanel.AddButton("< Back", () => eventTriggerPanel.SetActive(false));
        backButton.transform.SetAsLastSibling(); // Ensure it's at the bottom

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
        UIPanel uiPanel = infrastructureDetailPanel.GetComponent<UIPanel>();

        // Cleanup previous task buttons
        foreach (var button in _taskButtons)
        {
            Destroy(button.gameObject);
        }
        _taskButtons.Clear();

        // Hide static buttons that are no longer used.
        _planBuildButton.gameObject.SetActive(false);
        _upsizeButton.gameObject.SetActive(false);
        _downsizeButton.gameObject.SetActive(false);
        
        var tasks = _selectedInfrastructure.GetAvailableTasks();
        foreach (var task in tasks)
        {
            NPCTask localTask = task; // Local copy for the closure
            var newButton = uiPanel.AddButton(task.GetAssignButtonText(), () =>
            {
                GameManager.Instance.AddTask(localTask);
                HideInfrastructureDetail(); 
            });
            _taskButtons.Add(newButton.button);
        }
    }

    public void HideInfrastructureDetail()
    {
        _selectedInfrastructure = null;
        infrastructureDetailPanel.SetActive(false);
    }

    private void SetupTimeControls(Transform parent)
    {
        timeControlsContainer = CreateUIPanel(parent, "TimeControls", new Vector2(200, 50), new Vector2(1, 0),
            new Vector2(1, 0), new Vector2(-110, 35));

        UIPanel uiPanel = timeControlsContainer.GetComponent<UIPanel>();
        if (uiPanel != null)
        {
            uiPanel.titleText.text = "";
            if (uiPanel.closeButton != null) uiPanel.closeButton.gameObject.SetActive(false);
        }

        var layout = timeControlsContainer.AddComponent<HorizontalLayoutGroup>();
        layout.padding = new RectOffset(5, 5, 5, 5);
        layout.spacing = 5;
        // Make buttons fill the container
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = true;

        GameObject buttonPrefab = GameManager.Instance.prefabManager.GetPrefab("UIButton");
        if (buttonPrefab == null)
        {
            Debug.LogError("UIButton prefab not found. Cannot create TimeControls.");
            // Fallback to old method
            pauseButton = CreateButton(timeControlsContainer.transform, "||", SetTimeScalePause, new Vector2(40, 40));
            playButton = CreateButton(timeControlsContainer.transform, ">", SetTimeScalePlay, new Vector2(40, 40));
            fastForwardButton = CreateButton(timeControlsContainer.transform, ">>", SetTimeScaleFastForward,
                new Vector2(40, 40));
            superFastForwardButton = CreateButton(timeControlsContainer.transform, ">>>", SetTimeScaleSuperFastForward,
                new Vector2(40, 40));
            return;
        }

        // Pause Button
        GameObject pauseGO = Instantiate(buttonPrefab, timeControlsContainer.transform);
        UIButton pauseUI = pauseGO.GetComponent<UIButton>();
        pauseUI.buttonText.text = "||";
        pauseButton = pauseUI.button;
        pauseButton.onClick.AddListener(SetTimeScalePause);

        // Play Button
        GameObject playGO = Instantiate(buttonPrefab, timeControlsContainer.transform);
        UIButton playUI = playGO.GetComponent<UIButton>();
        playUI.buttonText.text = ">";
        playButton = playUI.button;
        playButton.onClick.AddListener(SetTimeScalePlay);

        // Fast Forward Button
        GameObject ffGO = Instantiate(buttonPrefab, timeControlsContainer.transform);
        UIButton ffUI = ffGO.GetComponent<UIButton>();
        ffUI.buttonText.text = ">>";
        fastForwardButton = ffUI.button;
        fastForwardButton.onClick.AddListener(SetTimeScaleFastForward);

        // Super Fast Forward Button
        GameObject sffGO = Instantiate(buttonPrefab, timeControlsContainer.transform);
        UIButton sffUI = sffGO.GetComponent<UIButton>();
        sffUI.buttonText.text = ">>>";
        superFastForwardButton = sffUI.button;
        superFastForwardButton.onClick.AddListener(SetTimeScaleSuperFastForward);

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

    public void TogglePause()
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
        fastForwardButton.GetComponent<Image>().color =
            _currentTimeState == TimeState.Fast ? activeColor : inactiveColor;
        superFastForwardButton.GetComponent<Image>().color =
            _currentTimeState == TimeState.SuperFast ? activeColor : inactiveColor;
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
        // Add summary text
        summaryPhaseText.textArea.text = text;
    }

    public void ShowNPCDialog(Sprite portrait, string dialog, List<DialogButtonOption> options = null)
    {
        if (_currentNPCDialogPanel != null)
        {
            // The panel itself is a child of the container with the layout group.
            // We need to activate the container.
            _currentNPCDialogPanel.gameObject.SetActive(true);
            _currentNPCDialogPanel.ShowDialog(portrait, dialog, options);
        }
        else
        {
            Debug.LogError("_currentNPCDialogPanel has not been created. Was SetupNPCDialogPanel called?", this);
        }
    }

    public void ShowAlert(string alertText)
    {
        alertPanel.SetActive(true);
        
        _alertText.text = alertText;
        
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

    private void SetupNPCDialogPanel(Transform parent)
    {
        // A container enforces the horizontal padding and prevents stretching.


        // 1. Create the panel using the standard helper method.
        var panelGO = CreateUIPanel(
            transform, 
            "NPCDialogPanel", 
            new Vector2(900, 200), 
            new Vector2(0.5f, 0.3f), 
            new Vector2(0.5f, 0.3f), 
            Vector2.zero
        );
        UIPanel uiPanel = panelGO.GetComponent<UIPanel>();
        if (uiPanel.titleText) uiPanel.titleText.gameObject.SetActive(false);
        if (uiPanel.closeButton) uiPanel.closeButton.gameObject.SetActive(false);

        // --- FIX: For this panel, we want the content to fill the space, not shrink to fit. ---
        // By destroying the ContentSizeFitter, the scrollContent will fill its parent viewport.
        var csf = uiPanel.scrollContent.GetComponent<ContentSizeFitter>();
        if (csf != null)
        {
            Destroy(csf);
        }

        // 2. Add our custom dialog panel component.
        _currentNPCDialogPanel = panelGO.AddComponent<NPCDialogPanel>();

        // 3. Create the required hierarchy and assign the few external dependencies.
        var mainLayout = new GameObject("MainLayout", typeof(RectTransform));
        mainLayout.transform.SetParent(uiPanel.scrollContent, false);
        
        var mainLayoutRect = mainLayout.GetComponent<RectTransform>();
        // Anchor to the top of the parent, let the width stretch.
        mainLayoutRect.anchorMin = new Vector2(0, 1);
        mainLayoutRect.anchorMax = new Vector2(1, 1);
        mainLayoutRect.pivot = new Vector2(0.5f, 1);
        // Set a fixed height of 145.
        mainLayoutRect.sizeDelta = new Vector2(0, 145);
        mainLayoutRect.anchoredPosition = Vector2.zero;
        
        var hlg = mainLayout.AddComponent<HorizontalLayoutGroup>();
        hlg.padding = new RectOffset(15, 15, 15, 15);
        hlg.spacing = 20;
        hlg.childAlignment = TextAnchor.MiddleLeft;
        hlg.childControlHeight = false;
        hlg.childForceExpandWidth = false;

        var portraitGO = new GameObject("PortraitImage", typeof(RectTransform));
        portraitGO.transform.SetParent(mainLayout.transform, false);
        var portraitLayout = portraitGO.AddComponent<LayoutElement>();
        portraitLayout.minWidth = 128;
        portraitLayout.minHeight = 128;
        _currentNPCDialogPanel._npcPortraitImage = portraitGO.AddComponent<Image>();
        var arf = portraitGO.AddComponent<AspectRatioFitter>();
        arf.aspectMode = AspectRatioFitter.AspectMode.HeightControlsWidth;


     
        
        
        GameObject textAreaPrefab = GameManager.Instance.prefabManager.GetPrefab("UITextArea");
        GameObject dialogTextGO = Instantiate(textAreaPrefab, mainLayout.transform);

      
        LayoutElement layoutElement = dialogTextGO.AddComponent<LayoutElement>();
        layoutElement.minWidth = 500;
        layoutElement.minHeight = 150;

        // Configure the layout element to be flexible.
        // layoutElement.flexibleHeight = 1;

        UITextArea uiTextArea = dialogTextGO.GetComponent<UITextArea>();
        uiTextArea.textArea.alignment = TextAlignmentOptions.TopLeft;
        uiTextArea.textArea.fontSize = 20;
        /*var textRect = uiTextArea.textArea.rectTransform;
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;*/

        _currentNPCDialogPanel._dialogTextMesh = uiTextArea.textArea;
        
        
        
        var buttonContainer = new GameObject("ButtonContainer", typeof(RectTransform));
        buttonContainer.transform.SetParent(mainLayout.transform, false);
        var vlg = buttonContainer.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 10;
        vlg.childControlHeight = false; // Take control of child heights
        vlg.childForceExpandHeight = true; // Force them to fill the available space
        LayoutElement buttonLayoutElement = buttonContainer.AddComponent<LayoutElement>();
        buttonLayoutElement.flexibleWidth = 1;
        buttonLayoutElement.minWidth = 100;
        
        // Create a dedicated container for buttons and give it a fixed minimum height
       
        _currentNPCDialogPanel._buttonContainer = buttonContainer.transform;

        _currentNPCDialogPanel.gameObject.SetActive(false);
    }

    private void RefreshHireDevOpsPanel()
    {
        // Clear old candidates, but skip the first child which is the "Back" button
        UIPanel hirePanel = hireDevOpsPanel.GetComponent<UIPanel>();
        if (hirePanel != null)
        {
            // Start from 1 to skip a potential 'Back' button or header
            for (int i = hirePanel.scrollContent.childCount - 1; i >= 0; i--)
            {
                // A bit brittle, but for now we assume non-candidates can be cleared.
                // A better approach would be to have a dedicated container for candidates.
                Destroy(hirePanel.scrollContent.GetChild(i).gameObject);
            }
        }
        else
        {
            // Fallback if no UIPanel
            for (int i = hireDevOpsPanel.transform.childCount - 1; i > 0; i--)
            {
                Destroy(hireDevOpsPanel.transform.GetChild(i).gameObject);
            }
        }

        var candidates = GameManager.Instance.GenerateNPCCandidates(3);

        if (hirePanel != null)
        {
            foreach (var candidate in candidates)
            {
                NPCDevOpsData localCandidate = candidate; // Local copy for closure
                hirePanel.AddButton($"Hire (${localCandidate.Stats.GetStatValue(StatType.NPC_DailyCost)}/day)", () =>
                {
                    GameManager.Instance.HireNPCDevOps(localCandidate);
                    hireDevOpsPanel.SetActive(false);
                });
            }
        }
        else
        {
            Debug.LogError("HireDevOpsPanel is missing UIPanel component.");
            foreach (var candidate in candidates)
            {
                NPCDevOpsData localCandidate = candidate; // Local copy for closure
                CreateButton(hireDevOpsPanel.transform,
                    $"Hire (${localCandidate.Stats.GetStatValue(StatType.NPC_DailyCost)}/day)", () =>
                    {
                        GameManager.Instance.HireNPCDevOps(localCandidate);
                        hireDevOpsPanel.SetActive(false);
                    });
            }
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
            Debug.LogError(
                $"Instantiated UIPanel prefab '{{uiPanelPrefab.name}}' is missing a RectTransform component. Cannot create UI Panel.");
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
        itemDetailPanel = CreateUIPanel(parent, "ItemDetailPanel", new Vector2(250, 200), new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f), Vector2.zero);
        UIPanel uiPanel = itemDetailPanel.GetComponent<UIPanel>();
        if (uiPanel == null)
        {
            Debug.LogError("ItemDetailPanel is missing UIPanel component.");
            return;
        }

        // A text area for the item's description
        GameObject textAreaPrefab = GameManager.Instance.prefabManager.GetPrefab("UITextArea");
        if (textAreaPrefab != null)
        {
            GameObject textGO = Instantiate(textAreaPrefab, uiPanel.scrollContent);
            _itemDetailDescriptionText = textGO.GetComponent<UITextArea>();
        }
        else
        {
            Debug.LogError("UITextArea prefab not found for ItemDetailPanel.");
        }

        // A container for buttons to be laid out horizontally
        GameObject buttonContainer = new GameObject("ButtonContainer", typeof(RectTransform));
        buttonContainer.transform.SetParent(uiPanel.scrollContent, false);
        var hlg = buttonContainer.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 10;
        hlg.childControlWidth = true;
        hlg.childForceExpandWidth = true;

        // Add buttons to the horizontal container
        var useButton = uiPanel.AddButton("Use", () =>
        {
            /* Action set in ShowItemDetail */
        });
        useButton.name = "UseItemButton"; // Assign a name to find it later
        useButton.transform.SetParent(buttonContainer.transform);

        var cancelButton = uiPanel.AddButton("Cancel", HideItemDetail);
        cancelButton.transform.SetParent(buttonContainer.transform);

        itemDetailPanel.SetActive(false);
    }

    public void ShowItemDetail(Items.ItemBase item)
    {
        _selectedItem = item;
        SetTimeState(TimeState.Paused); // Pause the game
        itemDetailPanel.SetActive(true);

        UIPanel uiPanel = itemDetailPanel.GetComponent<UIPanel>();
        if (uiPanel != null)
        {
            string itemName = _selectedItem.GetType().Name.Replace("Item", "");
            itemName = System.Text.RegularExpressions.Regex.Replace(itemName, "([A-Z])", " $1").Trim();
            uiPanel.titleText.text = itemName;
        }

        if (_itemDetailDescriptionText != null)
        {
            _itemDetailDescriptionText.textArea.text = "No description available.";
        }

        // Find the 'Use' button by name and update its properties
        UIButton useButton = null;
        var allButtons = itemDetailPanel.GetComponentsInChildren<UIButton>();
        foreach (var btn in allButtons)
        {
            if (btn.name == "UseItemButton")
            {
                useButton = btn;
                break;
            }
        }

        if (useButton != null)
        {
            useButton.buttonText.text = item.UseVerb();
            useButton.button.onClick.RemoveAllListeners();
            useButton.button.onClick.AddListener(() =>
            {
                GameManager.Instance.CreateUseItemTask(_selectedItem);
                HideItemDetail();
            });
        }
    }

    public void HideItemDetail()
    {
        _selectedItem = null;
        itemDetailPanel.SetActive(false);
        // Resume to whatever the state was before pausing
        SetTimeState(_timeStateBeforePause);
    }

    #endregion
}