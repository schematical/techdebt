using Items;
using Tutorial;
using UnityEngine;

public class TutorialMoveToTask : NPCTask
{
    private TutorialStep tutorialStep;
    public TutorialMoveToTask(TutorialStep tutorialStep) : base(tutorialStep.getTarget())
    {
        this.tutorialStep = tutorialStep;
        Priority = 10; // High priority
        Debug.Log("TutorialMoveToTask constructed.");
    }

    public override void OnUpdate(NPCBase npc)
    {
        Debug.Log("TutorialMoveToTask:OnUpdate.");
        if (IsCloseEnough())
        {
          
                Debug.LogWarning("Introduce stuff.");
            
 
        }
    }

    public override bool IsFinished(NPCBase npc)
    {       
        Debug.Log($"{GetType().Name}:: IsFinished - {tutorialStep.State} = {tutorialStep.State == TutorialStep.TutorialStepState.Completed}");
        return tutorialStep.State == TutorialStep.TutorialStepState.Completed;
    }

    public override void OnEnd(NPCBase npc)
    {       
        Debug.Log($"{GetType().Name}:: OnEnd");
        base.OnEnd(npc);
        CurrentState = State.Completed;
    }
}