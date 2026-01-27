using System.Collections.Generic;
using Items;
using UnityEngine;

namespace NPCs
{
    public class NPCBug : NPCBase
    {
        public override bool CanAssignTask(NPCTask task)
        {
            return task is BugConsumeItemTask;
        }

        public override void Initialize()
        {
            base.Initialize();
            Stats.Get(StatType.NPC_MovmentSpeed).SetBaseValue(1.5f);
            Debug.Log($"NPCBug Initialize {Stats.GetStatValue(StatType.NPC_MovmentSpeed)}");
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
            
            base.TriggerDefaultBehavior();
        }
    }
}