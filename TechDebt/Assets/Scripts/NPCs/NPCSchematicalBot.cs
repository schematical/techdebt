using UnityEngine;
using UnityEngine.EventSystems;


namespace NPCs
{
    public class NPCSchematicalBot: NPCBase
    {
        private InfrastructureInstance _hangOutAt;
        protected float idleDuration = 0;

        public override void Initialize()
        {
            base.Initialize();
            _hangOutAt = GameManager.Instance.GetInfrastructureInstanceByID("whiteboard");
        }

        public override void OnLeftClick(PointerEventData eventData)
        {
            base.OnLeftClick(eventData);
            // GameManager.Instance.UIManager.tutorialStepListPanel.Show();
        }

        public override void TriggerDefaultBehavior()
        {
            if (IsDialogBubbleActive())
            {
                return;
            }
            idleDuration += Time.fixedDeltaTime;
            if (idleDuration > 3)
            {
                CurrentState = State.Idle; // Use Wandering state to signify moving without a task
                animator.SetBool("isExiting", true);
            }
       
        }

     

        public override void AssignTask(NPCTask task)
        {
            idleDuration = 0;
            base.AssignTask(task);
        }

        public void ExitAnimationHasFinished()
        {
            Debug.Log("SchematicalBot::ExitAnimationHasFinished");
            gameObject.SetActive(false);
            animator.SetBool("isExiting", false);
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