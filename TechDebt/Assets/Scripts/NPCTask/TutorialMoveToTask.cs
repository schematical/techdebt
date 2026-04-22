using Items;
using Tutorial;
using UnityEngine;

public class TutorialMoveToTask : NPCTask
{
    private TutorialStep tutorialStep;
    private bool hasTriggeredCloseEnough = false;
    public TutorialMoveToTask(TutorialStep tutorialStep) : base(tutorialStep.getTarget())
    {
        this.tutorialStep = tutorialStep;
        Role = TaskRole.SchematicalBot;
        toastComplete = false;
        Priority = 10; // High priority
        interactionType = InteractionType.Explain;
        maxTaskRange = 0.25f;
        Debug.Log($"TutorialMoveToTask constructed: {tutorialStep.Id} - {tutorialStep.getTarget().name} - {tutorialStep.getTarget().GetInteractionPosition(interactionType)} - {interactionType}");
    }

    public override void OnUpdate(NPCBase npc)
    {
       
        if (IsCloseEnough())
        {
            if (!hasTriggeredCloseEnough)
            {
                hasTriggeredCloseEnough = true;
                tutorialStep.Render();
            }
            return;
        }
        npc.MoveTo(target.GetInteractionPosition(interactionType));
    }

    public override bool IsFinished(NPCBase npc)
    {       
        
        return tutorialStep.State == TutorialStep.TutorialStepState.Completed;
    }

    public override void OnEnd(NPCBase npc)
    {       
        Debug.Log($"{GetType().Name}::{tutorialStep.Id} IsFinished - {tutorialStep.State} = {tutorialStep.State == TutorialStep.TutorialStepState.Completed}");

        base.OnEnd(npc);
        CurrentState = State.Completed;
    }
    public override string GetDescription()
    {
        string description = $"{GetType()} - State: {CurrentState} " +
                             $"{target.name} - Tutorial Step: {tutorialStep.Id} - {tutorialStep.State}";
        if (AssignedNPC != null) {
            description += $"isCloseEnough`: {IsCloseEnough()} - " +
                           $" Dist: {Vector3.Distance(target.GetInteractionPosition(), AssignedNPC.transform.position)} Range: {maxTaskRange}";
        }
        return description;
    }
}