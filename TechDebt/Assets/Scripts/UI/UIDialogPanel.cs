// UIDialogPanel.cs
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

public class UIDialogPanel : UIPanel
{
    // These will be assigned by the UIManager after the panel is created
    public Image _npcPortraitImage;
    public TextMeshProUGUI _dialogTextMesh;
    public Transform _buttonContainer;

    public void ShowDialog(Sprite portrait, string dialog, List<DialogButtonOption> options)
    {
        _npcPortraitImage.sprite = portrait;
        _dialogTextMesh.text = dialog;
        Show();
        SetupButtons(options);

    
    }
    
    private void SetupButtons(List<DialogButtonOption> options)
    {
 

        // Clear existing buttons
        foreach (Transform child in _buttonContainer)
        {
            child.gameObject.SetActive(false);
        }

        // Fetch the prefab directly from the manager
       
        

        // If no options are provided, create a default "Continue" button.
        if (options == null || options.Count == 0)
        {
            options = new List<DialogButtonOption>
            {
                new DialogButtonOption { Text = "Continue", OnClick = null }
            };
        }

        foreach (DialogButtonOption option in options)
        {
            GameObject buttonGO = GameManager.Instance.prefabManager.Create("UIButton", Vector3.zero, _buttonContainer);
            UIButton uiButton = buttonGO.GetComponent<UIButton>();
            uiButton.buttonText.text = option.Text;
            uiButton.button.transform.position = Vector3.zero;
            uiButton.button.onClick.AddListener(() => {
                gameObject.SetActive(false); // Hide the parent container
                option.OnClick?.Invoke();
              
            });
           
        }
    }
}
