// NPCDevOps.cs

using System;
using System.Collections.Generic;
using NPCs;
using Stats;
using UnityEngine;
using UnityEngine.EventSystems;

public class NPCDevOps : NPCBase, IPointerClickHandler
{
    public NPCDevOpsData Data { get; private set; }
    public List<NPCTrait> Traits { get; private set; } = new List<NPCTrait>();
    public int level = 1;
    public int lastDisplayXP = 0;
    public float currentXP = 0;

    public void Initialize(NPCDevOpsData data)
    {
        Data = data;
        gameObject.name = $"NPCDevOps_{Data.Name}";
        base.Initialize();
        Stats.Add(new StatData(StatType.NPC_TraitSlots, 1));
        
    }
    
    protected override void Update()
    {
        // The base class now handles all the task logic.
        // We just need to ensure it only runs during the Play phase.
       
            base.Update();
            
        
    }
    public override void AddXP(float amount = 1)
    {
       currentXP += amount;
       if (Math.Floor(currentXP) != lastDisplayXP)
       {
           GameManager.Instance.FloatingTextFactory.ShowText($"{Math.Ceiling(amount)} XP",
               transform.position); //  + new Vector3(0, 1, 3));
           lastDisplayXP = (int)Math.Floor(currentXP);
       }

       int nextLevel = (int) Math.Round(20 * Math.Pow(1.5f, level));
       if (currentXP >= nextLevel)
       {
           level ++;
           currentXP = 0;
           lastDisplayXP = 0;
           Sprite sprite = GameManager.Instance.prefabManager.GetPrefab("Manual").GetComponent<SpriteRenderer>().sprite;
           int saftyCheck = 0;
           List<NPCTrait> traits = new List<NPCTrait>();
           while (
               saftyCheck < 20 &&
               traits.Count < 3
           )
           {
               NPCTrait trait = GameManager.Instance.GetRandomNPCTrait();
               if (!traits.Contains(trait))
               {
                   traits.Add(trait);
                   GameManager.Instance.UIManager.MultiSelectPanel.Add(
                           trait.Name, 
                           sprite, 
                           trait.GetDisplayText() 
                           )
                       .OnClick((string id) =>
                       {
                           AddTrait(trait);
                           GameManager.Instance.UIManager.MultiSelectPanel.Clear();
                       });
               }

               saftyCheck++;
           }
       }
    }
    public void AddTrait(NPCTrait trait)
    {
        Traits.Add(trait);
        
    }
    public override bool CanAssignTask(NPCTask task)
    {
        return task.Role == NPCTask.TaskRole.DevOps;
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
