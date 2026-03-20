using Infrastructure;
using Tutorial;
using UnityEngine;

public class OrgChartMapWOType : WorldObjectType
{
    public OrgChartMapWOType()
    {
        type = WorldObjectType.Type.OrgChart;
        DisplayName = "Org Chart";
        BuildTime = 30;
        TutorialStepId = TutorialStepId.Infra_OrgChart_Tip;
    }
}
