using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UI
{
    public class UIMultiSelectPanel: UIPanel
    {
      private List<UIMultiSelectOption> _optionPool = new List<UIMultiSelectOption>();
      public TextMeshProUGUI bottomText;
      public GameObject container;
      public UIButton confirmButton;
      public UIMultiSelectOption previewingOption;

      void Start()
      {
          confirmButton.button.onClick.AddListener(OnConfirmClick);
      }

      private void OnConfirmClick()
      {
         
          if (previewingOption == null)
          {
              Debug.LogError("previewingOption is null");
              return;
          }
          previewingOption.MarkSelected();
      }

      public void Close(bool forceClose = false)
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
      }

      public void CleanUp()
      {
          foreach (UIMultiSelectOption panel in _optionPool)
          {
              panel.gameObject.SetActive(false);
          }
          _optionPool.Clear();
          confirmButton.gameObject.SetActive(false);
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
          Debug.Log("SetPreview: " + uiMultiSelectOption.id);
          previewingOption = uiMultiSelectOption;
          confirmButton.gameObject.SetActive(true);
      }
    }
}