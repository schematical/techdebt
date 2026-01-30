using System;
using System.Collections.Generic;
using UI;
using UnityEngine;
using Random = UnityEngine.Random;

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
        public RectTransform rectTransform;
        public float particleCounter = 0;
        private List<ScreenParticle> particles = new List<ScreenParticle>();
        private float nextParticleAt = 10000; 
        
        void Update()
        {
            if (animationState == AnimationState.Intro)
            {
                return;
            }
            currentDisplayTime += Time.unscaledDeltaTime;
            
            
            
            
            
            
          
           
            particleCounter += Time.unscaledDeltaTime;
            if (particleCounter >= nextParticleAt)
            {
                float halfRange = rectTransform.rect.width / 2;
                Vector2 nextPosition = new Vector2(
                    rectTransform.position.x + Random.Range(-1 * halfRange, halfRange),
                    rectTransform.position.y);
                GameObject particleGO =
                    GameManager.Instance.prefabManager.Create("ScreenParticle", nextPosition);
                ScreenParticle particle = particleGO.GetComponent<ScreenParticle>();
                particle.transform.SetParent(transform);
                particle.Init(rectTransform.rotation.z);
                particles.Add(particle);
                particleCounter = Random.Range(0f, 10f) / 10;
            }
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
                 
                    if (spriteRenderer.color.a > 0)
                    {
                        spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, spriteRenderer.color.a - (Time.unscaledDeltaTime / 2));
                    }
                    else
                    {
                        Debug.Log("TODO: End");
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
            switch (currentlyDisplayedRarity)
            {
                case(Rarity.Common):
                    nextParticleAt = 10000;
                    break;
                case(Rarity.Uncommon):
                    nextParticleAt = 10000;
                    break;
                case(Rarity.Rare):
                    nextParticleAt = 10000;
                    break;
                case(Rarity.Epic):
                    nextParticleAt = 1;
                    break;
                case(Rarity.Legendary):
                    nextParticleAt = 0.25f;
                    break;
                default:
                    throw new NotImplementedException($"Missing nextParticleAt for currentlyDisplayedRarity {currentlyDisplayedRarity}");
                    
            }
        }

        protected int GetLevelAnimationDuration()
        {
            switch (goalRarity)
            {
                case(Rarity.Common):
                    return 10;
                case(Rarity.Uncommon):
                    switch (currentlyDisplayedRarity)
                    {
                        case(Rarity.Uncommon):
                            return 10;
                        default:
                            return 5;
                    }
                case(Rarity.Rare):
                    switch (currentlyDisplayedRarity)
                    {
                        case(Rarity.Rare):
                            return 10;
                        default:
                            return 3;
                    }
                case(Rarity.Epic):
                    switch (currentlyDisplayedRarity)
                    {
                        case(Rarity.Epic):
                            return 10;
                        default:
                            return 2;
                    }  
                case(Rarity.Legendary):
                    switch (currentlyDisplayedRarity)
                    {
                        case(Rarity.Legendary):
                            return 10;
                        default:
                            return 1;
                    }
                   
            }
            throw new NotImplementedException($"GetLevelAnimationDuration {goalRarity}");
        }
        
    }
}