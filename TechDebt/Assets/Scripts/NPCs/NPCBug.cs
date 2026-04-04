using System;
using System.Collections.Generic;
using DefaultNamespace;
using Items;
using Tutorial;
using UnityEngine;
using Random = UnityEngine.Random;

namespace NPCs
{
    public class NPCBug : NPCBase
    {
       
        public Severity severity = Severity.Minor;
        private float age = 0;
        private float nextLevelAge = 120;
        private bool isEvolving = false;

        public override void Initialize()
        {
            base.Initialize();
            severity = Severity.Minor;
            shadowOffset = new Vector2(0.0f, 0.0f);
            isEvolving = false;
            Stats.Get(StatType.NPC_MovmentSpeed).SetBaseValue(1.5f);
        }

  
        protected override void FixedUpdate()
        {
            if (

                GameManager.Instance.UIManager.IsPausedState() ||
                IsDead()
            )
            {
                base.FixedUpdate();
                return;
            }
            if (isEvolving)
             {
                 return;
             } 
            switch (severity)
            {
                case (Severity.Minor):
                    age += Time.fixedDeltaTime;
                    if (age > nextLevelAge)
                    {
                        IncreaseSeverity();
                    }

                    break;
            }

            base.FixedUpdate();
        }

        public override void OnDeath()
        {
            if (GameManager.Instance.GameLoopManager != null)
            {
                GameManager.Instance.TutorialManager.Trigger(TutorialStepId.NPC_Bug_Dead);
            }
        }
        void SetEvolving(bool isEvolving = true)
        {
            this.isEvolving = isEvolving;
        }
        private void IncreaseSeverity()
        {
            SetEvolving();
            StopMovement();
            //This is a bit screwy. TODO clean it up.
            nextLevelAge = 100000;
            EvolveEnvEffect evolveEnvEffect = GameManager.Instance.prefabManager.Create("EvolveEnvEffect", transform.position + new Vector3(0,0,-1)).GetComponent<EvolveEnvEffect>();
            NPCBug npcBug = null;
            evolveEnvEffect.Initialize(() =>
            {
                switch (severity)
                {
                    case (Severity.Minor):
                        npcBug = GameManager.Instance.prefabManager.Create("NPCBug", transform.position).GetComponent<NPCBug>();
                        npcBug.Initialize();
                        npcBug.SetSeverity(Severity.Medium);
                        npcBug.SetEvolving();
                        gameObject.SetActive(false);
                        GameManager.Instance.AllNpcs.Remove(this);
                        break;
                    default:
                        throw new NotImplementedException("TODO Write me");
                }
            }, () =>
            {
                npcBug.SetEvolving(false);
            });
           
        }

        private void SetSeverity(Severity _severity)
        {
            severity = _severity;
        }

        public override List<NPCTask> GetAvailableTasks()
        {
            List<NPCTask> tasks = new List<NPCTask>();
            switch (CurrentState)
            {
                case(State.Dead):
                    break;
                default:
                    tasks.Add(new AttackTask(this));
                break;
            }

            return tasks;

        }
        public override void TriggerDefaultBehavior()
        {
            if (GameManager.Instance.GameLoopManager.CurrentState != GameLoopManager.GameState.Play)
            {
                base.TriggerDefaultBehavior();
                return;
            }
            switch (severity)
            {
                case (Severity.Minor):
                    if (!CanTakeAction(CoolDownType.Consume))
                    {
                        base.TriggerDefaultBehavior();
                        return;
                    }
                    ItemBase[] allItems = GameObject.FindObjectsOfType<ItemBase>();
                    ItemBase targetItem = null;
                    float minDistance = float.MaxValue;

                    foreach (var item in allItems)
                    {
                        if (item.gameObject.activeSelf)
                        {
                            float distance = Vector3.Distance(Vector3.zero, item.transform.position);
                            if (distance < minDistance)
                            {
                                minDistance = distance;
                                targetItem = item;
                            }
                        }
                    }
                    if (targetItem != null)
                    {
             
                        AssignTask(new BugConsumeTask(targetItem));
                        return;
                    }
           
             
                    NetworkPacket[] allNetworkPackets = GameObject.FindObjectsOfType<NetworkPacket>();
                    NetworkPacket targetNetworkPacket = null;
                     minDistance = float.MaxValue;
      
                    foreach (NetworkPacket networkPackets in allNetworkPackets)
                    {
                        if (networkPackets.gameObject.activeSelf)
                        {
                            float distance = Vector3.Distance(Vector3.zero, networkPackets.transform.position);
                            if (distance < minDistance)
                            {
                                minDistance = distance;
                                targetNetworkPacket = networkPackets;
                            }
                        }
                    }
                    if (targetNetworkPacket != null)
                    {
                        AssignTask(new BugConsumeTask(targetNetworkPacket));
                        return;
                    }
                   
                    
                    break;
                case (Severity.Medium):
                    NPCDevOps npcDevOps = GameManager.Instance.AllNpcs.Find(n => n is NPCDevOps) as NPCDevOps;
                    AssignTask(new AttackTask(npcDevOps));
                    return;
                    
                    List<InfrastructureInstance> possibleTargets = new List<InfrastructureInstance>();
                    foreach (InfrastructureInstance infrastructureInstance in GameManager.Instance.ActiveInfrastructure)
                    {
                        if (
                            (
                                infrastructureInstance.GetWorldObjectType().networkPackets.Count > 0
                            ) &&
                            infrastructureInstance.IsActive() &&
                            !infrastructureInstance.IsDead()    
                        )
                        {
                            possibleTargets.Add(infrastructureInstance);
                        }
                    }
                    if (possibleTargets.Count > 0)
                    {
                        int i = Random.Range(0, possibleTargets.Count);
                        AssignTask(new AttackTask(possibleTargets[i]));
                        return;
                    }

                    break;
            }
            

           
            base.TriggerDefaultBehavior();
        }
    }
}