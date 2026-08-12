using UnityEngine;
using System.Linq;
using DefaultNamespace;
using Infrastructure;
using NPCs;
using UI;
using UnityEngine.EventSystems;

public class NPCStakeholder : NPCAnimatedBiped
{
    public WorldObjectBase home;
    public Stakeholder stakeholder;

    public void Initialize(Stakeholder stakeholder, WorldObjectBase home)
    {
        bodyTypeId = "Suit";
        base.Initialize();
        Randomize();
        this.stakeholder = stakeholder;
        Id = stakeholder.Id;
        this.home = home;
    

    }
    public override void OnLeftClick(PointerEventData eventData)
    {
        // GameManager.Instance.UIManager.npcDetailPanel.Show(this);
        RenderDetailBubble();
    }
    public virtual UIDialogBubble RenderDetailBubble()
    {
        UIDialogBubble dialogBubble = ShowDialogBubble();
        dialogBubble.AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().h1($"Hi! I am the {stakeholder.DisplayName}. \nHaving me on staff unlocks additional quests accessible via the product road map.");
        dialogBubble.AddButton("Continue", () => dialogBubble.Close());
        GameManager.Instance.UIManager.MarkDialogBubbleFocused(dialogBubble);
        return dialogBubble;
    }
    public override void TriggerDefaultBehavior()
    {
        
        // If we are already at the desk, do nothing.
        if (Vector3.Distance(transform.position, home.GetInteractionPosition()) < 0.1f)
        {
            FaceDown();
            return; 
        }
        

        MoveTo(home.GetInteractionPosition());
        CurrentState = State.Wandering; // Use Wandering state to signify moving without a task
     
    }
    public override bool CanAssignTask(NPCTask task)
    {
        return task.Role == NPCTask.TaskRole.Boss;
    }
    public override Vector3 GetHomePoint()
    {
        return home.GetInteractionPosition();
    }
}
