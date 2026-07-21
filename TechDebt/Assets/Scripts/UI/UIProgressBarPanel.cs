using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class UIProgressBarPanel : MonoBehaviour
    {
        public RectTransform ProgressBarBkgd;
        public TextMeshProUGUI Text;
        public RectTransform ProgressPanelHolder;
        public RectTransform ProgressBar;
        public TextMeshProUGUI MaskedText;
        
        protected Image progressImage;
        protected iTargetable target;
        protected iProgressable progressable;
        
        protected RectTransform rectTransform;
        protected Vector3 worldOffset = new Vector3(0f, 1.8f, 0f);

        private void Awake()
        {
            rectTransform = transform as RectTransform;
        }

        public void Initialize(iTargetable _target, iProgressable _progressable)
        {
            target = _target;
            progressable = _progressable;
            
            if (ProgressBar != null && progressImage == null)
            {
                progressImage = ProgressBar.GetComponent<Image>();
            }

            if (Text != null)
            {
                Text.color = Color.white;
            }
        }

        public void LateUpdate()
        {
            if (target == null || progressable == null)
            {
                return;
            }

            SetProgress(progressable.GetProgress());
            
            if (Text != null)
            {
                Text.text = progressable.GetProgressText();
            }

            // Keep the manually assigned masked text in sync with original text
            if (MaskedText != null && Text != null)
            {
                MaskedText.text = Text.text;
                
                // Align to exact world position of the base text
                MaskedText.rectTransform.position = Text.rectTransform.position;
                MaskedText.rectTransform.rotation = Text.rectTransform.rotation;
                
                // Keep the sizes identical so layout behaves the same
                MaskedText.rectTransform.sizeDelta = Text.rectTransform.sizeDelta;
                MaskedText.rectTransform.pivot = Text.rectTransform.pivot;
            }

            Camera cam = Camera.main;
            if (cam != null && rectTransform != null)
            {
                Vector3 worldPos = target.transform.position + worldOffset;
                Vector3 viewportPos = cam.WorldToViewportPoint(worldPos);

                // Pin the UI element to the target's viewport position
                rectTransform.anchorMin = new Vector2(viewportPos.x, viewportPos.y);
                rectTransform.anchorMax = new Vector2(viewportPos.x, viewportPos.y);
                rectTransform.anchoredPosition = Vector2.zero;
            }
        }

        public void SetProgress(float progress, Color? color = null)
        {
            if (color == null)
            {
                color = Color.white;
            }
            
            if (ProgressPanelHolder == null || ProgressBar == null)
            {
                throw new SystemException("Missing `ProgressPanelHolder` or `ProgressPanel`");
            }

            float fullWidth = ProgressPanelHolder.rect.width;
            float newWidth = fullWidth * Mathf.Clamp01(progress);
            ProgressBar.anchorMax = new Vector2(newWidth / fullWidth, ProgressBar.anchorMax.y);

            if (progressImage == null)
            {
                progressImage = ProgressBar.GetComponent<Image>();
            }

            if (progressImage != null)
            {
                progressImage.color = color.Value;
            }

            // Dynamic color contrast for the masked text based on progress bar fill luminance
            if (MaskedText != null)
            {
                float luminance = 0.2126f * color.Value.r + 0.7152f * color.Value.g + 0.0722f * color.Value.b;
                MaskedText.color = (luminance > 0.5f) ? new Color(0.15f, 0.15f, 0.15f, 1f) : Color.white;
            }
        }

        public void CleanUp()
        {
            target = null;
            gameObject.SetActive(false);
        }
    }
}