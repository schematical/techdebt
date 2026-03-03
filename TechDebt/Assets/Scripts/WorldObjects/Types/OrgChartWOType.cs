using Infrastructure;
using UnityEngine;

public class OrgChartMapWOType : WorldObjectType
{
    public OrgChartMapWOType()
    {
        type = WorldObjectType.Type.OrgChart;
        DisplayName = "Org Chart";
        BuildTime = 30;
    }
}
