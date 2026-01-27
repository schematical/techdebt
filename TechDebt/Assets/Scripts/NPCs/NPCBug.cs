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