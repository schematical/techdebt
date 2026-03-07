using System;
using System.Collections.Generic;
using UnityEngine;

namespace UI
{
    public class UIMoneyPanel:  MonoBehaviour
    {
        public RectTransform rectTransform;
        List<UICoin> coins = new List<UICoin>();

        private Canvas _canvas;
        private Canvas GetCanvas()
        {
            if (_canvas == null) _canvas = GetComponentInParent<Canvas>();
            return _canvas;
        }

        public void Show()
        {
            
            foreach (UICoin coin in coins)
            {
                coin.gameObject.SetActive(false);
            }
            coins.Clear();

            Canvas canvas = GetCanvas();
            Camera cam = canvas.worldCamera;
            if (cam == null) cam = Camera.main;
            
            // Calculate world scale for coin stacking
            // 20 pixels height roughly converted to world units
            Vector3 screenP0 = cam.WorldToScreenPoint(rectTransform.position);
            Vector3 screenP1 = screenP0 + new Vector3(0, 20, 0);
            Vector3 worldP1 = cam.ScreenToWorldPoint(screenP1);
            float worldStepY = worldP1.y - rectTransform.position.y;

            // Get world width for X offset
            Vector3[] corners = new Vector3[4];
            rectTransform.GetWorldCorners(corners);
            float worldWidth = Vector3.Distance(corners[0], corners[3]); 
            // Previous logic: rectTransform.position.x - rectTransform.rect.width / 2
            // Assuming intended to center in the left half? or just offset.
            // We'll use the left edge + half width (center) as a baseline if the original intent was centering.
            // Original: Pos.x - Width/2. If Pivot is Center, this is Left Edge.
            // If Pivot is Right, this is Center.
            // Let's assume we want to stack them relative to the panel.
            // Using the panel's position as a base seems safest, adjusted by the world-scale width.
            
            // Replicating "Pos.x - Width/2" in world space:
            // If the original worked in Overlay (pixels), "Width" was rect.width.
            // Now we need world width.
            // We will use the Left Edge of the panel as the spawn X roughly?
            // Let's stick to the rectTransform.position.x but offset by world-scaled width/2 to the left.
            
            // The '600' was a magic number in pixels. Let's try to anchor it to the panel bottom instead.
            // If we drop the magic number and just stack from the bottom of the panel:
            float startY = corners[0].y; 

            for (int i = 0; i < GameManager.Instance.GetStatValue(StatType.Money) / 10; i++)
            {
                // Replicate X offset: Pos.x - Width/2 (World)
                float spawnX = rectTransform.position.x - (rectTransform.rect.width * rectTransform.lossyScale.x) / 2;
                
                Vector3 pos = new Vector3(
                    spawnX, 
                    startY + i * worldStepY, 
                    rectTransform.position.z
                );
                UICoin coin = GameManager.Instance.prefabManager.Create("UICoin", pos, transform).GetComponent<UICoin>();
                coin.Initialize(false);
                coins.Add(coin);
            }
        }
        
        public void AddCoin()
        {
            Canvas canvas = GetCanvas();
            Camera cam = canvas.worldCamera;
            if (cam == null) cam = Camera.main;

            // Original: rectTransform.position.x - rectTransform.rect.width
            // Replicating "Pos.x - Width" in world space:
            float spawnX = rectTransform.position.x - (rectTransform.rect.width * rectTransform.lossyScale.x);

            // Calculate Top of Screen in World Space
            // We use the panel's Z depth
            Vector3 screenPos = cam.WorldToScreenPoint(rectTransform.position);
            Vector3 topScreenPos = new Vector3(screenPos.x, Screen.height + 100, screenPos.z);
            Vector3 worldTopPos = cam.ScreenToWorldPoint(topScreenPos);
            
            Vector3 pos = new Vector3(
                spawnX, 
                worldTopPos.y, 
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
        public void ExplodeCoins(float cost)
        {
            int coinCount = (int)Math.Round(cost / 10);
            if (coinCount > coins.Count)
            {
                coinCount = coins.Count;
            }
            List<UICoin> coinsToRemove = new List<UICoin>();
            for (int i = 0; i < coinCount; i++)
            {
                coins[i].Explode();
                coinsToRemove.Add(coins[i]);
             
            }

            foreach (UICoin coin in coinsToRemove)
            {
                coins.Remove(coin);
            }
        }
    }
}