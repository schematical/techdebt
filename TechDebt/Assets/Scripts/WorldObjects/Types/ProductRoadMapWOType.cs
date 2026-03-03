using Infrastructure;
using UnityEngine;

public class ProductRoadMapWOType : WorldObjectType
{
    public ProductRoadMapWOType()
    {
        type = WorldObjectType.Type.ProductRoadMap;
        DisplayName = "Product Road Map";
        BuildTime = 30;
    }
}
