using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    public enum ButtonState
    {
        Locked,
        Available,
        Selected,
        Passed
    }

    public class UIProductRoadMapLevelButton: MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public Image image;
        public TextMeshProUGUI text;
        public Button button;
        
        private ProductRoadMapLevel _level;
        public int LevelIndex { get; private set; }
        private Action<string> _onHover;
        private Action<ProductRoadMapLevel> _onClick;

        public void Init(ProductRoadMapLevel level, int levelIndex, ButtonState state, Action<string> onHover, Action<ProductRoadMapLevel> onClick)
        {
            _level = level;
            LevelIndex = levelIndex;
            _onHover = onHover;
            _onClick = onClick;
            text.text = level.Name;
            image.sprite = GameManager.Instance.SpriteManager.GetSprite(level.SpriteId);
            
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => _onClick?.Invoke(_level));

            switch (state)
            {
                case ButtonState.Locked:
                    button.interactable = false;
                    image.color = new Color(0.3f, 0.3f, 0.3f, 1.0f);
                    break;
                case ButtonState.Available:
                    button.interactable = true;
                    image.color = Color.white;
                    break;
                case ButtonState.Selected:
                    button.interactable = false;
                    image.color = Color.green;
                    break;
                case ButtonState.Passed:
                    button.interactable = false;
                    image.color = new Color(0.5f, 0.5f, 0.5f, 1.0f);
                    break;
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _onHover?.Invoke(_level.Name);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _onHover?.Invoke("");
        }
    }
}