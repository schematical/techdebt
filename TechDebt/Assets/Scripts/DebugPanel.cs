// DebugPanel.cs
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class DebugPanel : MonoBehaviour
{
    public Button instaBuildButton;
    public Button instaResearchButton;

    private GameManager gameManager;
    private UIManager uiManager;

    void Start()
    {
        gameManager = GameManager.Instance;
        uiManager = FindObjectOfType<UIManager>();
        instaBuildButton.onClick.AddListener(InstaBuild);
        instaResearchButton.onClick.AddListener(InstaResearch);
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
}
