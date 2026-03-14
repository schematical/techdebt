using System;
using DefaultNamespace;
using DefaultNamespace.Rewards;
using NPCs;
using Stats;
using UnityEngine;

namespace UI
{
    public class UIStatCollectionPaneStatDetailLine: UIPanelLine
    {
        protected StatData statData;
        public UIPanelLineSectionText mainText;
        public void SetStatData(StatData _statData)
        {
   
            statData = _statData;
            mainText = Add<UIPanelLineSectionText>();
            ResetText();
            if (statData.Modifiers.Count == 0)
            {
                return;
            }
            SetExpandable((UIPanelLine statLine) =>
            {
           
                foreach (StatModifier modifier in statData.Modifiers)
                {
                    UIPanelLine modifierLine = statLine.AddLine<UIPanelLine>();
                    modifierLine.Add<UIPanelLineSectionText>().text.text = modifier.GetDisplayText();
                }
            
            });
  
         
   
        }

        public void ResetText()
        {
            mainText.text.color = Color.white;
            mainText.text.text = statData.GetDescription();
        }

        public override string GetId()
        {
            return statData.Type.ToString();
        }

        public void Preview(StatModifierReward modifierBase)
        {
            float updatedValue = 0;
         
            StatModifier statModifier = modifierBase.BuildStatModifier();
            updatedValue = statData.PreviewValue(statModifier);
            mainText.text.text = statData.GetPreviewText(statModifier);
        
            
            
            if (Math.Abs(updatedValue - statData.Value) < 0.00001f)
            {
                mainText.text.color = Color.yellow;
            } else if (updatedValue > statData.Value)
            {
                switch (modifierBase.ScaleDirection)
                {
                    case(ScaleDirection.Up):
                        mainText.text.color = Color.green;
                        break;
                    case(ScaleDirection.Down):
                        mainText.text.color = Color.red;
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
            else
            {
                switch (modifierBase.ScaleDirection)
                {
                    case(ScaleDirection.Up):
                        mainText.text.color = Color.red;
                        break;
                    case(ScaleDirection.Down):
                        mainText.text.color = Color.green;
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
        }
    }
}