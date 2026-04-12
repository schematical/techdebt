using System.Collections.Generic;

namespace UI
{
    public class UIVictoryConditionListPanel: UIPanel
    {
        public override void Show()
        {
            runUICloseOnShow = false;
            Refresh();
            base.Show();
            
        }

        public void Refresh()
        {
            CleanUp();
            List<MapLevelVictoryConditionBase> victoryConditions =
                GameManager.Instance.Map.GetCurrentLevel().GetCombinedVictoryConditions();
            foreach (MapLevelVictoryConditionBase victoryCondition in victoryConditions)
            {
                victoryCondition.Render(this);
            }
        }
    }
}