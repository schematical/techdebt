using UnityEngine;
using System.Collections.Generic;
using System.Text;
using UI;

public class UIMetaChallengesPanel: UIPanel
{
    private List<Technology> _technologies;

    void OnEnable()
    {
        // Clear existing items to prevent duplicates
        foreach (Transform child in scrollContent)
        {
            Destroy(child.gameObject);
        }

        _technologies = MetaGameManager.GetAllTechnologies();

        if (_technologies == null)
        {
            Debug.LogWarning("UIMetaChallengesPanel: No technologies found in MetaGameManager.");
            return;
        }

        // Get the UITextArea prefab from the manager
        GameObject uiTextAreaPrefab = UIMainMenuCanvas.Instance.prefabManager.GetPrefab("UITextArea");
        if (uiTextAreaPrefab == null)
        {
            Debug.LogError("UITextArea prefab not found in PrefabManager. Cannot display challenges.");
            return;
        }

        foreach (var technology in _technologies)
        {
            // Instantiate a new UITextArea for each technology
            GameObject textAreaGO = Instantiate(uiTextAreaPrefab, scrollContent);
            UITextArea uiTextArea = textAreaGO.GetComponent<UITextArea>();

            if (uiTextArea != null && uiTextArea.textArea != null)
            {
                // Format the technology details into a string
                var sb = new StringBuilder();
                sb.AppendLine($"<color={(technology.CurrentState == Technology.State.Unlocked ? "#88FF88" : "white")}>{technology.DisplayName}</color>");
                sb.AppendLine($"<size=10>{technology.Description}</size>");
                sb.AppendLine($"<i>Cost: {technology.ResearchPointCost} RP | Status: {technology.CurrentState}</i>");
                
                // Set the text of the instantiated UITextArea
                uiTextArea.textArea.text = sb.ToString();
            }
            else
            {
                Debug.LogError("Instantiated UITextArea prefab is missing the UITextArea component or its text area reference.");
            }
        }
    }
}