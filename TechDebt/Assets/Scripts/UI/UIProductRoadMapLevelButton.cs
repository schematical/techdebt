using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class UIProductRoadMapLevelButton: MonoBehaviour
    {
        public Image image;
        public TextMeshProUGUI text;
        public Button button;

        public void Init(ProductRoadMapLevel level)
        {
            text.text = level.Name;
            image.sprite = GameManager.Instance.SpriteManager.GetSprite(level.SpriteId);
        }
    }
}