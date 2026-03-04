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
        public UITextArea textArea;
        
        public List<UIStatCollectionPanelLine> networkPacketPanels =  new List<UIStatCollectionPanelLine>(); 

        void Start()
        {
           
        }
        public override void Show()
        {
            foreach (UIStatCollectionPanelLine statPanel in networkPacketPanels)
            {
                statPanel.gameObject.SetActive(false);
            }
            base.Show();
            AddLine<UIStatCollectionPanelLine>().SetStatCollection(GameManager.Instance.Stats);
           
            /*foreach (NetworkPacketData networkPacketData in GameManager.Instance.GetNetworkPacketDatas())
            {
                UIStatCollectionPanelLine networkPacketPanelLine =
                    GameManager.Instance.prefabManager.Create("UIStatCollectionPanelLine", Vector3.zero, scrollContent)
                        .GetComponent<UIStatCollectionPanelLine>();
                networkPacketPanelLine.SetStatCollection(
                    networkPacketData.Stats,
                    networkPacketData.Type.ToString()
                );
                
                networkPacketPanels.Add(networkPacketPanelLine);
            }*/
            
            
            
            
            
            /*
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
            */
       
 
           
        }
    }
}