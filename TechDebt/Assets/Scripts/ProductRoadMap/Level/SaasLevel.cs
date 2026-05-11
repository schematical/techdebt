using UnityEngine;

public class SaasLevel : MapLevel
{
    public SaasLevel() : base()
    {
        Name = "SaaS Subscription Model";
        SpriteId = "IconMoney";
        RequiredStakeholderId = "cmo";
        DependencyIds.Add("OnlinePaymentsProductRoadMapLevel");
        UnlockConditions.Add(new UnlockCondition()
        {
            Type = UnlockCondition.ConditionType.Stakeholder,
            Level = 1,
            TargetId = "cmo"
        });
    }

    public override string GetDescription()
    {
        return "Transition to a subscription-based revenue model. Requires an Accountant.";
    }
}