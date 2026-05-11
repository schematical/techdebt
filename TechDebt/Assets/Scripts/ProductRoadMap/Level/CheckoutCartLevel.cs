using UnityEngine;

public class CheckoutCartLevel : MapLevel
{
    public CheckoutCartLevel() : base()
    {
        Name = "Checkout Cart";
        SpriteId = "IconCart";
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
        return "Build a robust checkout experience for customers.";
    }
}