using System.Collections.Generic;
using NPCs;
using Stats;
using Unity.VisualScripting;
using UnityEngine;

namespace UI
{
    public class UIGlobalStatsPanel: UIPanel
    {
        public UITextArea textArea;
        public UIStatCollectionListPanel detailPanel;
        public List<UIStatCollectionListPanel> networkPacketPanels =  new List<UIStatCollectionListPanel>(); 

        void Start()
        {
           
        }
        public override void Show()
        {
            foreach (UIStatCollectionListPanel statPanel in networkPacketPanels)
            {
                statPanel.gameObject.SetActive(false);
            }
            base.Show();
            if (detailPanel == null)
            {
                detailPanel =
                    GameManager.Instance.prefabManager.Create("UIStatCollectionListPanel", Vector3.zero, scrollContent)
                        .GetComponent<UIStatCollectionListPanel>();
            }
            detailPanel.Initialize(GameManager.Instance.Stats);
           
            foreach (NetworkPacketData networkPacketData in GameManager.Instance.GetNetworkPacketDatas())
            {
                UIStatCollectionListPanel networkPacketPanel =
                    GameManager.Instance.prefabManager.Create("UIStatCollectionListPanel", Vector3.zero, scrollContent)
                        .GetComponent<UIStatCollectionListPanel>();
                networkPacketPanel.Initialize(
                    networkPacketData.Stats,
                    networkPacketData.Type.ToString()
                );
                
                networkPacketPanels.Add(networkPacketPanel);
            }
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
            /*textArea.transform.SetAsLastSibling();
            var sb = new System.Text.StringBuilder();
    
            sb.AppendLine("<b>Modifiers</b>");
            foreach (ModifierBase modifier in GameManager.Instance.Modifiers.Modifiers)
            {
                sb.AppendLine(modifier.ToFullDetail());
            }
            textArea.textArea.text = sb.ToString();*/
 
           
        }
    }
}