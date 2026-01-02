// DebugManager.cs
using UnityEngine;
using UnityEngine.InputSystem;

public class DebugManager : MonoBehaviour
{
    public static DebugManager Instance { get; private set; }

    private bool isDebugPanelOpen = false;

    private UIManager uiManager;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
    
    void Start()
    {
        uiManager = FindObjectOfType<UIManager>();
    }

    void Update()
    {
        if (Keyboard.current.backquoteKey.wasPressedThisFrame)
        {
            ToggleDebugPanel();
        }
    }

    private void ToggleDebugPanel()
    {
        isDebugPanelOpen = !isDebugPanelOpen;
        if (uiManager != null)
        {
            uiManager.ToggleDebugPanel();
        }
        Debug.Log("Debug panel toggled: " + isDebugPanelOpen);
    }
}
