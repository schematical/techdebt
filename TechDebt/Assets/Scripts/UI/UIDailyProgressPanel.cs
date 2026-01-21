using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace UI
{
    public class UIDailyProgressPanel: MonoBehaviour
    {
        public TextMeshProUGUI SuccededText;
        public RectTransform ProgressPanelHolder;
        public RectTransform SuccededProgressBar;
        public RectTransform FailedProgressBar;
        public TextMeshProUGUI FailedText;

   

        public void Update()
        {
            if (ProgressPanelHolder == null || SuccededProgressBar == null)
            {
                throw new SystemException("Missing `ProgressPanelHolder` or `ProgressPanel`");
            };
            float packetsFailed = GameManager.Instance.Stats.GetStatValue(StatType.PacketsFailed);
            float packetsSucceeded = GameManager.Instance.Stats.GetStatValue(StatType.PacketsServiced);
            float packetsTotal = GameManager.Instance.Stats.GetStatValue(StatType.Traffic);

            float fullWidth = ProgressPanelHolder.rect.width;
            
            float succeededPercent = packetsSucceeded / packetsTotal;
            float succededWidth = fullWidth * Mathf.Clamp01(succeededPercent);
            SuccededProgressBar.anchorMax = new Vector2(succededWidth / fullWidth, SuccededProgressBar.anchorMax.y);
            SuccededText.text = $"{Math.Round(succeededPercent * 100)}%";
            
            
            float failedPercent = packetsFailed / packetsTotal;
            float failedWidth = (fullWidth * Mathf.Clamp01(failedPercent));
            FailedProgressBar.anchorMin = new Vector2( ((fullWidth - failedWidth) / fullWidth), FailedProgressBar.anchorMin.y);
            // FailedProgressBar.transform.position = new Vector2(SuccededProgressBar.anchorMax.x, FailedProgressBar.transform.position.y);
            FailedText.text = $"{Math.Round(failedPercent * 100)}%";
        }
    }
}