using UnityEngine;
using System.Collections.Generic;
using System.Text;

public class UIMetaChallengesPanel : UIPanel
{
    public UITextArea challengeTextArea;

    private List<Technology> _technologies;

    void OnEnable()
    {
        RefreshChallengesDisplay();
    }

    public void RefreshChallengesDisplay()
    {
        if (challengeTextArea == null || challengeTextArea.textArea == null)
        {
            Debug.LogWarning("UIMetaChallengesPanel: UITextArea reference is missing.");
            return;
        }

        _technologies = MetaGameManager.GetAllTechnologies();

        if (_technologies == null || _technologies.Count == 0)
        {
            challengeTextArea.textArea.text = "No technologies found.";
            Debug.LogWarning("UIMetaChallengesPanel: No technologies found in MetaGameManager.");
            return;
        }

        StringBuilder sb = new StringBuilder();
        foreach (var technology in _technologies)
        {
            sb.AppendLine($"<color={(technology.CurrentState == Technology.State.Unlocked ? "green" : "white")}>{technology.DisplayName}</color>");
            sb.AppendLine($"  Description: {technology.Description}");
            sb.AppendLine($"  Cost: {technology.ResearchPointCost} RP");
            sb.AppendLine($"  Status: {technology.CurrentState}");
            sb.AppendLine($"--------------------");
        }

        challengeTextArea.textArea.text = sb.ToString();
    }

    public void OnMetaProgressUpdated()
    {
        RefreshChallengesDisplay();
    }
}