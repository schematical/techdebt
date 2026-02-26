
    using Effects.Infrastructure;
    using UnityEngine;

    public abstract class InfrastructureTaskBase: NPCTask
    {
        
        public EnvEffectBase buildEffect { get; set; }
        public InfrastructureInstance TargetInfrastructure { get; private set; }
        protected InfrastructureTaskBase(InfrastructureInstance target): base(target)
        {
            TargetInfrastructure = target;
        }

        public override void OnUpdate(NPCBase npc)
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
            }

        }

        public override void OnEnd(NPCBase npc)
        {
            if (buildEffect != null)
            {
                buildEffect.gameObject.SetActive(false);
            }
            base.OnEnd(npc);
        }


        public override void OnInterrupt()
        {
            base.OnInterrupt();
            if (buildEffect != null)
            {
                buildEffect.gameObject.SetActive(false);
            }
        }
    }
