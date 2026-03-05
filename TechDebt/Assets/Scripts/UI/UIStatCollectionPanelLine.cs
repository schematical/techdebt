using System.Collections.Generic;
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
            Debug.Log("UIStatCollectionPanelLine.SetStatCollection");
            statCollection = _statCollection;
          
            UIPanelLineSectionText textSection = Add<UIPanelLineSectionText>();
            textSection.text.text = title;
            // textSection.text.fontSize = 24;
            

            SetExpandable(OnCollectionExpand);
        }

        private void OnCollectionExpand(UIPanelLine line)
        {
            foreach (StatData statData in statCollection.Stats.Values)
            {
                UIPanelLine statLine = AddLine<UIPanelLine>();
                statLine.Add<UIPanelLineSectionText>().text.text = statData.GetDescription();
                if (statData.Modifiers.Count == 0)
                {
                    continue;
                }
                statLine.SetExpandable((UIPanelLine statLine) =>
                {
           
                    foreach (StatModifier modifier in statData.Modifiers)
                    {
                        UIPanelLine modifierLine = statLine.AddLine<UIPanelLine>();
                        modifierLine.Add<UIPanelLineSectionText>().text.text = modifier.GetDisplayText();
                    }
            
                });

            }
        }
        
    }
}