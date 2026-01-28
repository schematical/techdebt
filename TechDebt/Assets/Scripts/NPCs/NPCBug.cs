using System.Collections.Generic;
using Items;
using UnityEngine;

namespace NPCs
{
    public class NPCBug : NPCBase
    {
        public enum Level { Minor, Medium, Major, Critical }
        public override bool CanAssignTask(NPCTask task)
        {
            return task is BugConsumeItemTask;
        }

        public override void Initialize()
        {
            base.Initialize();
            Stats.Get(StatType.NPC_MovmentSpeed).SetBaseValue(1.5f);
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
            Debug.Log("NPCBug TriggerDefaultBehavior");
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
                Debug.Log("NPCBug TriggerDefaultBehavior 2");
                AssignTask(new BugConsumeItemTask(targetItem));
                return;
            }
            Debug.Log("NPCBug TriggerDefaultBehavior 3");
            base.TriggerDefaultBehavior();
        }
    }
}