using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class UIPanelLineSectionText: UIPanelLineSection
    {
        public TextMeshProUGUI text;
        public override void Initialize()
        {
            base.Initialize();
            text.text = "";
            text.fontSize = 20;
            text.fontWeight = FontWeight.Regular;
            text.color = Color.white;
            GetComponent<LayoutElement>().preferredWidth = -1;
        }

        public void h1(string s)
        {
            text.text = s;
            text.fontSize = 24;
            text.fontWeight = FontWeight.Bold;
        }
    }
}