using System.Collections.Generic;

public class ProductRoadMap
{
    public List<ProductRoadMapStage> Stages { get; set; }

    public void Randomize()
    {
        // Level 1 is just launch
        Stages[0] = new ProductRoadMapStage();
        Stages[0].Levels.Add(new LaunchProductRoadMapLevel());
        
        Stages[1] = new ProductRoadMapStage();
        Stages[1].Levels.Add(new MobileProductRoadMapLevel());
        Stages[1].Levels.Add(new EmailProductRoadMapLevel());
        
        Stages[2] = new ProductRoadMapStage();
        Stages[2].Levels.Add(new SecurityAuditProductRoadMapLevel());
    }
}
public class ProductRoadMapStage
{
    public List<ProductRoadMapLevel>  Levels { get; set; }
}

public class ProductRoadMapLevel
{
   public string Name { get; set; }
   public string SpriteId { get; set; }
   public int SprintDuration { get; set; } = 5;

   public virtual bool HasVictoryConditionBeenMet()
   {
       return false;
   }
}
