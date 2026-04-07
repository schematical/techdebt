
    using System;
    using Effects.Infrastructure;
    using UnityEngine;

    public abstract class InfrastructureTaskBase: NPCTask, iProgressable
    {
        
        public EnvEffectBase buildEffect { get; set; }
        public InfrastructureInstance TargetInfrastructure { get; private set; }
        protected float progress = 0f;
        private int displayProgress = -1;
        public InfrastructureData.State? OnQueuedSetState;
        public StatType? npcWorkSpeedStatType;
        public StatType? npcWorkQualityStatType;
        protected InfrastructureTaskBase(InfrastructureInstance target): base(target)
        {
            TargetInfrastructure = target;
        }

        public override void OnStart(NPCBase npc)
        {
            npc.AddStatusBar(this);
            base.OnStart(npc);
        }
        public void CheckBuildEffect()
        {
            if (IsCloseEnough())
            {
                if (buildEffect == null)
                {
                    GameObject be = GameManager.Instance.prefabManager.Create("BuildInfraEffect",
                        TargetInfrastructure.transform.position);
                    // be.transform.localPosition = Vector3.zero;

                    be.transform.SetParent(target.transform);
                    be.transform.localPosition = new Vector3(0, 0, -1f);
                    buildEffect = be.GetComponent<EnvEffectBase>();
                }
                else
                {
                    buildEffect.gameObject.SetActive(true);
                }
            }
            else
            {
                if (buildEffect != null)
                {
                    buildEffect.gameObject.SetActive(false);
                }
            }

        }
        
        public override void OnUpdate(NPCBase npc)
        {
            CheckBuildEffect();
            // Only start building after the NPC has arrived.
            if (IsCloseEnough())
            {
          
                NPCDevOps npcDevOps = npc.GetComponent<NPCDevOps>();

                float adjustedProgress = Time.fixedDeltaTime * GetNpcWorkSpeed(npcDevOps);
                progress += adjustedProgress;
                int checkProgress = (int)Math.Round(progress/TargetInfrastructure.GetWorldObjectType().BuildTime * 100f);
                if (checkProgress % 10 == 0 && displayProgress != checkProgress)
                {
                    displayProgress = checkProgress;
                    GameManager.Instance.FloatingTextFactory.ShowText($"{displayProgress}%",
                        TargetInfrastructure.transform.position); //  + new Vector3(0, 1, 3));
                    npc.AddXP(GetTaskExp() * Time.fixedDeltaTime );
                }
            }
        }

    


        public override void OnInterrupt()
        {
            base.OnInterrupt();
            if (buildEffect != null)
            {
                buildEffect.gameObject.SetActive(false);
            }
        }
        

        public override bool IsFinished(NPCBase npc)
        {
            return progress >= GetProgressRequirement();
        }

        protected abstract float GetProgressRequirement();

        public override string GetDescription()
        {
            return $"{base.GetDescription()} Progress: {progress}";
        }
    
        public override void OnEnd(NPCBase npc)
        {
            npc.HideProgressBar();
            if (buildEffect != null)
            {
                buildEffect.gameObject.SetActive(false);
            }

            base.OnEnd(npc);
        
            CurrentState = State.Completed; // Set status to completed
        
            TargetInfrastructure.SetState(InfrastructureData.State.Operational);
            GameManager.Instance.NotifyDailyCostChanged();

        }

        public override void OnQueued()
        {
            if (OnQueuedSetState != null)
            {
                Debug.Log($"{GetType().Name} Queued SetState - {TargetInfrastructure.gameObject.name} to `{OnQueuedSetState}`");
                TargetInfrastructure.SetState(OnQueuedSetState.Value);
            }

            base.OnQueued();
        }


        public virtual float GetProgress()
        {
            return progress / GetProgressRequirement();
        }


        protected virtual float GetNpcWorkSpeed(NPCDevOps npcDevOps)
        {
            if (npcWorkSpeedStatType != null)
            {
                return npcDevOps.Stats.GetStatValue(npcWorkSpeedStatType.Value);
            }
            return 1;
        } 
        protected virtual float GetNpcWorkQuality(NPCDevOps npcDevOps)
        {
            if (npcWorkQualityStatType != null)
            {
                return npcDevOps.Stats.GetStatValue(npcWorkQualityStatType.Value);
            }
            return 1;
        }

        protected virtual float GetTaskExp()
        {
            // TODO: Give NPC's affinities or dislikes for certain tasks and bonus or penalize appropriately.
            // TODO: Also give inspiration phases where NPCs get extra XP or debuffs if tired.
            return 1;
        }

    }
