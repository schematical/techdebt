using System.Collections.Generic;
using Stats;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class UITopBarPanel: UIPanel
    {
        private UIManager uiManager;

        void Start()
        {
            uiManager = FindObjectOfType<UIManager>();
            
            uiManager.statTexts.Clear();

            if (titleText != null)
            {
                titleText.text = "";
            }
            if (closeButton != null)
            {
                closeButton.gameObject.SetActive(false);
            }

            var layout = gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(10, 10, 5, 5);
            layout.spacing = 15;

            uiManager.gameStateText = CreateText(transform, "GameStateText", "State: Initializing", 18);
            uiManager.gameStateText.color = Color.cyan;

            uiManager.clockText = CreateText(transform, "ClockText", "9:00 AM", 18);

            var statsToDisplay = new List<StatType>
            {
                StatType.Money,
                StatType.TechDebt,
                StatType.Traffic,
                StatType.PacketsSent,
                StatType.PacketsServiced,
                StatType.DailyIncome,
                StatType.Difficulty,
                StatType.PRR
            };

            foreach (StatType type in statsToDisplay)
            {
                uiManager.statTexts.Add(type, CreateText(transform, type.ToString(), $"{type}: 0", 18));
            }
        }
        
        private TextMeshProUGUI CreateText(Transform p, string n, string c, int s)
        {
            var go = new GameObject(n);
            go.transform.SetParent(p, false);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.raycastTarget = false;
            tmp.text = c;
            tmp.fontSize = s;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            tmp.enableWordWrapping = false;
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = 8;
            tmp.fontSizeMax = s;

            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;
            rt.anchoredPosition = Vector2.zero;
            return tmp;
        }
    }
}