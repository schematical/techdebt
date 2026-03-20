using Infrastructure;
using Tutorial;
using UnityEngine;

public class ProductRoadMapWOType : WorldObjectType
{
    public ProductRoadMapWOType()
    {
        type = WorldObjectType.Type.ProductRoadMap;
        DisplayName = "Product Road Map";
        BuildTime = 30;
        TutorialStepId = TutorialStepId.Infra_ProductRoadMap_Tip;
    }
}
