using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace UI
{
    public class UIMetaUnlockOrgChartTab : UIMetaUnlockMapTabBase
    {
        public override string TabName => "OrgChart";

        public override void PopulateNodes(List<UIMapPanel.MapNodeView> mapNodes)
        {
            foreach (UIMetaUnlockMapNode node in GetOrgChartDefinitions())
            {
                mapNodes.Add(new UIMapPanel.MapNodeView { Node = node });
            }
        }

        public override void UpdateDetailsArea()
        {
            _panel.CleanUp();
            
            MetaProgressData progress = MetaGameManager.LoadProgress();
            
            UIPanelLine prestigeLine = _panel.AddLine<UIPanelLine>();
            prestigeLine.Add<UIPanelLineSectionText>().text.text = $"Vested Shares: {progress.prestigePoints}";

            var selectedNode = _panel.GetSelectedNode();
            if (selectedNode == null)
            {
                _panel.AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = "Select a position on the Org Chart to view details.";
                return;
            }

            UIMetaUnlockMapNode mapNode = (UIMetaUnlockMapNode)selectedNode.Node;
            
            UIPanelLineSectionText header = _panel.AddLine<UIPanelLine>().Add<UIPanelLineSectionText>();
            header.h1(mapNode.DisplayName);
            
            _panel.AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = mapNode.Description;
            
            if (mapNode.PrestigeCost > 0)
            {
                _panel.AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = $"\nCost: {mapNode.PrestigeCost} Vested Shares";
            }

            int currentLevelIdx = mapNode.CurrentLevelIndex;
            bool isMaxLevel = mapNode.Levels != null && currentLevelIdx == mapNode.Levels.Count - 1;
            bool isCEO = mapNode.Id == "OrgChart_CEO";

            string statusText = "STATUS: VACANT";
            if (isCEO) statusText = "STATUS: ACTIVE (FOUNDER)";
            else if (isMaxLevel) statusText = "STATUS: MAX LEVEL REACHED";
            else if (currentLevelIdx >= 0) statusText = $"STATUS: LEVEL {currentLevelIdx + 1} ACTIVE";
            
            _panel.AddLine<UIPanelLine>().Add<UIPanelLineSectionText>().text.text = $"\n{statusText}";

            // Action Buttons
            if (!isCEO && !isMaxLevel)
            {
                bool canAfford = progress.prestigePoints >= mapNode.PrestigeCost;
                bool readyToUnlock = mapNode.CurrentState == MapNodeState.Locked;

                if (readyToUnlock)
                {
                    if (canAfford)
                    {
                        _panel.AddButton("Hire / Promote", () =>
                        {
                            var nextLevel = mapNode.Levels[currentLevelIdx + 1];
                            MetaGameManager.ToggleResourceEquip(mapNode.ResourceType, nextLevel.Id, nextLevel.PrestigeCost, nextLevel.StatType, nextLevel.Value);
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

            if (!isCEO && currentLevelIdx >= 0)
            {
                _panel.AddButton("Step Down (Refund)", () =>
                {
                    var currentLevel = mapNode.Levels[currentLevelIdx];
                    MetaGameManager.ToggleResourceEquip(mapNode.ResourceType, currentLevel.Id, currentLevel.PrestigeCost, currentLevel.StatType, currentLevel.Value);
                    _panel.Refresh();
                });
            }
        }

        protected override UIMetaUnlockMapNode GetNodeById(string id)
        {
            return GetOrgChartDefinitions().Find(n => n.Id == id || (n.Levels != null && n.Levels.Any(l => l.Id == id)));
        }

        private List<UIMetaUnlockMapNode> GetOrgChartDefinitions()
        {
            List<UIMetaUnlockMapNode> nodes = new List<UIMetaUnlockMapNode>();

            // Root Node: CEO
            nodes.Add(new UIMetaUnlockMapNode
            {
                ResourceType = MetaResourceType.GlobalStat,
                Id = "OrgChart_CEO",
                DisplayName = "CEO",
                Description = "The founder and visionary leader of the company.",
                Direction = MapNodeDirection.Down,
                DependencyIds = new List<string>()
            });

            // Branch: Marketing
            nodes.Add(new UIMetaUnlockMapNode
            {
                ResourceType = MetaResourceType.GlobalStat,
                Id = "OrgChart_Marketing",
                Direction = MapNodeDirection.Left,
                DependencyIds = new List<string> { "OrgChart_CEO" },
                Levels = new List<UIMetaUnlockLevelData>
                {
                    new UIMetaUnlockLevelData { Id = "OrgChart_Marketing_1", DisplayName = "Marketing Intern", Description = "An eager intern to help spread the word.", PrestigeCost = 1 },
                    new UIMetaUnlockLevelData { Id = "OrgChart_Marketing_2", DisplayName = "Director Of Marketing", Description = "A seasoned professional to lead marketing campaigns.", PrestigeCost = 2 },
                    new UIMetaUnlockLevelData { Id = "OrgChart_Marketing_3", DisplayName = "CMO", Description = "Chief Marketing Officer. Total market dominance.", PrestigeCost = 4 }
                }
            });

            // Branch: Engineering
            nodes.Add(new UIMetaUnlockMapNode
            {
                ResourceType = MetaResourceType.GlobalStat,
                Id = "OrgChart_Engineering",
                Direction = MapNodeDirection.Right,
                DependencyIds = new List<string> { "OrgChart_CEO" },
                Levels = new List<UIMetaUnlockLevelData>
                {
                    new UIMetaUnlockLevelData { Id = "OrgChart_Dev_1", DisplayName = "Dev Intern", Description = "Someone to write the unit tests no one wants to write.", PrestigeCost = 1 },
                    new UIMetaUnlockLevelData { Id = "OrgChart_Dev_2", DisplayName = "Senior Dev", Description = "Solves complex architectural problems and mentors others.", PrestigeCost = 3 },
                    new UIMetaUnlockLevelData { Id = "OrgChart_Dev_3", DisplayName = "CTO", Description = "Chief Technology Officer. Visionary engineering leadership.", PrestigeCost = 5 }
                }
            });

            // Branch: Finance
            nodes.Add(new UIMetaUnlockMapNode
            {
                ResourceType = MetaResourceType.GlobalStat,
                Id = "OrgChart_Finance",
                Direction = MapNodeDirection.Down,
                DependencyIds = new List<string> { "OrgChart_CEO" },
                Levels = new List<UIMetaUnlockLevelData>
                {
                    new UIMetaUnlockLevelData { Id = "OrgChart_Finance_1", DisplayName = "Office Assistant", Description = "Keeps the office running and manages basic expenses.", PrestigeCost = 1 },
                    new UIMetaUnlockLevelData { Id = "OrgChart_Finance_2", DisplayName = "Accountant", Description = "Expertly manages taxes, payroll, and budget allocation.", PrestigeCost = 2 },
                    new UIMetaUnlockLevelData { Id = "OrgChart_Finance_3", DisplayName = "CFO", Description = "Chief Financial Officer. Maximizing shareholder value.", PrestigeCost = 4 }
                }
            });

            // Branch: Security
            nodes.Add(new UIMetaUnlockMapNode
            {
                ResourceType = MetaResourceType.GlobalStat,
                Id = "OrgChart_Security",
                Direction = MapNodeDirection.Down,
                DependencyIds = new List<string> { "OrgChart_CEO" },
                Levels = new List<UIMetaUnlockLevelData>
                {
                    new UIMetaUnlockLevelData { Id = "OrgChart_Security_1", DisplayName = "Cyber Security Intern", Description = "Checking logs and running vulnerability scanners.", PrestigeCost = 1 },
                    new UIMetaUnlockLevelData { Id = "OrgChart_Security_2", DisplayName = "Security Consultant", Description = "Implementing zero-trust architecture and hardening systems.", PrestigeCost = 3 },
                    new UIMetaUnlockLevelData { Id = "OrgChart_Security_3", DisplayName = "CISO", Description = "Chief Information Security Officer. Unbreakable infrastructure.", PrestigeCost = 5 }
                }
            });

            return nodes;
        }
    }
}