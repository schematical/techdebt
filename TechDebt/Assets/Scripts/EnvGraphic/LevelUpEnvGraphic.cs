using System;
using UnityEngine;

namespace DefaultNamespace.EnvGraphic
{
    public class LevelUpEnvGraphic:EnvGraphicBase
    {
        
        public enum AnimationState
        {
            Intro,
            Loop
        };

        public Animator animator;
        protected Rarity currentlyDisplayedRarity = Rarity.Common; // TODO: Do something for common
        protected Rarity goalRarity = Rarity.Legendary;
        protected float currentDisplayTime = 0;
        protected AnimationState  animationState = AnimationState.Intro;
        public SpriteRenderer spriteRenderer;
        void Update()
        {
            if (animationState == AnimationState.Intro)
            {
                return;
            }
            currentDisplayTime += Time.unscaledDeltaTime;
            if (
                currentDisplayTime >= GetLevelAnimationDuration()
            )
            {
                if (currentlyDisplayedRarity != goalRarity)
                {
                    StartNextLevel();
                }
                else
                {
                    Debug.Log("TODO: End");
                    if (spriteRenderer.color.a > 0)
                    {
                        spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, spriteRenderer.color.a - (Time.unscaledDeltaTime / 2));
                    }
                    else
                    {
                        gameObject.SetActive(false);
                    }
                }
            }
        }
        public void EndIntro()
        {
            currentDisplayTime = 0;
            animationState =  AnimationState.Loop;
            Debug.Log($"Show{currentlyDisplayedRarity}");
            animator.SetBool($"Show{currentlyDisplayedRarity}", true);
        }
        public void StartNextLevel()
        {
          
            currentlyDisplayedRarity = RarityHelper.GetNextRarity(currentlyDisplayedRarity);
            Debug.Log($"StartNextLevel {currentlyDisplayedRarity}");
            animationState =  AnimationState.Intro;
            currentDisplayTime = 0;
            animator.SetBool($"Intro{currentlyDisplayedRarity}", true);
        }

        protected int GetLevelAnimationDuration()
        {
            switch (goalRarity)
            {
                case(Rarity.Common):
                    return 10;
                case(Rarity.Uncommon):
                    return 5;
                case(Rarity.Rare):
                    return 3;
                case(Rarity.Legendary):
                    return 3;
            }
            throw new NotImplementedException($"GetLevelAnimationDuration {goalRarity}");
        }
        
    }
}