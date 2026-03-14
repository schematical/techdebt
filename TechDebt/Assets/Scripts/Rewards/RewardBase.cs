
    using UnityEngine;

    public abstract class RewardBase
    {
        public enum RewardGroup
        {
            Meta,
            NPC,
            Release
        }
        public string Id;
        public string IconSpriteId;
        public string Name;
      

        public RewardGroup Group;

    

        public abstract void Apply();

        public virtual Sprite GetSprite()
        {
            if (IconSpriteId == null)
            {
                IconSpriteId = "skelliton_0";
            }
            return GameManager.Instance.SpriteManager.GetSprite(IconSpriteId);
        }
    }
