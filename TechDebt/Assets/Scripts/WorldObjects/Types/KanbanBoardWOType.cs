using Infrastructure;
using UnityEngine;

public class KanbanBoardWOType : WorldObjectType
{
    public KanbanBoardWOType()
    {
        type = WorldObjectType.Type.KanbanBoard;
        DisplayName = "KanbanBoard";
        BuildTime = 30;
    }
}
