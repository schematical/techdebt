using System.Collections.Generic;
using UnityEngine.UI;

namespace UI
{
    public class SummaryData
    {
        public int Day;
        public float PacketsTotal;
        public float PacketsFailed;
        public float PacketsSucceeded;
        public float PercentageServed;
        public float TotalCosts;
        public float TotalIncome;
        public float NetIncome;
        public float TotalMoney;
        public float AttackPossibility;
        public List<string> VictoryConditions = new List<string>();
        public List<KeyValuePair<string, float>> InfraCosts = new List<KeyValuePair<string, float>>();
    }

    public class UISummaryPhasePanel: UIPanel
    {
     

        void Start()
        {
        
        }

        public void ShowSummary(List<MapLevelVictoryConditionBase> victoryConditions)
        {
            CleanUp();
            GameManager.Instance.UIManager.Block();
            base.Show();
            AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().h1("Game Over");
            AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().h2("Failed Victory Conditions");
            foreach (MapLevelVictoryConditionBase condition in victoryConditions)
            {
                if (condition.GetFinalState() == VictoryConditionState.Failed)
                {
                    AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = $" - {condition.GetDescription()}";
               
                }
            }
          
            AddButton("Start Over", () => { GameManager.Instance.StartNewGame(); });
            AddButton("Main Menu", () => { GameManager.Instance.ShowSaveSlotDetailPanel(); });
       
        }
        
    }
}
