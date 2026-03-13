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

        public void ShowSummary(SummaryData data)
        {
            CleanUp();
            base.Show();

            AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().h1($"End of Day {data.Day}");
            
            AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = $"Total Packets: {data.PacketsTotal}";
            AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = $"Packets Failed: {data.PacketsFailed}";
            AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = $"Packets Succeeded: {data.PacketsSucceeded}";
            AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = $"Percentage Served: %{data.PercentageServed:F2}";
            AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = $"Total Costs: ${data.TotalCosts}";
            AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = $"Total Income: ${data.TotalIncome}";
            AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = $"Net Income: ${data.NetIncome}";
            AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = $"Total: {data.TotalMoney}";
            AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = $"Tomorrow's Attack Possibility: {data.AttackPossibility:F2}%";

            // Victory Conditions
            AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = "\n<b>Victory Conditions:</b>";
            foreach (string condition in data.VictoryConditions)
            {
                AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = $"  - {condition}";
            }

            // Infrastructure Costs
            if (data.InfraCosts.Count > 0)
            {
                AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = "\n<b>Infrastructure Costs:</b>";
                
                foreach (var kvp in data.InfraCosts)
                {
                    AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = $"{kvp.Key}: ${kvp.Value}";
                }
            }
            AddButton("Continue", OnContinue);
        }

    
        private void OnContinue()
        {
            GameManager.Instance.GameLoopManager.PostSummaryCheck();
            Close();
        }
    }
}
