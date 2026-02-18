using System.Collections.Generic;
using UnityEngine;

namespace NPCs
{
    public class NPCXSS: NPCBase
    {
        public override void Initialize()
        {
            base.Initialize();
            shadow.gameObject.SetActive(false);
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
            if (!CanTakeAction(CoolDownType.Attack))
            {
                base.TriggerDefaultBehavior();
                return;
            }
            List<ApplicationServer> applicationServers =
                GameManager.Instance.GetInfrastructureInstanceByClass<ApplicationServer>().FindAll((infra) =>
                {
                    return infra.IsActive();
                });
            if (applicationServers.Count == 0)
            {
                // throw new System.Exception("No target: There are " + applicationServers.Count + " infrastructure instances for this NPC.");
                base.TriggerDefaultBehavior();
                return;
            }
            int i = Random.Range(0, applicationServers.Count);
            ApplicationServer applicationServer = applicationServers[i];
            AssignTask(new RedirectTrafficTask(applicationServer));
            

        }
    }
}