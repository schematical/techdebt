using System;
using System.Collections.Generic;
using NPCs;
using Stats;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

namespace UI
{
    public class UIGlobalStatsPanel: UIPanel
    {
        protected UIStatCollectionPanelLine statCollectionPanelLine;
        protected UIStatCollectionPanelLine networkLine;
        void Start()
        {
           
        }
        // ReSharper disable Unity.PerformanceAnalysis
        public override void Show()
        {

            if (!IsOpen())
            {
                base.Show();
            }
           
            CleanUp();

            statCollectionPanelLine = AddLine<UIStatCollectionPanelLine>();
            statCollectionPanelLine.SetStatCollection(GameManager.Instance.Stats);
            networkLine = AddLine<UIStatCollectionPanelLine>();
            networkLine.Add<UIPanelLineSectionText>().text.text = "Network Packet Data:";
            networkLine.SetExpandable((line =>
            {
                foreach (NetworkPacketData networkPacketData in GameManager.Instance.GetNetworkPacketDatas())
                {
                    UIStatCollectionPanelLine networkDetailLine = line.AddLine<UIStatCollectionPanelLine>();
                    networkDetailLine.SetStatCollection( 
                        networkPacketData.Stats,
                        networkPacketData.Type.ToString()
                    );
                    networkDetailLine.SetId(networkPacketData.Type.ToString());
                }
            }));
        
            
            
            
            UIStatCollectionPanelLine globalModifiersLine = AddLine<UIStatCollectionPanelLine>();
            globalModifiersLine.Add<UIPanelLineSectionText>().text.text = "Upgrades:";
            globalModifiersLine.SetExpandable((_globalModifiersLine =>
            {
                GameManager.Instance.Modifiers.Modifiers.Add(new ModifierBase()
                {
                    Group = ModifierBase.ModifierGroup.Release,
                    Target = ModifierBase.ModifierTarget.Run,
                    Type = ModifierBase.ModifierType.Run_Stat,
                    Id = "input_validation",
                    Name = "Input Validation",
                    StatType = StatType.Infra_InputValidation,
                    // BaseValue = 1.05f,
                    IconSpriteId = "IconCode"
                });
                GameManager.Instance.Modifiers.Modifiers[0].Apply();
                GameManager.Instance.Modifiers.Modifiers[0].LevelUp(Rarity.Common);
                GameManager.Instance.Modifiers.Render(_globalModifiersLine);
            }));



        }

        public void Preview(ModifierBase modifierBase)
        {
            
           Show();
           switch (modifierBase.Type)
           {
               case (ModifierBase.ModifierType.Run_Stat):
               case (ModifierBase.ModifierType.Run_Stat_Flat):

                   statCollectionPanelLine.Preview(modifierBase);
                   break;
               case (ModifierBase.ModifierType.Global_NetworkPacketStat):
                   if (!networkLine.IsExpanded())
                   {
                       networkLine.Expand();
                   }
                   Debug.Log($"networkLine.GetLines() {networkLine.GetLines().Count}");
                   foreach (UIPanelLine line in networkLine.GetLines())
                   {
                       Debug.Log($"{line.GetId()} == {modifierBase.NetworkPacketType.ToString()} => {line.GetId() == modifierBase.NetworkPacketType.ToString()}");
                       if (line.GetId() == modifierBase.NetworkPacketType.ToString())
                       {
                           (line as UIStatCollectionPanelLine).Preview(modifierBase);
                       } /*
                       else
                       {
                           (line as UIStatCollectionPanelLine).ResetText();
                       }*/
                   }

                   break;
               default:
                   throw new NotImplementedException();
           }
        }
    }
}