using UnityEngine;
using UnityEngine.Serialization;

namespace EnvGraphic
{
    public class RandomizableSprite: MonoBehaviour
    {
        public SpriteRenderer spriteRenderer;

        void Start()
        {
            spriteRenderer.sprite = GameManager.Instance.SpriteManager.GetRandom("NPCHead");
        }
    }
}