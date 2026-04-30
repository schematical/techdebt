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
using Tutorial;
using Infrastructure;
using UI;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class UIManager : MonoBehaviour

{

    public enum TimeState
    {
        Paused,
        Normal,
        Fast,
        SuperFast
    }

    public enum MetricsState
    {
        Display,
        Hidden
    }
    protected List<UIAttentionIcon> attentionIcons = new  List<UIAttentionIcon>();
    
    public UITopBarPanel topBarPanel;
  
    public UIDeskMenuPanel deskMenuPanel;
    public UIMultiSelectPanel multiSelectPanel; 
    public UIReleaseHistoryPanel  releaseHistoryPanel;
    public UILeftMenuPanel leftMenuPanel;
    public UIPlanPhaseMenuPanel planPhaseMenuPanel;
    public UIWorldObjectDetailPanel worldObjectDetailPanel;
    public UISummaryPhasePanel summaryPhasePanel;
    public UITimeControlPanel timeControlPanel;
    public UIRewardPanel rewardPanel;
    public UINPCDetailPanel npcDetailPanel;
    public UIDebugPanel debugPanel;
    public RectTransform attentionIconBoarderPanel;
    public UINPCListPanel npcListPanel;
    public UITechTreePanel techTreePanel;
    public UITaskListPanel taskListPanel;
    public UIProductRoadMap productRoadMap;
    public UIGlobalStatsPanel globalStatsPanel;
    public UIEventDebugPanel eventDebugPanel;
    public UIMoneyPanel moneyPanel;
    public UIOrgChartPanel orgChartPanel;
    public UIItemDetailPanel itemDetailPanel;
    public UIAlertPanel alertPanel;
    public UIDialogPanel dialogPanel;
    public UIToastHolderPanel toastHolderPanel;
    public UIToolTip toolTip;
    public UIGameTipPanel gameTipPanel;
    public UITutorialStepListPanel tutorialStepListPanel;
    public UIVictoryConditionListPanel victoryConditionListPanel;
    public UIMetaUnlockMapPanel metaUnlockMapPanel;

    public UISaveSlotListPanel saveSlotListPanel;
    public UISaveSlotDetailPanel saveSlotDetailPanel;

    public UIMainMenu mainMenu;
    public UIMetaChallengesPanel metaChallengesPanel;
    public UIPauseMenu pauseMenu;

    public GameObject clickBlockingPanel;
    // OLD UI Containers

   


    private GameObject eventTriggerPanel;
    // UI Elements

    

    private TimeState _currentTimeState { get; set; } = TimeState.Normal;
    private TimeState _timeStateBeforePause = TimeState.Normal;
    private TimeState _userSpecifiedTimeState = TimeState.Normal;

    private float shakeDuration = 0f;
    private float shakeMagnitude = 0.1f;
    private Vector3 originalCameraPosition;

    private float lastTaskListUpdateTime;
    private bool forcePause = false;
    private MetricsState metricsState = MetricsState.Hidden;


    public void Initialize()
    {
   

        Close(true);
    }

    public void Close(bool forceClose = false)
    {
        clickBlockingPanel.gameObject.SetActive(false);
   
        productRoadMap.Close(forceClose);
 
        orgChartPanel.Close(forceClose);
        pauseMenu.Close(forceClose);
        multiSelectPanel.Close(forceClose);
        gameTipPanel.Close(forceClose);
        CloseSideBars(forceClose);
    }

    public void CloseSideBars(bool forceClose = false)
    {
        releaseHistoryPanel.Close(forceClose);
        deskMenuPanel.Close(forceClose);
        worldObjectDetailPanel.Close(forceClose);
        npcDetailPanel.Close(forceClose);
        npcListPanel.Close(forceClose);
        techTreePanel.Close(forceClose);
        taskListPanel.Close(forceClose);
        globalStatsPanel.Close(forceClose);
        eventDebugPanel.Close(forceClose);
        tutorialStepListPanel.Close(forceClose);
    }

    public void Block()
    {
        clickBlockingPanel.gameObject.SetActive(true);
    }
    public void SetupUIInfrastructure()
    {
        if (FindObjectOfType<EventSystem>() == null)
        {
            GameObject esGO = new GameObject("EventSystem");
            esGO.AddComponent<EventSystem>();
            esGO.AddComponent<InputSystemUIInputModule>();
        }

        Canvas canvas = GetComponentInParent<Canvas>();
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

    

        // Update any displays that need it after initialization
        topBarPanel.UpdateStatsDisplay();
    }
  



  

   



    void Update()
    {
        if (shakeDuration > 0)
        {
            Camera.main.transform.position = originalCameraPosition + (Vector3)Random.insideUnitCircle * shakeMagnitude;
            shakeDuration -= Time.unscaledDeltaTime;
        
            if (shakeDuration <= 0)
            {
                shakeDuration = 0f;
                Camera.main.transform.position = originalCameraPosition;
            }
        }
        
        if (Keyboard.current.backquoteKey.wasPressedThisFrame)
        {
            if (debugPanel.GetPanelState() == UIGameObject.UIState.Closed)
            {
                leftMenuPanel.Show();
                debugPanel.Show();
            }
            else
            {
                leftMenuPanel.Close();
                debugPanel.Close();
            }
        } 
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (
                pauseMenu.GetPanelState() == UIGameObject.UIState.Closed
                )
            {
                ForcePause();
                pauseMenu.Show();
            }
            else
            {
                StopForcePause();
                pauseMenu.Close();
            }
        }

        
        if (
            !forcePause &&
            Keyboard.current != null && 
            Keyboard.current.spaceKey.wasPressedThisFrame
        )
        {
            if (_currentTimeState == TimeState.Paused)
            {
                SetTimeState(_timeStateBeforePause);
            }
            else
            {
                SetTimeScalePause();
            }
        }
        
        if (
            !forcePause &&
            Keyboard.current != null && 
            Keyboard.current.fKey.wasPressedThisFrame
        )
        {
            switch (_currentTimeState)
            {
                case(TimeState.Paused):
                    SetTimeState(TimeState.Normal, true);
                    break;
                case(TimeState.Normal):
                    SetTimeState(TimeState.Fast, true);
                    break;
                case(TimeState.Fast):
                    SetTimeState(TimeState.SuperFast, true);
                    break;
                case(TimeState.SuperFast):
                    SetTimeState(TimeState.Normal, true);
                    break;
            }
        }

        if (
            Keyboard.current.shiftKey.wasPressedThisFrame &&
            GameManager.Instance.GetTechnologyByID("cloud-watch-metrics").IsUnlocked()
        )
        {

            if (metricsState == MetricsState.Display)
            {
                metricsState = MetricsState.Hidden;
            }
            else
            {
                metricsState = MetricsState.Display;
            }
            foreach (InfrastructureInstance infrastructureInstance in GameManager.Instance.ActiveInfrastructure)
            {
                if (infrastructureInstance.IsActive())
                {
                    if (metricsState == MetricsState.Display)
                    {
                        infrastructureInstance.ShowMetricsBubble();
                    }
                    else
                    {
                        infrastructureInstance.HideMetricsBubble(); 
                    }
                }
            }
        }

    }

    public void ForcePause()
    {
        forcePause = true;
        timeControlPanel.Close();
        SetTimeScalePause();
    }

    public void StopForcePause()
    {
        forcePause = false;
        timeControlPanel.Show();
        Resume();
    }

    public void SetTimeScalePause() {
        _timeStateBeforePause = _currentTimeState;
        SetTimeState(TimeState.Paused);
    }

    public void Resume()
    {
        if (forcePause)
        {
            return;
        }
        SetTimeState(_userSpecifiedTimeState);
        
    }

    public void SetTimeScalePlay(bool setDesired = false)
    {
        
        SetTimeState(UIManager.TimeState.Normal);
    }

    public void SetTimeScaleFastForward(bool setDesired = false)
    {
        SetTimeState(UIManager.TimeState.Fast, setDesired);
    }

    public void SetTimeScaleSuperFastForward(bool setDesired = false)
    {
        GameManager.Instance.UIManager.SetTimeState(UIManager.TimeState.SuperFast, setDesired);
    }

    public void SetTimeState(TimeState newState,  bool setDesired = false)
    {
        
        _currentTimeState = newState;

        float newTimeScale = 1f;
        switch (newState)
        {
            case TimeState.Paused:
                newTimeScale = 1f;
                break;
            case TimeState.Normal:
                newTimeScale = 1f;
                break;
            case TimeState.Fast:
                newTimeScale = 2f;
                break;
            case TimeState.SuperFast:
                newTimeScale = 4f;
                break;
        }

        if (newState != TimeState.Paused)
        {
            _timeStateBeforePause = newState;
            if (setDesired)
            {
                // Debug.Log($"Setting `_userSpecifiedTimeState`: {newState}");
                _userSpecifiedTimeState = newState;
            }
        }

     
        Time.timeScale = newTimeScale;
      
        timeControlPanel.UpdateTimeScaleButtons();
    }

    public bool IsPausedState()
    {
        return _currentTimeState == TimeState.Paused;
    }
    public void ShowPlanUI()
    {
        Close();
        planPhaseMenuPanel.Show();
        timeControlPanel.Close();
    }

    public void HidePlanUI()
    {
        planPhaseMenuPanel.Close();
        timeControlPanel.Show();
    }


    

    public void ShowAlert(string alertText)
    {
        alertPanel.Show();
        
        alertPanel.bodyText.text = alertText;
        
    }

   

    public void UpdateClockDisplay(float timeElapsed, float dayDuration)
    {
        topBarPanel.UpdateClockDisplay(timeElapsed, dayDuration);
    }

    public TimeState GetCurrentTimeState()
    {
        return _currentTimeState;
    }

    public UIAttentionIcon AddAttentionIcon(Transform _transform, Color color, UnityAction onClick)
    {
       
            GameObject iconGo = GameManager.Instance.prefabManager.Create("UIAttentionIcon", _transform.position);
            UIAttentionIcon attentionIcon = iconGo.GetComponent<UIAttentionIcon>();
            attentionIcon.Show(_transform, color, onClick);
            return attentionIcon;
    }

    public void ShowPacketFail(Sprite sprite)
    {
        RectTransform rt = GetComponent<RectTransform>();
        if (rt == null) return;

        int n = Random.Range(-1, 1);
        float width = rt.rect.width;
        float middle = width / 2f;
        float min = middle + (n * width / 4f);
        float max = width;
        if (n < 0)
        {
            max = min;
            min = 0f;
        }

        float x = Random.Range(min, max);
        Vector3 localPos = new Vector3(rt.rect.xMin + x, rt.rect.yMax, 0);
        Vector3 worldPos = rt.TransformPoint(localPos);

        UIScreenParticle screenParticle = GameManager.Instance.prefabManager.Create("UIScreenParticle", worldPos, transform).GetComponent<UIScreenParticle>();
        screenParticle.Init(
            sprite,
            Random.value * 360,
            new List<UIScreenParticle.Effects>()
            {
                UIScreenParticle.Effects.Fire
            }
        );
        TriggerScreenShake();
    }
    public void ShowNPCDialog(Sprite botSprite, string dialog, List<DialogButtonOption> options = null)
    {
        dialogPanel.ShowDialog(botSprite, dialog, options);
    }

    public void TriggerScreenShake(float duration = 0.2f, float magnitude = 0.1f)
    {
        shakeDuration = duration;
        shakeMagnitude = magnitude;
        originalCameraPosition = Camera.main.transform.position;
    }

    public void ShowMainMenu()
    {
        Close(true);
        Block();
        victoryConditionListPanel.Close(true);
        topBarPanel.Close(true);
        moneyPanel.Close(true);
        timeControlPanel.Close(true);
        mainMenu.Show();
        summaryPhasePanel.Close();
    }

    public void ShowGameUI()
    {
        Close(true);
        topBarPanel.Clear();
        topBarPanel.Show();
        // moneyPanel.Show();
        victoryConditionListPanel.Show();
        timeControlPanel.Show();
        mainMenu.Close();
        victoryConditionListPanel.Refresh();
        summaryPhasePanel.Close();
    }


    public void RemoveBlock()
    {
        clickBlockingPanel.gameObject.SetActive(false);
    }

    public void Reset()
    {
        SetTimeState(TimeState.Normal, true);
        clickBlockingPanel.gameObject.SetActive(false);
        summaryPhasePanel.Close();
        
    }

 
}