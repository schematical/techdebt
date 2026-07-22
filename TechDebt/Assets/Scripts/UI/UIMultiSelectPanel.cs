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
      private List<UIMultiSelectOptionPanel> _optionPool = new List<UIMultiSelectOptionPanel>();
      public TextMeshProUGUI bottomText;
      public GameObject container;
      public UIButton confirmButton;
      [FormerlySerializedAs("previewingOption")] public UIMultiSelectOptionPanel previewingOptionPanel;

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
         
          if (previewingOptionPanel == null)
          {
              return;
          }
          previewingOptionPanel.MarkSelected();
      }

      public override void Close(bool forceClose = false)
      {
          base.Close(forceClose);
          previewingOptionPanel = null;
          CleanUp();
          GameManager.Instance.UIManager.Resume();
      }

      public void Display(string title, string bottom = "")
      {
          base.Show();
 
          previewingOptionPanel = null;
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
          foreach (UIMultiSelectOptionPanel panel in _optionPool)
          {
              panel.gameObject.SetActive(false);
          }
          _optionPool.Clear();
          confirmButton.gameObject.SetActive(false);
          _onReRoll = null;
      }
      private UIMultiSelectOptionPanel _Add(string id)
      {
          if (panelState == UIState.Closed)
          {
              Show();
          }

          GameManager.Instance.UIManager.SetTimeScalePause();

          UIMultiSelectOptionPanel optionPanel = GameManager.Instance.prefabManager.Create("UIMultiSelectOptionPanel", Vector3.zero, container.transform).GetComponent<UIMultiSelectOptionPanel>();
          _optionPool.Add(optionPanel);

          optionPanel.gameObject.SetActive(true);
        
          optionPanel.name = "UIMultiSelectOptionPanel-" + optionPanel.id;


          return optionPanel;
      }
      public UIMultiSelectOptionPanel Add(string id, Sprite sprite, string primaryText, string secondaryText = "")
      {
          UIMultiSelectOptionPanel optionPanel = _Add(id);
          optionPanel.Initialize(this, id, sprite, primaryText, secondaryText);
          return optionPanel;
      }
      public UIMultiSelectOptionPanel AddReward(iModifiable target, RewardBase rewardBase, Rarity rarity)
      {
          UIMultiSelectOptionPanel optionPanel = _Add(rewardBase.Id);
          optionPanel.SetReward( target, rewardBase, rarity);
          return optionPanel;
      }

      public void SetPreview(UIMultiSelectOptionPanel uiMultiSelectOptionPanel)
      {
          foreach (UIMultiSelectOptionPanel multiSelectOption in _optionPool)
          {
              multiSelectOption.Reset();
              
          }
          previewingOptionPanel = uiMultiSelectOptionPanel;
          previewingOptionPanel.backgroundImage.color = Color.white;
          previewingOptionPanel.selectButton.gameObject.SetActive(false);
          confirmButton.gameObject.SetActive(true);
      }

      public void RefreshBanishButtons()
      {
          foreach (UIMultiSelectOptionPanel option in _optionPool)
          {
              if (option.gameObject.activeSelf)
              {
                  option.MarkBanisable();
              }
          }
      }
    }
}