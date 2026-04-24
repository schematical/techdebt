using System.Collections.Generic;
using Tutorial;
using UnityEngine;
using UnityEngine.EventSystems;

namespace NPCs
{
    public class NPCFishingAttack: NPCBase
    {
        public override void Initialize()
        {
            base.Initialize();
            // tutorialStepId = TutorialStepId.NPC_XSS_View;
            shadow.gameObject.SetActive(false);
            spriteRenderer.sprite = GameManager.Instance.SpriteManager.GetSprite("FishingAttack", "0");
        }

        public override List<NPCTask> GetAvailableTasks()
        {
            List<NPCTask> tasks = new List<NPCTask>();
            switch (CurrentState)
            {
                case(State.Dead):
                    break;
                default:
                    AttackTask task = new AttackTask(this);
                    task.assignButtonText = "Stop Fishing Attack";
                    tasks.Add(task);
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
            if (!CanTakeAction(CoolDownType.Attack))
            {
                base.TriggerDefaultBehavior();
                return;
            }
            List<NPCBase> npcs =
                GameManager.Instance.AllNpcs.FindAll((npc => npc is NPCDevOps));
 
            int i = Random.Range(0, npcs.Count);
            NPCBase npc = npcs[i];
            AssignTask(new FishingTask(npc));
            

        }

        public void MarkReturning()
        {
            spriteRenderer.sprite = GameManager.Instance.SpriteManager.GetSprite("FishingAttack", "1");
        }
    }
}