using System.Collections.Generic;

namespace UI
{
    public class UIVictoryConditionListPanel: UIPanel
    {
        public void Refresh()
        {
            CleanUp();
            List<MapLevelVictoryConditionBase> victoryConditions =
                GameManager.Instance.Map.GetCurrentLevel().VictoryConditions;
            foreach (MapLevelVictoryConditionBase victoryCondition in victoryConditions)
            {
                victoryCondition.Render(this);
            }
        }
    }
}