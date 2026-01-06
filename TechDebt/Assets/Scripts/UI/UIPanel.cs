using UnityEngine;
using TMPro;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIPanel : MonoBehaviour
{
    public TextMeshProUGUI titleText; // Assign the TextMeshProUGUI component for the panel title
    public Transform scrollContent;   // Assign the Transform for the scrollable content area
    public Button closeButton; // Assign the Button component for the panel's close button

    void Awake()
    {
        if (closeButton != null)
        {
           
            closeButton.onClick.AddListener(() =>
            {
                gameObject.SetActive(false);
            });
        }
        else
        {
            Debug.LogError("Missing `closeButton`");
        }
    }

    public UIButton AddButton(string buttonText, UnityAction onClickAction)
    {
        if (GameManager.Instance == null || GameManager.Instance.prefabManager == null)
        {
            Debug.LogError("GameManager or PrefabManager is null. Cannot add button to UIPanel.");
            return null;
        }

        GameObject uiButtonPrefab = GameManager.Instance.prefabManager.GetPrefab("UIButton");

        if (uiButtonPrefab == null)
        {
            Debug.LogError($"UIButton prefab with name 'UIButton' not found in PrefabManager. Cannot add button to UIPanel.");
            return null;
        }

        var buttonGO = Instantiate(uiButtonPrefab, scrollContent);
        var uiButton = buttonGO.GetComponent<UIButton>();

        if (uiButton == null)
        {
            Debug.LogError($"Instantiated UIButton prefab '{uiButtonPrefab.name}' is missing a UIButton component.");
            Destroy(buttonGO); // Clean up
            return null;
        }

        if (uiButton.buttonText == null)
        {
            Debug.LogWarning($"UIButton prefab '{uiButtonPrefab.name}' has a UIButton component but is missing its buttonText reference.");
        }
        else
        {
            uiButton.buttonText.text = buttonText;
        }

        if (uiButton.button == null)
        {
            Debug.LogWarning($"UIButton prefab '{uiButtonPrefab.name}' has a UIButton component but is missing its Button reference.");
        }
        else
        {
            uiButton.button.onClick.RemoveAllListeners(); // Clear existing listeners
            uiButton.button.onClick.AddListener(onClickAction);
        }

        return uiButton;
    }
}