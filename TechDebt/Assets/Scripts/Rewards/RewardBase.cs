
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
        public string Description = "";
      

        public RewardGroup Group;

        public string GetTitle()
        {
            return Name;
        }

        public string GetDescription()
        {
            return Description;
        }

        public abstract void Apply();

        public virtual Sprite GetSprite()
        {
            if (IconSpriteId == null)
            {
                IconSpriteId = "skelliton";
            }
            return GameManager.Instance.SpriteManager.GetSprite(IconSpriteId);
        }
    }
