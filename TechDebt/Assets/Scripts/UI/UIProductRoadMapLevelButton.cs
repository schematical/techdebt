using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    public class UIProductRoadMapLevelButton: MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public Image image;
        public TextMeshProUGUI text;
        public Button button;
        
        private ProductRoadMapLevel _level;
        private Action<string> _onHover;

        public void Init(ProductRoadMapLevel level, Action<string> onHover)
        {
            _level = level;
            _onHover = onHover;
            text.text = level.Name;
            image.sprite = GameManager.Instance.SpriteManager.GetSprite(level.SpriteId);
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