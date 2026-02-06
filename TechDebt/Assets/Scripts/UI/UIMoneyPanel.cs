using System;
using System.Collections.Generic;
using UnityEngine;

namespace UI
{
    public class UIMoneyPanel:  MonoBehaviour
    {
        public RectTransform rectTransform;
        List<UICoin> coins = new List<UICoin>();

        public void Show()
        {
            
            foreach (UICoin coin in coins)
            {
                coin.gameObject.SetActive(false);
            }
            coins.Clear();
            for (int i = 0; i < GameManager.Instance.GetStat(StatType.Money) / 10; i++)
            {
                Vector3 pos = new Vector3(
                    rectTransform.position.x - rectTransform.rect.width /2, 
                    i * 20 + 200, 
                    rectTransform.position.z
                );
                UICoin coin = GameManager.Instance.prefabManager.Create("UICoin", pos, transform).GetComponent<UICoin>();
                coin.Initialize(false);
                coins.Add(coin);
            }
        }
        
        public void AddCoin()
        {
           
            Vector3 pos = new Vector3(
                rectTransform.position.x - rectTransform.rect.width /2, 
                rectTransform.rect.height * 1.25f, 
                rectTransform.position.z
                );
            UICoin coin = GameManager.Instance.prefabManager.Create("UICoin", pos, transform).GetComponent<UICoin>();
            coin.Initialize();
            coins.Add(coin);
        }

        public void SpendCoins(float totalDailyCost)
        {
            int coinCount = (int)Math.Round(totalDailyCost / 10);
            Debug.Log($"Spending - Coin Count: {coinCount}  totalDailyCost: {totalDailyCost}");
            if (coinCount > coins.Count)
            {
                coinCount = coins.Count;
            }
            List<UICoin> coinsToRemove = new List<UICoin>();
            for (int i = 0; i < coinCount; i++)
            {
                coins[i].Spend();
                coinsToRemove.Add(coins[i]);
             
            }

            foreach (UICoin coin in coinsToRemove)
            {
                coins.Remove(coin);
            }
        }
    }
}