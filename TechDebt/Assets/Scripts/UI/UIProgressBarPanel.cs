using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class UIProgressBarPanel: MonoBehaviour
    {
        public RectTransform ProgressBarBkgd;
        public TextMeshProUGUI Text;
        public RectTransform ProgressPanelHolder;
        public RectTransform ProgressBar;
        protected SpriteRenderer ProgressImage;
        protected iTargetable target;
        protected iProgressable progressable;

        public void Initialize(iTargetable _target, iProgressable _progressable)
        {
            target = _target;
            progressable = _progressable;
            ProgressBarBkgd.GetComponent<SpriteRenderer>().size = ProgressBarBkgd.rect.size;
        }

        public void FixedUpdate()
        {
            SetProgress(progressable.GetProgress());
            transform.position = target.transform.position + new Vector3(0f, 2f, -1.1f);
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
            // Text.text = $"{Math.Round(progress*100)}%";
            if (ProgressImage == null)
            {
                ProgressImage = ProgressBar.GetComponent<SpriteRenderer>();
            }

    
            ProgressImage.drawMode = SpriteDrawMode.Sliced;
            ProgressImage.size = ProgressBar.rect.size;
            ProgressImage.color = color.Value;
            
        }

        public void CleanUp()
        {
            target = null;
            gameObject.SetActive(false);
        }
    }
}