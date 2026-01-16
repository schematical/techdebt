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

        // Ensure MetaProgressData is loaded before trying to access metaStats
        var progressData = MetaGameManager.ProgressData;

        foreach (var challenge in _challenges)
        {
            // Calculate current progress for the challenge dynamically
            int currentProgress = 0;
            var infraStats = progressData.metaStats?.infra.Find(i => i.infraId == challenge.InfrastructureId);
            if (infraStats != null)
            {
                var statPair = infraStats.stats.Find(s => s.statName == challenge.metaStat.ToString());
                if (statPair != null)
                {
                    currentProgress = statPair.value;
                }
            }
            challenge.IsCompleted = currentProgress >= challenge.RequiredValue;

            GameObject textAreaGO = Instantiate(uiTextAreaPrefab, scrollContent);
            UITextArea uiTextArea = textAreaGO.GetComponent<UITextArea>();

            if (uiTextArea != null && uiTextArea.textArea != null)
            {
                var sb = new StringBuilder();
                string challengeColor = challenge.IsCompleted ? "#88FF88" : "white"; // Green for completed

                sb.AppendLine($"<color={challengeColor}>{challenge.DisplayName}</color>");
                sb.AppendLine($"<size=10>{challenge.Description}</size>");
                sb.AppendLine($"<i>Progress: {currentProgress}/{challenge.RequiredValue} ({ (challenge.IsCompleted ? "Completed" : "In Progress") })</i>");
                sb.AppendLine($"<i>Reward: {challenge.RewardId}</i>");
                
                uiTextArea.textArea.text = sb.ToString();
            }
            else
            {
                Debug.LogError("Instantiated UITextArea prefab is missing the UITextArea component or its text area reference.");
            }
        }
    }
}