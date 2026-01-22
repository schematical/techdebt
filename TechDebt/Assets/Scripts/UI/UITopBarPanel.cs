using System;
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
        private Dictionary<StatType, TextMeshProUGUI> _statTexts = new Dictionary<StatType, TextMeshProUGUI>();
        private TextMeshProUGUI _gameStateText;
        private TextMeshProUGUI _clockText;
        
        
        void Start()
        {
            _statTexts.Clear();

            if (titleText != null)
            {
                titleText.text = "";
            }
 

            HorizontalLayoutGroup layout = gameObject.GetComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(10, 10, 5, 5);
            layout.spacing = 15;

            _gameStateText = CreateText(transform, "GameStateText", "State: Initializing", 24);
            _gameStateText.color = Color.cyan;

            _clockText = CreateText(transform, "ClockText", "9:00 AM", 24);

            var statsToDisplay = new List<StatType>
            {
                StatType.Money,
                StatType.Traffic,
                StatType.DailyIncome,
                StatType.Difficulty,
            };

            foreach (StatType type in statsToDisplay)
            {
                _statTexts.Add(type, CreateText(transform, type.ToString(), $"{type}: 0", 24));
            }
            
            UpdateStatsDisplay();
        }

        void OnEnable()
        {
            GameManager.OnStatsChanged += UpdateStatsDisplay;
            if (GameManager.Instance != null)
            {
                GameManager.Instance.Stats.Stats[StatType.Money].OnStatChanged += OnMoneyChanged;
            }
        }

        void OnDisable()
        {
            GameManager.OnStatsChanged -= UpdateStatsDisplay;
            if (GameManager.Instance != null)
            {
               GameManager.Instance.Stats.Stats[StatType.Money].OnStatChanged -= OnMoneyChanged;
            }
        }

        private void OnMoneyChanged(float value)
        {
            UpdateStatText(StatType.Money, value);
        }

        public void UpdateGameStateDisplay(string state)
        {
            if (_gameStateText != null) _gameStateText.text = $"State: {state}";
        }

        public void UpdateClockDisplay(float timeElapsed, float dayDuration)
        {
            if (_clockText == null) return;

            int day = GameManager.Instance.GameLoopManager.currentDay;

            float dayPercentage = Mathf.Clamp01(timeElapsed / dayDuration);
            float totalWorkdayHours = 8f;
            float elapsedHours = totalWorkdayHours * dayPercentage;
            int currentHour = 9 + (int)elapsedHours;
            int currentMinute = (int)((elapsedHours - (int)elapsedHours) * 60);

            string amPm = currentHour < 12 ? "AM" : "PM";
            int displayHour = currentHour > 12 ? currentHour - 12 : currentHour;
            if (displayHour == 0) displayHour = 12;

            _clockText.text = $"Day: {day} | {displayHour:D2}:{currentMinute:D2} {amPm}";
        }

        public void UpdateStatsDisplay()
        {
            if (GameManager.Instance == null) return;
            foreach (var statText in _statTexts)
            {
                statText.Value.text = $"{statText.Key}: {Math.Round(GameManager.Instance.GetStat(statText.Key))}";
            }
        }
        
        public void UpdateStatText(StatType statType, float value)
        {
            if (_statTexts.ContainsKey(statType))
            {
                _statTexts[statType].text = $"{statType}: {value:F2}";
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