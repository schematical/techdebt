using Stats;
using TMPro;
using UnityEngine;

namespace UI
{
    public class UIStatModifierDetailPanel: MonoBehaviour
    {
        protected StatModifier  statModifier;
        public TextMeshProUGUI statNameText;
        public TextMeshProUGUI statValueText;
        public void Initialize(StatModifier _statModifier)
        {
            statModifier = _statModifier;
            statNameText.text = statModifier.Id;
            statValueText.text = statModifier.GetDisplayText();
        }
    }
}