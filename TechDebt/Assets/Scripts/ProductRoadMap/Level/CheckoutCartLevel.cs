using UnityEngine;

public class CheckoutCartLevel : MapLevel
{
    public CheckoutCartLevel() : base()
    {
        Name = "Checkout Cart";
        SpriteId = "IconCart";
        RequiredStakeholderId = "cmo";
        DependencyIds.Add("OnlinePaymentsProductRoadMapLevel");
    }

    public override string GetDescription()
    {
        return "Build a robust checkout experience for customers. Requires a Marketing Associate.";
    }
}