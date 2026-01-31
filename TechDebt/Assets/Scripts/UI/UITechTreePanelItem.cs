using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Linq;

public class UITechTreePanelItem : MonoBehaviour
{
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI requirementsText;
    public UIButton researchButton;

    private Technology _techData;

    public void Initialize(Technology tech)
    {
        _techData = tech;

        titleText.text = $"<b>{_techData.DisplayName}</b>";
        descriptionText.text = _techData.Description;

        string reqText = "Requires: " + (_techData.RequiredTechnologies.Count == 0 ? "None" :
            string.Join(", ", _techData.RequiredTechnologies.Select(reqId =>
                GameManager.Instance.GetTechnologyByID(reqId)?.DisplayName ?? "Unknown")));
        requirementsText.text = reqText;
        
        researchButton.button.onClick.AddListener(() => GameManager.Instance.SelectTechnologyForResearch(_techData));
        
        // Set initial state
        UpdateState();
    }

    public void UpdateState()
    {
        if (_techData == null || researchButton == null || researchButton.buttonText == null || !researchButton.button) return;

        var buttonImage = researchButton.GetComponent<Image>();
        bool prerequisitesMet = _techData.RequiredTechnologies.All(reqId => GameManager.Instance.GetTechnologyByID(reqId)?.CurrentState == Technology.State.Unlocked);

        switch (_techData.CurrentState)
        {
            case Technology.State.Unlocked:
                researchButton.buttonText.text = "Unlocked";
                researchButton.button.interactable = false;
                if (buttonImage != null) buttonImage.color = Color.green;
                break;

            case Technology.State.Researching:
                UpdateProgress(); // Show initial progress
                researchButton.button.interactable = false;
                if (buttonImage != null) buttonImage.color = Color.cyan;
                break;

            case Technology.State.Locked:
                researchButton.buttonText.text = "Research";
                if (prerequisitesMet && GameManager.Instance.CurrentlyResearchingTechnology == null)
                {
                    researchButton.button.interactable = true;
                    if (buttonImage != null) buttonImage.color = Color.yellow;
                }
                else
                {
                    researchButton.button.interactable = false;
                    if (buttonImage != null) buttonImage.color = Color.gray;
                }
                break;
        }
    }

    public void UpdateProgress()
    {
        if (_techData != null && _techData.CurrentState == Technology.State.Researching)
        {
            float percentage = (_techData.CurrentResearchProgress / _techData.ResearchPointCost) * 100f;
            researchButton.buttonText.text = $"Researching... ({Mathf.FloorToInt(percentage)}%)";
        }
    }
}
