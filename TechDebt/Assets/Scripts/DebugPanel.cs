// DebugPanel.cs
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using TMPro;
using UnityEngine.InputSystem;

public class DebugPanel : MonoBehaviour
{
    public Button instaBuildButton;
    public Button instaResearchButton;
    public Button unlockAllTechButton;
    public Button triggerEventButton;
    public TextMeshProUGUI mouseCoordsText;

    private GameManager gameManager;
    private UIManager uiManager;
    private GridManager gridManager;

    void Start()
    {
        gameManager = GameManager.Instance;
        uiManager = FindObjectOfType<UIManager>();
        gridManager = GridManager.Instance;
        instaBuildButton.onClick.AddListener(InstaBuild);
        instaResearchButton.onClick.AddListener(InstaResearch);
        unlockAllTechButton.onClick.AddListener(UnlockAllTechnologies);
    }

    void Update()
    {
        if (mouseCoordsText != null && gridManager != null && Camera.main != null)
        {
            Vector3 mousePos = Mouse.current.position.ReadValue();
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
            Vector3Int cellPos = gridManager.gridComponent.WorldToCell(worldPos);
            mouseCoordsText.text = $"X: {cellPos.x}, Y: {cellPos.y}";
        }
    }

    private void InstaBuild()
    {
        if (gameManager == null) return;

        var plannedInfrastructure = gameManager.ActiveInfrastructure.FirstOrDefault(i => i.data.CurrentState == InfrastructureData.State.Planned);
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
        if (gameManager == null) return;

        if (gameManager.CurrentlyResearchingTechnology != null)
        {
            string techName = gameManager.CurrentlyResearchingTechnology.DisplayName;
            gameManager.ApplyResearchProgress(gameManager.CurrentlyResearchingTechnology.ResearchPointCost);
            Debug.Log($"Insta-researched {techName}");
            if (uiManager != null)
            {
                uiManager.ForceRefreshTechTreePanel();
            }
        }
        else
        {
            Debug.Log("No technology is currently being researched.");
        }
    }

    private void UnlockAllTechnologies()
    {
        if (gameManager == null) return;

        gameManager.UnlockAllTechnologies();
        Debug.Log("All technologies unlocked.");
        if (uiManager != null)
        {
            uiManager.ForceRefreshTechTreePanel();
        }
    }
}
