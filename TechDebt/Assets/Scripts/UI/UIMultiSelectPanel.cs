using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UI
{
    public class UIMultiSelectPanel: MonoBehaviour
    {
      private List<UIMultiSelectOption> _optionPool = new List<UIMultiSelectOption>();
      public TextMeshProUGUI titleText;
      public TextMeshProUGUI bottomText;
      public GameObject container;
      public void Clear()
      {
          foreach (UIMultiSelectOption panel in _optionPool)
          {
              panel.gameObject.SetActive(false);
          }
          gameObject.SetActive(false);
          GameManager.Instance.UIManager.Resume();
      }

      public void Display(string title, string bottom = "")
      {
          titleText.text = title;
          bottomText.text = bottom;
          
          GameObject prefab = GameManager.Instance.prefabManager.GetPrefab("UILazarBeamPanel");
          if (prefab == null)
          {
              Debug.LogError("UILazerBeamPanel: prefab is null");
              return;
          }
          GameObject lazerGO = Instantiate(prefab, GameManager.Instance.UIManager.transform);
          // lazerGO.transform.SetAsFirstSibling();
          UILazerBeam lazerBeam = lazerGO.GetComponent<UILazerBeam>();
          lazerBeam.Init(20);
      }
      public UIMultiSelectOption Add(string id, Sprite sprite, string primaryText, string secondaryText = "")
      {
          this.gameObject.SetActive(true);
          GameManager.Instance.UIManager.SetTimeScalePause();

          // Find an inactive option in the pool to reuse.
          UIMultiSelectOption option = _optionPool.FirstOrDefault(o => !o.gameObject.activeSelf);

          if (option == null)
          {
              // If no inactive option is available, create a new one.
              GameObject prefab = GameManager.Instance.prefabManager.GetPrefab("UIMultiSelectOptionPanel");
              GameObject gameObject = Instantiate(prefab, container.transform);
              option = gameObject.GetComponent<UIMultiSelectOption>();
              _optionPool.Add(option);
          }
          
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