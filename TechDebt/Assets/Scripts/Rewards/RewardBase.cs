
    using System.Collections.Generic;
    using System.Linq;
    using UI;
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
        public List<UnlockCondition> UnlockConditions = new();
      

        public RewardGroup Group;

        public virtual string GetTitle()
        {
            return Name;
        }

        public virtual string GetDescription()
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

        public virtual UIPanelLine Render(UIPanelLine line)
        {
            UIPanelLine rewardLine = line.AddLine<UIPanelLine>();
            rewardLine.Add<UIPanelLineSectionImage>().image.sprite = GetSprite();
            rewardLine.AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().h3(GetTitle());
            rewardLine.AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = GetDescription();
            
            return rewardLine;
        }

        public virtual bool IsUnlocked()
        {
            return UnlockConditions.All((condition) => condition.IsUnlocked() );
        }
    }
