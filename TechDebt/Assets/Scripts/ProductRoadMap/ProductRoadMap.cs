using System;
using System.Collections.Generic;
using NPCs;
using Stats;
using UnityEditor.SceneManagement;
using UnityEngine;

public class ProductRoadMap
{
    public List<ProductRoadMapStage> Stages { get; set; }
    public int CurrentStage { get; protected set; } = 0;

    public void Randomize()
    {
        Stages = new List<ProductRoadMapStage>();
        // Level 1 is just launch
        ProductRoadMapStage Stage;
        Stage = new ProductRoadMapStage(Stages.Count);
        Stage.Levels.Add(new LaunchProductRoadMapLevel());
        Stages.Add(Stage);
        
        Stage = new ProductRoadMapStage(Stages.Count);
        Stage.Levels.Add(new MobileProductRoadMapLevel());
        Stage.Levels.Add(new EmailProductRoadMapLevel());
        Stages.Add(Stage);
        
        Stage = new ProductRoadMapStage(Stages.Count);
        Stage.Levels.Add(new SecurityAuditProductRoadMapLevel());
        Stages.Add(Stage);
        
        Stage = new ProductRoadMapStage(Stages.Count);
        Stage.Levels.Add(new SocketChatProductRoadMapLevel());
        Stage.Levels.Add(new GeoLocationProductRoadMapLevel());
        Stages.Add(Stage);
        foreach (ProductRoadMapStage nextStage in Stages)
        {
            nextStage.Randomize();
        }
        
        
    }

    public ProductRoadMapLevel GetCurrentLevel()
    {
        return Stages[CurrentStage].GetSelectedLevel();
    }

   
}
public class ProductRoadMapStage
{
    public int SelectedLevel { get; protected set; } = -1;
    public List<ProductRoadMapLevel>  Levels { get; set; } = new List<ProductRoadMapLevel>();
    public int Stage { get; protected set; }
    public ProductRoadMapStage(int _stage)
    {
        Stage = _stage;
    }

    public void Randomize()
    {
        foreach (ProductRoadMapLevel level in Levels)
        {
            level.Randomize(Stage);
        }   
    }
    public ProductRoadMapLevel SetSelectedLevel(int level)
    {
        Debug.Log($"SetSelectedLevel - {level}");
        SelectedLevel = level;
        return Levels[SelectedLevel];
    }

    public ProductRoadMapLevel GetSelectedLevel()
    {
        if (SelectedLevel == -1)
        {
            throw new SystemException("Selected level is invalid.");
        }
        return Levels[SelectedLevel];
    }
}

public class ProductRoadMapLevel
{
   public string Name { get; set; }
   public string SpriteId { get; set; }
   public int SprintDuration { get; set; } = 5;
   // TODO Stake holder? Sales, PR, etc?
   //TODO Add in modifiers and rewards
   List<ModifierBase> Modifiers { get; set; } = new List<ModifierBase>();

   public virtual bool HasVictoryConditionBeenMet()
   {
       return false;
   }

   public virtual void Randomize(int stage = 0)
   {
       // Chose random modifiers based on stage
       for (int i = 0; i < stage; i++)
       {
           
       }
   }

   public virtual bool IsLaunchDay()
   {
       return SprintDuration == GameManager.Instance.GameLoopManager.currentDay;
   }

   public virtual void PlanPhaseCheck()
   {
       if (IsLaunchDay())
       {
           OnLaunchDayPlan();
       } else if (GameManager.Instance.GameLoopManager.currentDay == 0)
       {
           OnStartDayPlan();
       }
      
   }

   public virtual void OnStartDayPlan()
   {
       
   } 
   public virtual void OnLaunchDayPlan()
   {
       
   }
   public virtual void SummaryPhaseCheck()
   {
       if (GameManager.Instance.GetStat(StatType.Money) < 0)
       {
           EndGame();
           return;
       }
       if (IsLaunchDay())
       {
           OnLaunchDaySummary();
       }
      
   }
   public virtual void OnLaunchDaySummary()
   {
       throw new NotImplementedException();
   }

   public virtual void OnLaunchDay()
   {
       throw new NotImplementedException();
   }

   public virtual string GetDescription()
   {
       string res = $"{Name}";
       // TODO: Add the modifiers here
       return res;
   }

   public virtual void EndGame()
   {
       GameManager.Instance.UIManager.SetTimeScalePause();
       NPCBase bossNPC = GameManager.Instance.AllNpcs.Find((npc) => npc.GetComponent<BossNPC>() != null);
       GameManager.Instance.cameraController.ZoomToAndFollow(bossNPC.transform);
       GameManager.Instance.UpdateMetaProgress();
       GameManager.Instance.UIManager.ShowNPCDialog(
           GameManager.Instance.SpriteManager.GetSprite("Suit1NPC"),
           "You have failed to keep our infrastructure up and running with in our budget. You are fired!",
           new List<DialogButtonOption>()
           {
               new DialogButtonOption() { Text = "Start Over", OnClick = () =>
                   {
                        
                       GameManager.Instance.Reset();
                   }
               }/*,
               new DialogButtonOption() { Text = "Main Menu", OnClick = () =>
                   {
                        
                   }
               },*/
           }
       );
   }
}
