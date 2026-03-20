using UnityEngine;
using UnityEngine.EventSystems;


namespace NPCs
{
    public class NPCSchematicalBot: NPCBase
    {
        private InfrastructureInstance _hangOutAt;

        public override void Initialize()
        {
            base.Initialize();
            _hangOutAt = GameManager.Instance.GetInfrastructureInstanceByID("whiteboard");
            if (_hangOutAt == null)
            {
                Debug.LogError("BossNPC: Could not find 'boss-desk'. Boss will wander.");
            }
        }

        public override void OnLeftClick(PointerEventData eventData)
        {
            base.OnLeftClick(eventData);
            // GameManager.Instance.UIManager.tutorialStepListPanel.Show();
        }

        public override void TriggerDefaultBehavior()
        {
     
            if (Vector3.Distance(transform.position, _hangOutAt.GetInteractionPosition()) < 0.1f)
            {
                return; 
            }
            

            MoveTo(_hangOutAt.GetInteractionPosition());
            CurrentState = State.Idle; // Use Wandering state to signify moving without a task
     
        }
        public override bool CanAssignTask(NPCTask task)
        {
            return task.Role == NPCTask.TaskRole.SchematicalBot;
        }
        public override Vector3 GetHomePoint()
        {
            return GameManager.Instance.GetInfrastructureInstanceByID("desk").transform.position;
        }
    }
}