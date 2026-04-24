using UnityEngine;

public class CodePipelineLevel : MapLevel
{
    public CodePipelineLevel() : base()
    {
        Name = "Code Pipeline Level";
        SpriteId = "IconDisk";
        RequiredStakeholderId = "cto";
        Direction = MapNodeDirection.Down;
        DependencyIds.Add("LaunchMapLevel");
        VictoryConditions.Add(new InfraActiveVictoryCondition()
        {
            TargetId = "codepipeline"
        });
        AddCashReward(100, 250);
        AddPrestigePointsReward(2);
    }

    public override string GetDescription()
    {
        return "Unlock code pipeline to automate deployment of software releases.";
    }
}