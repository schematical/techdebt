using System.Collections.Generic;
using UnityEngine;

namespace UI
{
    public class UIMoneyPanel:  MonoBehaviour
    {
        public RectTransform rectTransform;
        List<UICoin> coins = new List<UICoin>();

        public void AddCoin()
        {
           
            Vector3 pos = new Vector3(
                rectTransform.position.x - rectTransform.rect.width /2, 
                rectTransform.rect.height, 
                rectTransform.position.z
                );
            UICoin uiCoin = GameManager.Instance.prefabManager.Create("UICoin", pos, transform).GetComponent<UICoin>();
            coins.Add(uiCoin);
        }
    }
}