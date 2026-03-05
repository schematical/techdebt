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
       
        void Start()
        {
           
        }
        public override void Show()
        {
          
            base.Show();
            AddLine<UIStatCollectionPanelLine>().SetStatCollection(GameManager.Instance.Stats);
            UIStatCollectionPanelLine networkLine = AddLine<UIStatCollectionPanelLine>();
            networkLine.Add<UIPanelLineSectionText>().text.text = "Network Packet Data:";
            networkLine.SetExpandable((line =>
            {
                foreach (NetworkPacketData networkPacketData in GameManager.Instance.GetNetworkPacketDatas())
                {
                    line.AddLine<UIStatCollectionPanelLine>().SetStatCollection( 
                        networkPacketData.Stats,
                        networkPacketData.Type.ToString()
                    );
                }
            }));
        
            
            
            
            
            
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
            GameManager.Instance.Modifiers.Render(this);
       
 
           
        }
    }
}