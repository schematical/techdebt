using System;
using System.Collections.Generic;
using DefaultNamespace;
using Items;
using UnityEngine;
using Random = UnityEngine.Random;

namespace NPCs
{
    public class NPCBug : NPCBase
    {
        public enum Severity { Minor, Medium, Major, Critical }
        public Severity severity = Severity.Minor;
        public float age = 0;
        public float nextLevelAge = 30;
        public bool isEvolving = false;
        public override bool CanAssignTask(NPCTask task)
        {
            return task is BugConsumeItemTask;
        }

        public override void Initialize()
        {
            base.Initialize();
            isEvolving = false;
            Stats.Get(StatType.NPC_MovmentSpeed).SetBaseValue(1.5f);
        }

        void Update()
        {
            if (isEvolving)
            {
                return;
            } 
            base.Update();
        }
        void FixedUpdate()
        {
            age += Time.fixedDeltaTime;
            if (age > nextLevelAge)
            {
                IncreaseSeverity();
            }
            base.FixedUpdate();
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
                        severity = Severity.Medium;
                        npcBug = GameManager.Instance.prefabManager.Create("NPCBug", transform.position).GetComponent<NPCBug>();
                        npcBug.Initialize();
                        npcBug.SetEvolving();
                        gameObject.SetActive(false);
                        break;
                    default:
                        throw new NotImplementedException("TODO Write me");
                }
            }, () =>
            {
                npcBug.SetEvolving(false);
            });
           
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
            switch (severity)
            {
                case (Severity.Minor):
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
             
                        AssignTask(new BugConsumeItemTask(targetItem));
                        return;
                    }
           
                    
                    //TODO: Find network packets to eat
                    
                    break;
                case (Severity.Medium):
                    List<InfrastructureInstance> possibleTargets = new List<InfrastructureInstance>();
                    foreach (InfrastructureInstance infrastructureInstance in GameManager.Instance.ActiveInfrastructure)
                    {
                        if (
                            (
                                // infrastructureInstance.data.NetworkConnections.Count > 0 ||
                                infrastructureInstance.data.networkPackets.Count > 0
                            ) &&
                            infrastructureInstance.IsActive() &&
                            !infrastructureInstance.IsDead()    
                        )
                        {
                            possibleTargets.Add(infrastructureInstance);
                        }
                    }
                    Debug.Log($"TriggerDefaultBehavior: `{possibleTargets.Count}`");
                    if (possibleTargets.Count > 0)
                    {
                        int i = Random.Range(0, possibleTargets.Count);
                        Debug.Log($"{name} Starting to attack `{possibleTargets[i].name}`");
                        AssignTask(new AttackTask(possibleTargets[i]));
                        return;
                    }

                    break;
            }
            

           
            base.TriggerDefaultBehavior();
        }
    }
}