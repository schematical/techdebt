using System.Collections.Generic;
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

        public void Preview(ModifierBase modifierBase)
        {
            if (!IsExpanded())
            {
                Debug.Log("Expanding");
                Expand();
            }
            else
            {
                Debug.Log("Already is expanded");
            }

            foreach (UIPanelLine line in lines)
            {
                // Find the right line
                if(line.GetId() == modifierBase.StatType.ToString()) 
                {
                    (line as UIStatCollectionPaneStatDetailLine).Preview(modifierBase);
                }
                else
                {
                    (line as UIStatCollectionPaneStatDetailLine).ResetText();
                }
            }
        }
    }
}