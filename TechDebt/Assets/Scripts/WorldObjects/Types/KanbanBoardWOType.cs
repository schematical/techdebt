using Infrastructure;
using Tutorial;
using UnityEngine;

public class KanbanBoardWOType : WorldObjectType
{
    public KanbanBoardWOType()
    {
        type = WorldObjectType.Type.KanbanBoard;
        DisplayName = "KanbanBoard";
        BuildTime = 30;
        TutorialStepId = TutorialStepId.Infra_KanbanBoard_Tip;
    }
}
