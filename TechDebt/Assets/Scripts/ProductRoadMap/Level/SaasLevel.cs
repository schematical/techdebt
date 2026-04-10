using UnityEngine;

public class SaasLevel : MapLevel
{
    public SaasLevel() : base()
    {
        Name = "SaaS Subscription Model";
        SpriteId = "IconMoney";
        RequiredStakeholderId = "cfo";
        DependencyIds.Add("LaunchMapLevel");
    }

    public override string GetDescription()
    {
        return "Transition to a subscription-based revenue model. Requires an Accountant.";
    }
}