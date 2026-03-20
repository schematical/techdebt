using Infrastructure;
using Tutorial;
using UnityEngine;

public class WhiteBoardWOType : WorldObjectType
{
    public WhiteBoardWOType()
    {
        type = WorldObjectType.Type.WhiteBoard;
        DisplayName = "White Board";
        BuildTime = 30;
        TutorialStepId = TutorialStepId.Infra_WhiteBoard_Tip;
    }
}
