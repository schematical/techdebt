using System.Collections.Generic;
using UnityEngine;

namespace UI
{
    public class UIVictoryConditionListPanel: UIPanel
    {
        public override void Show()
        {
            runUICloseOnShow = false;
            base.Show();
            Refresh();
            
        }

        public void Refresh()
        {
            if (GameManager.Instance.Map == null)
            {
                return;
            }
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

        public override void Close(bool forceClose = false)
        {
            Debug.Log("UIVictoryConditionListPanel::Close");
            base.Close(forceClose);
        }
    }
}