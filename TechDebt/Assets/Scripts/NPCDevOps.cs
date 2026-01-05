// NPCDevOps.cs
using UnityEngine;
using UnityEngine.EventSystems;

public class NPCDevOps : NPCBase, IPointerClickHandler
{
    public NPCDevOpsData Data { get; private set; }

    public void Initialize(NPCDevOpsData data)
    {
        Data = data;
        gameObject.name = $"NPCDevOps_{Data.Name}";
        base.Initialize();
    }
    
    protected override void Update()
    {
        // The base class now handles all the task logic.
        // We just need to ensure it only runs during the Play phase.
        if (GameManager.Instance.GameLoopManager.CurrentState == GameLoopManager.GameState.Play ||
            GameManager.Instance.GameLoopManager.CurrentState == GameLoopManager.GameState.WaitingForNpcsToExpire)
        {
            base.Update();
        }
    }
    
    public override bool CanAssignTask(NPCTask task)
    {
        return true;
    }

    public float GetResearchPointsPerSecond(Technology technology)
    {
        // This could be influenced by the NPC's skills or the technology type
        return 1f;
    }
    
    public void OnBuildPhaseStart()
    {
        if (CurrentTask != null)
        {
            CurrentTask.Unassign();
        }
        StopMovement();
        CurrentState = State.Idle;
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        GameManager.Instance.UIManager.ShowNPCDetail(this);
    }
}
