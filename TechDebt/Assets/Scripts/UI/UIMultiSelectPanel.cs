using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UnityEngine.Events;

namespace UI
{
    public class UIMultiSelectPanel: UIPanel
    {
      private List<UIMultiSelectOption> _optionPool = new List<UIMultiSelectOption>();
      public TextMeshProUGUI bottomText;
      public GameObject container;
      public UIButton confirmButton;
      public UIMultiSelectOption previewingOption;

      public UIButton rerollButton;
      private UnityAction _onReRoll;

      void Start()
      {
          confirmButton.button.onClick.AddListener(OnConfirmClick);
      }

      public void OnReRoll(UnityAction action)
      {
          _onReRoll = action;
          

          rerollButton.gameObject.SetActive(false);
          rerollButton.button.onClick.RemoveAllListeners();

          if (_onReRoll != null)
          {
              int rerollCount = (int)GameManager.Instance.GetStatValue(StatType.Global_ReRolls);
              if (rerollCount > 0)
              {
                  rerollButton.gameObject.SetActive(true);
                  rerollButton.buttonText.text = $"Re-Roll ({rerollCount})";

                  rerollButton.button.onClick.AddListener(() =>
                  {
                      if (_onReRoll != null)
                      {
                          _onReRoll.Invoke();
                      }
                  });
              }
          }
          
      }

      private void OnConfirmClick()
      {
         
          if (previewingOption == null)
          {
              return;
          }
          previewingOption.MarkSelected();
      }

      public override void Close(bool forceClose = false)
      {
          base.Close(forceClose);
          previewingOption = null;
          CleanUp();
          GameManager.Instance.UIManager.Resume();
      }

      public void Display(string title, string bottom = "")
      {
          base.Show();
 
          previewingOption = null;
          titleText.text = title;
          bottomText.text = bottom;
          CleanUp();

          if (rerollButton != null)
          {
              rerollButton.gameObject.SetActive(false);
              rerollButton.button.onClick.RemoveAllListeners();
          }

      }

      public void CleanUp()
      {
          foreach (UIMultiSelectOption panel in _optionPool)
          {
              panel.gameObject.SetActive(false);
          }
          _optionPool.Clear();
          confirmButton.gameObject.SetActive(false);
          _onReRoll = null;
      }

      public UIMultiSelectOption Add(string id, Sprite sprite, string primaryText, string secondaryText = "")
      {
          if (panelState == UIState.Closed)
          {
              Show();
          }

          GameManager.Instance.UIManager.SetTimeScalePause();

          UIMultiSelectOption option = GameManager.Instance.prefabManager.Create("UIMultiSelectOptionPanel", Vector3.zero, container.transform).GetComponent<UIMultiSelectOption>();
          _optionPool.Add(option);

      
          option.gameObject.SetActive(true);
          option.Initialize(this, id, sprite, primaryText, secondaryText);
        
          option.name = "UIMultiSelectOption-" + option.id;


          return option;
      }

      public void SetPreview(UIMultiSelectOption uiMultiSelectOption)
      {
          foreach (UIMultiSelectOption panel in _optionPool)
          {
              panel.image.color = Color.white;
          }
          previewingOption = uiMultiSelectOption;
          previewingOption.image.color = Color.green;
          confirmButton.gameObject.SetActive(true);
      }

      public void RefreshBanishButtons()
      {
          foreach (UIMultiSelectOption option in _optionPool)
          {
              if (option.gameObject.activeSelf)
              {
                  option.MarkBanisable();
              }
          }
      }
    }
}