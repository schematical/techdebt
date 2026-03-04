using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class UIPanelLineSectionButton: UIPanelLineSectionText
    {
        public Button button;
        // public Image image;
        public override void Initialize(UIPanelLineSectionOptions options)
        {
            base.Initialize(options);
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(options.onClick.Invoke);
            if (options.sprite != null)
            {
                text.gameObject.SetActive(false);
                button.image.sprite = options.sprite;
                /*image.sprite = options.sprite;
                image.gameObject.SetActive(true);*/
            }

            if (options.text == null)
            {
                text.gameObject.SetActive(false);
            }/*
            else
            {
                image.gameObject.SetActive(false);
                text.gameObject.SetActive(true);
            }*/
        }
    }
}