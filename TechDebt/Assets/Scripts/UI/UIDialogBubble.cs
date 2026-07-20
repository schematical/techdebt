using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace UI
{
    public class UIDialogBubble: UIPanel
    {
        private const float TextRevealSpeed = 0.025f;

        private struct DisplayRequest
        {
            public string Description;
            public List<DialogButtonOption> Options;
        }

        private Queue<DisplayRequest> displayQueue = new Queue<DisplayRequest>();
        private bool isDisplayingText = false;

        private string currentFullText = "";
        private int currentCharacterIndex = 0;
        private float textRevealTimer = 0f;
        private UIPanelLineSectionText currentTextSection;
        private List<DialogButtonOption> currentOptions;

        public RectTransform pointer;
        protected iUIDialogBubbleAttachable target;
        protected Vector3 worldOffset = new Vector3(0, 1.5f, .5f);
        public RectTransform dialogBox;

        protected override void Awake()
        {
            runUICloseOnShow = false;
            base.Awake();
        }

        public override void Show()
        {
            displayQueue.Clear();
            isDisplayingText = false;
            CleanUp();
            gameObject.SetActive(true);
            transform.SetAsFirstSibling();
            panelState = UIState.Open;
        }

        public void SimpleDisplay(string description, List<DialogButtonOption> options = null)
        {
            if (options == null)
            {
                options = new List<DialogButtonOption>()
                {
                    new DialogButtonOption()
                    {
                        Text = "Continue", 
                        OnClick = () =>
                        {
                            Close();
                        }
                    },
                };
            }
            
            DisplayRequest request = new DisplayRequest();
            request.Description = description;
            request.Options = options;
            
            displayQueue.Enqueue(request);
        }

        private void FixedUpdate()
        {
            if (isDisplayingText)
            {
                textRevealTimer += Time.unscaledDeltaTime;
                bool textChanged = false;
                
                while (textRevealTimer >= TextRevealSpeed && isDisplayingText)
                {
                    textRevealTimer -= TextRevealSpeed;
                    currentCharacterIndex++;
                    textChanged = true;
                    
                    if (currentCharacterIndex >= currentFullText.Length)
                    {
                        isDisplayingText = false;
                    }
                }

                if (textChanged && currentTextSection != null && currentTextSection.text != null)
                {
                    int safeIndex = Mathf.Min(currentCharacterIndex, currentFullText.Length);
                    currentTextSection.text.text = currentFullText.Substring(0, safeIndex);
                    MarkUpdated();
                }

                if (!isDisplayingText)
                {
                    ShowButtons(currentOptions);
                }
            }
            else if (displayQueue.Count > 0)
            {
                StartNextDisplay();
            }
        }

        private void StartNextDisplay()
        {
            DisplayRequest request = displayQueue.Dequeue();
            
            currentFullText = request.Description;
            currentOptions = request.Options;
            currentCharacterIndex = 0;
            textRevealTimer = 0f;
            isDisplayingText = true;
            
            currentTextSection = AddLine<UIPanelLine>().Add<UIPanelLineSectionText>();
            currentTextSection.text.text = "";
            MarkUpdated();

            if (string.IsNullOrEmpty(currentFullText))
            {
                isDisplayingText = false;
                ShowButtons(currentOptions);
            }
        }

        private void ShowButtons(List<DialogButtonOption> options)
        {
            foreach (DialogButtonOption option in options)
            {
                AddButton(
                    option.Text,
                    () =>
                    {
                        option.OnClick?.Invoke();
                    }
                );
            }
            MarkUpdated();
        }

        public void SetTarget(iUIDialogBubbleAttachable target)
        {
            this.target = target;
        }

        public override void Close(bool forceClose = false)
        {
            displayQueue.Clear();
            isDisplayingText = false;
            CleanUp();
            gameObject.SetActive(false);
        }

        public override void RefreshLayout()
        {
            base.RefreshLayout();
            UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(dialogBox);
        }

        protected override void LateUpdate()
        {
            base.LateUpdate();
            Camera cam = Camera.main;

            Vector3 worldPos = target.transform.position + worldOffset;
            Vector3 viewportPos = cam.WorldToViewportPoint(worldPos);

            bool isOffScreen = viewportPos.z < 0 || 
                               viewportPos.x < 0 || viewportPos.x > 1 || 
                               viewportPos.y < 0 || viewportPos.y > 1;
            
            // Pin the UI element to the target's viewport position
            rectTransform.anchorMin = new Vector2(viewportPos.x, viewportPos.y);
            rectTransform.anchorMax = new Vector2(viewportPos.x, viewportPos.y);
            rectTransform.anchoredPosition = Vector2.zero;
        }
    }
}
