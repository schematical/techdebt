using UnityEngine;
using UnityEngine.Serialization;

namespace EnvGraphic
{
    public class RandomizableSprite: MonoBehaviour
    {
        public enum Type
        {
            NPCHead,
            NPCBody
        };
        public Type type;
        public SpriteRenderer spriteRenderer;

        void Start()
        {
            spriteRenderer.sprite = GameManager.Instance.SpriteManager.GetRandom(type.ToString());
        }
    }
}