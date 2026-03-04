using System.Collections.Generic;
using Stats;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem.HID;
using UnityEngine.UI;

namespace UI
{
    public class UIStatDataDetailPanel: MonoBehaviour
    {
        public bool isExpanded = false;
        public TextMeshProUGUI statNameText;
        public TextMeshProUGUI statValueText;
        public Button expandButton;
        public TextMeshProUGUI expandButtonText;
        protected StatData  statData;
        protected List<UIStatModifierDetailPanel> modifierPanels = new List<UIStatModifierDetailPanel>();

        void Start()
        {
            expandButton.onClick.AddListener(OnExpandClick);
        }

        private void OnExpandClick()
        {
            isExpanded =  !isExpanded;
            switch (isExpanded)
            {
                case(true):
                    expandButtonText.text = "-";
                    break;
                case(false):
                    expandButtonText.text = "+";
                    break;
            }
            if (isExpanded)
            {
                ShowModifiers();
            }
            else
            {
                HideModifiers();
            }
            
        }

        public void Initialize(StatData _statData)
        {
            statData = _statData;
            statNameText.text = statData.Type.ToString();
            statValueText.text = statData.GetDisplayValue();
            expandButtonText.text = "+";
            expandButton.gameObject.SetActive(statData.Modifiers.Count > 0);
            HideModifiers();

        }

        void ShowModifiers()
        {
            statValueText.text = $"{statData.GetDisplayValue()} ({statData.GetDisplayBaseValue()})";
            foreach (StatModifier statModifier in statData.Modifiers)
            {
                UIStatModifierDetailPanel modifierPanel =
                    GameManager.Instance.prefabManager.Create("UIStatModifierDetailPanel", Vector3.zero, transform)
                        .GetComponent<UIStatModifierDetailPanel>();
                modifierPanel.Initialize(statModifier);
                modifierPanels.Add(modifierPanel);
            }
        }
        void HideModifiers()
        {
            foreach (UIStatModifierDetailPanel panel in modifierPanels)
            {
                panel.gameObject.SetActive(false);
            }
            modifierPanels.Clear();
        }
    }
}