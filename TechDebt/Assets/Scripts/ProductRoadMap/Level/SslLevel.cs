using UnityEngine;

public class SslLevel : MapLevel
{
    public SslLevel() : base()
    {
        Name = "SSL Implementation";
        SpriteId = "IconLock";
        RequiredStakeholderId = "ciso";
        Direction = MapNodeDirection.Left;
        DependencyIds.Add("LaunchMapLevel");
        AddCashReward(100, 300);// TODO scale this with difficulty.
        AddPrestigePointsReward();
        UnlockConditions.Add(new UnlockCondition()
        {
            Type = UnlockCondition.ConditionType.Stakeholder,
            Level = 0,
            TargetId = "ciso"
        });
        UnlockConditions.Add(new UnlockCondition()
        {
            Type = UnlockCondition.ConditionType.Stakeholder,
            Level = 0,
            TargetId = "ciso"
        });
    }

    public override string GetDescription()
    {
        return "Implement SSL to protect against Man-in-the-Middle attacks. Requires a Security Officer.";
    }
}