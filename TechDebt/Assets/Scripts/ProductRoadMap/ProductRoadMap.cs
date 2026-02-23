using System.Collections.Generic;

public class ProductRoadMap
{
    public List<ProductRoadMapStage> Stages { get; set; }

    public void Randomize()
    {
        Stages = new List<ProductRoadMapStage>();
        // Level 1 is just launch
        ProductRoadMapStage Stage;
        Stage = new ProductRoadMapStage();
        Stage.Levels.Add(new LaunchProductRoadMapLevel());
        Stages.Add(Stage);
        
        Stage = new ProductRoadMapStage();
        Stage.Levels.Add(new MobileProductRoadMapLevel());
        Stage.Levels.Add(new EmailProductRoadMapLevel());
        Stages.Add(Stage);
        
        Stage = new ProductRoadMapStage();
        Stage.Levels.Add(new SecurityAuditProductRoadMapLevel());
        Stages.Add(Stage);
        
        Stage = new ProductRoadMapStage();
        Stage.Levels.Add(new SocketChatProductRoadMapLevel());
        Stage.Levels.Add(new GeoLocationProductRoadMapLevel());
        Stages.Add(Stage);
    }
}
public class ProductRoadMapStage
{
    public List<ProductRoadMapLevel>  Levels { get; set; } = new List<ProductRoadMapLevel>();
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
