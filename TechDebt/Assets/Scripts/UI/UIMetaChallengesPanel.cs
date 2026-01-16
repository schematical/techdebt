using UnityEngine;
using System.Collections.Generic;
using System.Text;
using UI;
using MetaChallenges;

public class UIMetaChallengesPanel: UIPanel
{
    private List<MetaChallengeBase> _challenges;

    void OnEnable()
    {
        // Clear existing items to prevent duplicates
        foreach (Transform child in scrollContent)
        {
            Destroy(child.gameObject);
        }

        _challenges = MetaGameManager.GetAllChallenges();

        if (_challenges == null)
        {
            Debug.LogWarning("UIMetaChallengesPanel: No challenges found in MetaGameManager.");
            return;
        }

        GameObject uiTextAreaPrefab = UIMainMenuCanvas.Instance.prefabManager.GetPrefab("UITextArea");
        if (uiTextAreaPrefab == null)
        {
            Debug.LogError("UITextArea prefab not found in PrefabManager. Cannot display challenges.");
            return;
        }

        foreach (var challenge in _challenges)
        {
            GameObject textAreaGO = Instantiate(uiTextAreaPrefab, scrollContent);
            UITextArea uiTextArea = textAreaGO.GetComponent<UITextArea>();

            if (uiTextArea != null && uiTextArea.textArea != null)
            {
                var sb = new StringBuilder();
                
                // You will likely want to add DisplayName and Description to MetaChallengeBase
                // For now, we will construct them based on the data we have.
                string goal = $"Reach {challenge.RequiredValue} for {challenge.metaStat} on {challenge.InfrastructureId}";
                string reward = $"Unlock: {challenge.RewardId}";

                sb.AppendLine(goal);
                sb.AppendLine($"<size=10><i>Reward: {reward}</i></size>");
                
                uiTextArea.textArea.text = sb.ToString();
            }
            else
            {
                Debug.LogError("Instantiated UITextArea prefab is missing the UITextArea component or its text area reference.");
            }
        }
    }
}