using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class UIOrgChartPanel : UIMapPanel
    {
        [Header("OrgChart Details Area")]
        public TextMeshProUGUI stakeholderNameText;
        public TextMeshProUGUI stakeholderLevelText;
        public TextMeshProUGUI stakeholderDescriptionText;
        public UIButton upgradeButton;

        public override void PopulateNodes()
        {
            if (GameManager.Instance.Stakeholders == null) return;

            foreach (var stakeholder in GameManager.Instance.Stakeholders)
            {
                var nodeView = new MapNodeView
                {
                    Node = stakeholder
                };
                _mapNodes.Add(nodeView);
            }
        }

        public override void UpdateDetailsArea()
        {
            if (_selectedNode == null || _selectedNode.Node is not Stakeholder stakeholder)
            {
                if (stakeholderNameText != null) stakeholderNameText.text = "Select a Stakeholder";
                if (stakeholderLevelText != null) stakeholderLevelText.text = "";
                if (stakeholderDescriptionText != null) stakeholderDescriptionText.text = "";
                if (upgradeButton != null) upgradeButton.gameObject.SetActive(false);
                return;
            }

            if (stakeholderNameText != null) stakeholderNameText.text = stakeholder.DisplayName;
            
            if (stakeholderLevelText != null)
            {
                stakeholderLevelText.text = $"Level: {stakeholder.CurrentLevelIndex + 1}";
            }

            if (stakeholderDescriptionText != null)
            {
                stakeholderDescriptionText.text = stakeholder.Description;
            }

            if (upgradeButton != null)
            {
                upgradeButton.gameObject.SetActive(true);
                
                if (stakeholder.CanUpgrade())
                {
                    upgradeButton.button.interactable = true;
                    upgradeButton.buttonText.text = "Upgrade";
                    
                    upgradeButton.button.onClick.RemoveAllListeners();
                    upgradeButton.button.onClick.AddListener(() =>
                    {
                        stakeholder.Upgrade();
                        UpdateDetailsArea(); // Refresh details after upgrade
                        Refresh(); // Refresh map to update icons/state
                    });
                }
                else
                {
                    upgradeButton.button.interactable = false;
                    upgradeButton.buttonText.text = "Max Level / Cannot Upgrade";
                }
            }
        }
    }
}