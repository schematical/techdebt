using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class UIPanelLineProgressBar: UIPanelLine
    {
        protected string preText = "";
        public RectTransform ProgressBarBkgd;
        public TextMeshProUGUI Text;
        public RectTransform ProgressPanelHolder;
        public RectTransform ProgressBar;
        protected Image ProgressImage;
        public override void Initialize(int _depth, UIPanel _rootPanel, UIPanelLine _parentLine)
        {
       
            var bkgdImage = ProgressBarBkgd.GetComponent<Image>();
            if (bkgdImage != null)
            {
                bkgdImage.type = Image.Type.Sliced;
            }
            base.Initialize(_depth, _rootPanel, _parentLine);
        }

        public void SetPreText(string text)
        {
            preText = text;
        }
        public void SetProgress(float progress, Color ?color = null)
        {
            if (color == null)
            {
                color = Color.white;
            }
            if (ProgressPanelHolder == null || ProgressBar == null)
            {
                throw new SystemException("Missing `ProgressPanelHolder` or `ProgressPanel`");
            };

            float fullWidth = ProgressPanelHolder.rect.width;
            float newWidth = fullWidth * Mathf.Clamp01(progress);
            ProgressBar.anchorMax = new Vector2(newWidth / fullWidth, ProgressBar.anchorMax.y);
          
            if (ProgressImage == null)
            {
                ProgressImage = ProgressBar.GetComponent<Image>();
            }

    
            ProgressImage.type = Image.Type.Sliced;
            ProgressImage.color = color.Value;
            Text.text = $"{preText}{Math.Round(progress*100)}%";
            
            float textWidth = Text.preferredWidth;
            float textX = newWidth;
            if (textX + textWidth > fullWidth)
            {
                textX = fullWidth - textWidth;
            }
            Text.rectTransform.anchoredPosition = new Vector2(textX, Text.rectTransform.anchoredPosition.y);
            
        }
    }
}