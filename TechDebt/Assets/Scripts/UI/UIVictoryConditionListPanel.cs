using System.Collections.Generic;
using UnityEngine;

namespace UI
{
    public class UIVictoryConditionListPanel: UIPanel
    {
        public override void Show()
        {
            Debug.Log("Showing UIVictoryConditionListPanel");
            runUICloseOnShow = false;
            Refresh();
            base.Show();
            
        }

        public void Refresh()
        {
            switch (panelState)
            {
                case(UIState.Closed):
                case(UIState.Closing):
                    return;
            }
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