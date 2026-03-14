using System.Collections.Generic;
using DefaultNamespace.Rewards;
using NPCs;
using Stats;
using TMPro;
using UnityEngine;

namespace UI
{
    public class UIStatCollectionPanelLine: UIPanelLine
    {
        protected StatsCollection  statCollection;
        public void SetStatCollection(StatsCollection _statCollection, string title = "Stats")
        {
  
            statCollection = _statCollection;
          
            UIPanelLineSectionText textSection = Add<UIPanelLineSectionText>();
            textSection.text.text = title;
         
            SetExpandable(OnCollectionExpand);
        }

        private void OnCollectionExpand(UIPanelLine line)
        {
            foreach (StatData statData in statCollection.Stats.Values)
            {
                UIStatCollectionPaneStatDetailLine statLine = AddLine<UIStatCollectionPaneStatDetailLine>();
                statLine.SetStatData(statData);
                

            }
        }

        public void Preview(RewardBase modifierBase)
        {
            if (!IsExpanded())
            {
                Expand();
            }

            foreach (UIPanelLine line in lines)
            {
                // Find the right line
                if (modifierBase is StatModifierReward)
                {
                    StatModifierReward statModifierReward = (StatModifierReward)modifierBase;
                    if (line.GetId() == statModifierReward.StatType.ToString())
                    {
                        (line as UIStatCollectionPaneStatDetailLine).Preview(statModifierReward);
                    }
                    else
                    {
                        (line as UIStatCollectionPaneStatDetailLine).ResetText();
                    }
                }
            }
        }
    }
}