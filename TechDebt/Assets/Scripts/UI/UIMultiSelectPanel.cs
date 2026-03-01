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
      public void Close(bool forceClose = false)
      {
          Debug.Log("Closing  UIMultiSelectPanel");
          base.Close(forceClose);
          CleanUp();
      
          GameManager.Instance.UIManager.Resume();
      }

      public void Display(string title, string bottom = "")
      {
          base.Show();
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
      }
      public UIMultiSelectOption Add(string id, Sprite sprite, string primaryText, string secondaryText = "")
      {
          if (panelState == UIState.Closed)
          {
              Show();
          }

          GameManager.Instance.UIManager.SetTimeScalePause();

          // Find an inactive option in the pool to reuse.
        
      
          // If no inactive option is available, create a new one.
          UIMultiSelectOption option = GameManager.Instance.prefabManager.Create("UIMultiSelectOptionPanel", Vector3.zero, container.transform).GetComponent<UIMultiSelectOption>();
          _optionPool.Add(option);

      
          option.gameObject.SetActive(true);
          option.id = id;
          option.name = "UIMultiSelectOption-" + option.id;
          option.image.sprite = sprite;
          option.primaryText.text = primaryText;
          option.secondaryText.text = secondaryText;
          
          // Clear any previous listeners and reset the button state.
          option.button.onClick.RemoveAllListeners();

          return option;
      }
    }
}