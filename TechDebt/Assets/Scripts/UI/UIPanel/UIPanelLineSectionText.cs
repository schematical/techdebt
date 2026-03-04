using TMPro;
using UnityEngine;

namespace UI
{
    public class UIPanelLineSectionText: UIPanelLineSection
    {
        public TextMeshProUGUI text;
   
        public override void Initialize(UIPanelLineSectionOptions options)
        {
            base.Initialize(options);
            text.fontSize = options.fontSize;
            text.text = options.text;
            text.color = options.textColor;
        }
    }
}