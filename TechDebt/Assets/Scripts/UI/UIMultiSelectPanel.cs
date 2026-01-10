using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class UIMultiSelectPanel: MonoBehaviour
    {
      public List<UIMultiSelectPanel> panels;
      public GameObject container;
      public void Clear()
      {
          foreach (UIMultiSelectPanel panel in panels)
          {
              Destroy(panel.gameObject);
          }
          gameObject.SetActive(false);
          GameManager.Instance.UIManager.TogglePause();
      }

      public UIMultiSelectOption Add(string id, Sprite sprite, string primaryText, string secondaryText = "")
      {
          this.gameObject.SetActive(true);
          GameManager.Instance.UIManager.SetTimeScalePause();
          GameObject prefab = GameManager.Instance.prefabManager.GetPrefab("UIMultiSelectOptionPanel");
          GameObject gameObject = Instantiate(prefab, container.transform);
          UIMultiSelectOption option = gameObject.GetComponent<UIMultiSelectOption>();
          option.id = id;
          option.name = option.name + "-" + option.id;
          option.image.sprite = sprite;
          option.primaryText.text = primaryText;
          option.secondaryText.text = secondaryText;
          
          return option;
      }
    }
}