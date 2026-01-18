using System;
using TMPro;
using UnityEngine;

namespace UI
{
    public class UIProgressBarPanel: MonoBehaviour
    {
        public TextMeshProUGUI Text;
        public RectTransform ProgressPanelHolder;
        public RectTransform ProgressBar;

        public void SetProgress(float progress)
        {
            if (ProgressPanelHolder == null || ProgressBar == null)
            {
                throw new SystemException("Missing `ProgressPanelHolder` or `ProgressPanel`");
            };

float fullWidth = ProgressPanelHolder.rect.width;
            float newWidth = fullWidth * Mathf.Clamp01(progress);
            ProgressBar.anchorMax = new Vector2(newWidth / fullWidth, ProgressBar.anchorMax.y);
        }
    }
}