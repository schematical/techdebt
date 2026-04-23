using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace UI
{
    public class UIMetaUnlockOrgChartTab : UIMetaUnlockMapTabBase
    {
        public override string TabName => "Org Chart";

        public override void PopulateNodes(List<UIMapPanel.MapNodeView> mapNodes)
        {
            foreach (UIMetaUnlockMapLeveledNode node in GetOrgChartDefinitions())
            {
                SetNodeState(node);
                mapNodes.Add(new UIMapPanel.MapNodeView { Node = node });
            }
        }

        public override void UpdateDetailsArea()
        {
            _panel.CleanUp();
            
            MetaProgressData progress = MetaGameManager.GetProgress();
            
            UIPanelLine prestigeLine = _panel.AddLine<UIPanelLine>();
            prestigeLine.Add<UIPanelLineSectionText>().text.text = $"Vested Shares: {progress.prestigePoints}";

            UIMapPanel.MapNodeView selectedNode = _panel.GetSelectedNode();
            if (selectedNode == null)
            {
                _panel.AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = "Select a position on the Org Chart to view details.";
                return;
            }

            UIMetaUnlockMapLeveledNode mapLeveledNode = (UIMetaUnlockMapLeveledNode)selectedNode.Node;
            
            UIPanelLineSectionText header = _panel.AddLine<UIPanelLine>().Add<UIPanelLineSectionText>();
            header.h1(mapLeveledNode.DisplayName);
            
            _panel.AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = mapLeveledNode.Description;
            // _panel.AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = $"DEBUG:CurrentLevelIndex: {mapLeveledNode.CurrentLevelIndex}";
            
            if (mapLeveledNode.PrestigeCost > 0)
            {
                _panel.AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = $"\nCost: {mapLeveledNode.PrestigeCost} Vested Shares";
            }

            int currentLevelIdx = mapLeveledNode.CurrentLevelIndex;
            bool isMaxLevel = mapLeveledNode.Levels != null && currentLevelIdx == mapLeveledNode.Levels.Count - 1;
 
            bool canAfford = progress.prestigePoints >= mapLeveledNode.PrestigeCost;
            bool readyToUnlock = mapLeveledNode.CurrentState == MapNodeState.Locked;

            int i = 0;
            foreach (UIMetaUnlockLevelData level in mapLeveledNode.Levels)
            {
                UIPanelLine levelLine = _panel.AddLine<UIPanelLine>();
                Color color = Color.grey;
            
                if (currentLevelIdx == i)
                {
                    color = Color.white;
                }
                else if (currentLevelIdx < i)
                {
                    color = Color.darkGray;
                }
                

                UIPanelLineSectionText titleText = levelLine.Add<UIPanelLineSectionText>();
                titleText.h2( $"Level {i + 1}: {level.DisplayName}");
                titleText.text.color = color;

                UIPanelLineSectionText descText = _panel.AddLine<UIPanelLine>().Add<UIPanelLineSectionText>();
                descText.text.text = $"{level.Description}";
                descText.text.color = color;
                
                i++;
            }
            
          

            // Action Buttons
            if (!isMaxLevel)
            {
    
                if (readyToUnlock)
                {
                    if (canAfford)
                    {
                        string hireText = "Hire";
                        if (currentLevelIdx >= 0)
                        {
                            hireText = "Promote";
                        }
                        _panel.AddButton(hireText, () =>
                        {
                            UIMetaUnlockLevelData nextLevel = mapLeveledNode.Levels[currentLevelIdx + 1];
                            MetaGameManager.ToggleResourceEquip(mapLeveledNode.ResourceType, nextLevel.Id, nextLevel.PrestigeCost, nextLevel.StatType, nextLevel.Value);
                            _panel.Refresh();
                        });
                    }
                    else
                    {
                        _panel.AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = "<color=red>NOT ENOUGH VESTED SHARES</color>";
                    }
                }
                else
                {
                    _panel.AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = "<color=red>PREREQUISITES NOT MET</color>";
                }
            }

            if (currentLevelIdx > -1)
            {
                string demoteText = "Remove";
                if (currentLevelIdx > 0)
                {
                    demoteText = "Demote";
                }
                _panel.AddButton(demoteText, () =>
                {
                    UIMetaUnlockLevelData currentLevel = mapLeveledNode.Levels[currentLevelIdx];
                    MetaGameManager.ToggleResourceEquip(mapLeveledNode.ResourceType, currentLevel.Id, currentLevel.PrestigeCost, currentLevel.StatType, currentLevel.Value);
                    _panel.Refresh();
                });
            }
        }

     

        private List<UIMetaUnlockMapLeveledNode> GetOrgChartDefinitions()
        {
            List<UIMetaUnlockMapLeveledNode> nodes = new List<UIMetaUnlockMapLeveledNode>();

            // Root Node: CEO
            nodes.Add(new UIMetaUnlockMapLeveledNode
            {
                ResourceType = MetaResourceType.GlobalStatBaseStat,
                Id = "OrgChart_CEO",
                DisplayName = "CEO",
                Description = "",
                Direction = MapNodeDirection.Down,
                DependencyIds = new List<string>(),
                CurrentState = MapNodeState.Unlocked
            });

            // Branch: Marketing
            nodes.Add(new UIMetaUnlockMapLeveledNode
            {
                ResourceType = MetaResourceType.GlobalStatBaseStat,
                Id = "OrgChart_Marketing",
                DisplayName = "Marketing",
                Direction = MapNodeDirection.Down,
                DependencyIds = new List<string> { "OrgChart_CEO" },
                Levels = new List<UIMetaUnlockLevelData>
                {
                    new UIMetaUnlockLevelData { Id = "OrgChart_Marketing_1", DisplayName = "Marketing Intern", Description = "Unlock beginner marketing missions at the Product Road Map", PrestigeCost = 1 },
                    new UIMetaUnlockLevelData { Id = "OrgChart_Marketing_2", DisplayName = "Director Of Marketing", Description = "Unlock intermediate marketing missions at the Product Road Map", PrestigeCost = 2 },
                    new UIMetaUnlockLevelData { Id = "OrgChart_Marketing_3", DisplayName = "CMO", Description = "Unlock advanced marketing missions at the Product Road Map", PrestigeCost = 4 }
                }
            });

         
            nodes.Add(new UIMetaUnlockMapLeveledNode
            {
                ResourceType = MetaResourceType.GlobalStatBaseStat,
                Id = "OrgChart_Technology",
                DisplayName = "Technology",
                Direction = MapNodeDirection.Down,
                DependencyIds = new List<string> { "OrgChart_CEO" },
                Levels = new List<UIMetaUnlockLevelData>
                {
                    new UIMetaUnlockLevelData { Id = "OrgChart_Dev_1", DisplayName = "Dev Intern", Description = "Unlock beginner technology missions at the Product Road Map", PrestigeCost = 1 },
                    new UIMetaUnlockLevelData { Id = "OrgChart_Dev_2", DisplayName = "Senior Dev", Description = "Unlock intermediate technology missions at the Product Road Map", PrestigeCost = 3 },
                    new UIMetaUnlockLevelData { Id = "OrgChart_Dev_3", DisplayName = "CTO", Description = "Unlock advanced technology missions at the Product Road Map", PrestigeCost = 5 }
                }
            });

            // Branch: Finance
            nodes.Add(new UIMetaUnlockMapLeveledNode
            {
                ResourceType = MetaResourceType.GlobalStatBaseStat,
                Id = "OrgChart_Finance",
                DisplayName =  "Finance",
                Direction = MapNodeDirection.Down,
                DependencyIds = new List<string> { "OrgChart_CEO" },
                Levels = new List<UIMetaUnlockLevelData>
                {
                    new UIMetaUnlockLevelData { Id = "OrgChart_Finance_1", DisplayName = "Office Assistant", Description = "Unlock beginner financial missions at the Product Road Map", PrestigeCost = 1 },
                    new UIMetaUnlockLevelData { Id = "OrgChart_Finance_2", DisplayName = "Accountant", Description = "Unlock intermediate financial missions at the Product Road Map", PrestigeCost = 2 },
                    new UIMetaUnlockLevelData { Id = "OrgChart_Finance_3", DisplayName = "CFO", Description = "Unlock advanced financial missions at the Product Road Map", PrestigeCost = 4 }
                }
            });

            // Branch: Security
            nodes.Add(new UIMetaUnlockMapLeveledNode
            {
                ResourceType = MetaResourceType.GlobalStatBaseStat,
                Id = "OrgChart_Security",
                DisplayName = "Info Security",
                Direction = MapNodeDirection.Down,
                DependencyIds = new List<string> { "OrgChart_CEO" },
                Levels = new List<UIMetaUnlockLevelData>
                {
                    new UIMetaUnlockLevelData { Id = "OrgChart_Security_1", DisplayName = "Cyber Security Intern", Description = "Unlock beginner cyber security missions at the Product Road Map", PrestigeCost = 1 },
                    new UIMetaUnlockLevelData { Id = "OrgChart_Security_2", DisplayName = "Security Consultant", Description = "Unlock intermediate cyber security missions at the Product Road Map", PrestigeCost = 3 },
                    new UIMetaUnlockLevelData { Id = "OrgChart_Security_3", DisplayName = "CISO", Description = "Unlock advanced cyber security missions at the Product Road Map", PrestigeCost = 5 }
                }
            });

            return nodes;
        }
    }


}