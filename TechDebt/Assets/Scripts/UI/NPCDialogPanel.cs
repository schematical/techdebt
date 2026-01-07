// NPCDialogPanel.cs
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

public class NPCDialogPanel : MonoBehaviour
{
    // These will be assigned by the UIManager after the panel is created
    public Image _npcPortraitImage;
    public TextMeshProUGUI _dialogTextMesh;
    public Transform _buttonContainer;

    public void ShowDialog(Sprite portrait, string dialog, List<DialogButtonOption> options)
    {
        if (_npcPortraitImage != null)
        {
            _npcPortraitImage.sprite = portrait;
            _npcPortraitImage.gameObject.SetActive(portrait != null);
        }

        if (_dialogTextMesh != null)
        {
            _dialogTextMesh.text = dialog;
        }

        SetupButtons(options);
        
        // Activate the top-level container
        transform.parent.gameObject.SetActive(true);
    }
    
    private void SetupButtons(List<DialogButtonOption> options)
    {
        if (_buttonContainer == null)
        {
            Debug.LogError("Button container is null. Cannot create buttons.", this);
            return;
        }

        // Clear existing buttons
        foreach (Transform child in _buttonContainer)
        {
            Destroy(child.gameObject);
        }

        // Fetch the prefab directly from the manager
        GameObject buttonPrefab = GameManager.Instance.prefabManager.GetPrefab("UIButton");
        if (buttonPrefab == null)
        {
            Debug.LogError("FATAL: 'UIButton' prefab not found in PrefabManager. Cannot add buttons to NPCDialogPanel.", this);
            return;
        }

        // If no options are provided, create a default "Continue" button.
        if (options == null || options.Count == 0)
        {
            options = new List<DialogButtonOption>
            {
                new DialogButtonOption { Text = "Continue", OnClick = null }
            };
        }

        foreach (var option in options)
        {
            GameObject buttonGO = Instantiate(buttonPrefab, _buttonContainer);
            UIButton uiButton = buttonGO.GetComponent<UIButton>();

            if (uiButton != null && uiButton.button != null && uiButton.buttonText != null)
            {
                uiButton.buttonText.text = option.Text;
                uiButton.button.onClick.AddListener(() => {
                    option.OnClick?.Invoke();
                    transform.parent.gameObject.SetActive(false); // Hide the parent container
                });
            }
            else
            {
                Debug.LogError("The UIButton prefab is missing a UIButton, Button, or TextMeshProUGUI component.", buttonGO);
            }
        }
    }
}
