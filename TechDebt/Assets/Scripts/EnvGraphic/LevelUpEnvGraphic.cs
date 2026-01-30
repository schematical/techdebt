using System;
using UnityEngine;

namespace DefaultNamespace.EnvGraphic
{
    public class LevelUpEnvGraphic:MonoBehaviour
    {
        
        public enum AnimationState
        {
            Intro,
            Loop
        };

        public Animator animator;
        protected Rarity currentlyDisplayedRarity = Rarity.Uncommon; // TODO: Do something for common
        protected Rarity goalRarity;
        const int LEVEL_ANIMATION_DURATION = 10;
        protected float currentDisplayTime = 0;
        protected AnimationState  animationState = AnimationState.Intro;

        void Update()
        {
            currentDisplayTime += Time.unscaledDeltaTime;
            if (
                currentDisplayTime >= LEVEL_ANIMATION_DURATION
            )
            {
                if (currentlyDisplayedRarity != goalRarity)
                {
                    StartNextLevel();
                }
                else
                {
                    Debug.Log("TODO: End");
                    gameObject.SetActive(false);
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
        
    }
}